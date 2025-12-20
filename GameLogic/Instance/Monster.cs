

using System;
using System.Collections.Generic;
using IdleCs.Library;
using IdleCs.Utils;

using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	/// <inheritdoc />
	/// <summary>
	/// monster
	/// </summary>
    public class Monster : Unit, ICorgiInterface<MonsterInfoSpec>
	{
		private MonsterGrade _monsterGrade;
		private MonsterInfoSpec _spec;
		
		public bool IsBoss => _monsterGrade == MonsterGrade.boss;

		public List<int> _skillStartSecs = new List<int>();
		public List<bool> _skillStartInstant = new List<bool>();

		public List<SkillActive> _instantSkills = new List<SkillActive>();

		public MonsterInfoSpec GetSpec()
		{
			return _spec;
		}
		
		/// <summary>
		/// static data
		/// </summary>
		public override float MoveSpeed 
		{
			get {
				if (IsBoss)
				{
					return 1f;
				}
				else
				{
					return 2f;
				}
			}
		}
		
		public override string Name { get { return _spec.Name; } }
		public override string CharName { get { return _spec.Name; } }
		public ulong GroupUid
		{
			get { return _spec.GroupUid; }
		}
		//public override string ModelName { get { return _spec.ModelName; } }
		//public override string PortraitName { get { return _spec.PortraitName; } }
		
        public Monster(Dungeon dungeon)
			: base(dungeon)
        {
        }

	    
        protected override bool LoadInternal(ulong uid)
	    {
            var sheet = Dungeon.GameData.GetData<MonsterInfoSpec>(uid);
	        if (sheet == null) { return false; }

	        _spec = sheet;
	        
	        _monsterGrade = CorgiEnum.ParseEnumPascal<MonsterGrade>(sheet.MonsterGrade);
	        
		    RoleType = RoleType.RtDealer;
		    CombatSideType = CombatSideType.Enemy;

		    AttackRange = sheet.AttackRange;

		    if (IsBoss)
		    {
			    UnitSize = 1.5f;
		    }
		    else
		    {
				UnitSize = 1f;
		    }
		    

		    //StatMap[StatType.StCritDmg].OrigStat =  50;
		    
			var attack = SkillFactory.Create(sheet.AttackUid, this) as SkillAttack;

			if (attack == null)
			{
				return false;
			}

			Attack = attack;

			// load attribute value
			var dungeonInstance = Dungeon as DungeonInstance;
			var attributeModifier = 1.0f;

			if (dungeonInstance != null)
			{
				attributeModifier = GetAttributeModifier(Dungeon.GameData, dungeonInstance.DungeonCriteriaType,
					dungeonInstance.CurStageUid, dungeonInstance.Grade);

			}
			
			AttributePowerMap[SkillAttributeType.SatFire] = (uint)(sheet.FireValue * attributeModifier);
			AttributePowerMap[SkillAttributeType.SatIce] = (uint)(sheet.IceValue * attributeModifier);
			AttributePowerMap[SkillAttributeType.SatLightning] = (uint)(sheet.LightningValue * attributeModifier);
			AttributePowerMap[SkillAttributeType.SatEarth] = (uint)(sheet.EarthValue * attributeModifier);
			AttributePowerMap[SkillAttributeType.SatHoly] = (uint)(sheet.HolyValue * attributeModifier);
			AttributePowerMap[SkillAttributeType.SatShadow] = (uint)(sheet.ShadowValue * attributeModifier);
			
			// load monster crazy skill if boss
			var crazySkillUidStr = GetCrazySkillUidStr();
			var crazySkillUid = GameDataManager.GetUidByString(crazySkillUidStr);
			if (IsBoss)
			{
				var curSkill = SkillFactory.Create(crazySkillUid, this);

				if (curSkill != null)
				{
					AddInstantSkill(curSkill as SkillActive, 60, true);
				}
				else
				{
					CorgiCombatLog.LogError(CombatLogCategory.Monster, "invalid boss crazy skill");
				}
				
				
			}

			// load monster skills from sheet
			if (sheet.SkillStartSecs.Count != sheet.SkillStartInstant.Count)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Monster,"invalid start sec & instant");
				return false;
			}

			var i = 0;
			var dummySkillUid = GameDataManager.GetUidByString("skill.adv.crazy.skill1");
			foreach (var skillUid in sheet.Skills)
			{
				if (skillUid == 0 || skillUid == crazySkillUid || skillUid == dummySkillUid)
				{
					continue;
				}
				
				var curSkill = SkillFactory.Create(skillUid, this);

				if (i < sheet.SkillStartSecs.Count)
				{
					var skillStartSec = (int)sheet.SkillStartSecs[i];
					var skillStartInstant = sheet.SkillStartInstant[i];

					if (skillStartSec == 0)
					{
                        AddSkill(curSkill);
						
					}
					else
					{
                        AddInstantSkill(curSkill as SkillActive, skillStartSec, skillStartInstant);
					}

				}
				else
				{
					AddSkill(curSkill);
				}

				i++;
			}
			
		    
		    return base.LoadInternal(uid);
	    }

        protected virtual string GetCrazySkillUidStr()
        {
	        return "skill.common.crazy"; 
        }
        
		protected void AddInstantSkill(SkillActive skill, int skillStartSec, bool skillStartInstant)
		{
			if (skill == null) { return; }

			// instant skill's manacost should be 0
			skill.ResetManaCost();
			
		    _skillStartSecs.Add((int)skillStartSec);
		    _skillStartInstant.Add(skillStartInstant);
			_instantSkills.Add(skill);
		}

		public override SkillActive GetNextActiveSkill()
		{
			//check Instant Skill
			SkillActive retSkill = GetInstantSkill();

			if (retSkill == null)
			{
				return base.GetNextActiveSkill();
			}
			return retSkill;
		}

		protected override SkillActive GetInstantSkill(bool isInstant = false)
		{
			SkillActive retSkill = null;
			// check instant skill
			for (var i = 0; i < _skillStartSecs.Count; i++)
			{
				var startSec = _skillStartSecs[i];
				var startInstant = _skillStartInstant[i];
				var instantSkill = _instantSkills[i];

				var startMilSec = (ulong)startSec * 1000;
				if (Dungeon.CurCombatTick >= startMilSec)
				{
					if ((isInstant == false)|| startInstant)
					{
                        retSkill = instantSkill;
                        
                        _skillStartSecs.RemoveAt(i);
                        _skillStartInstant.RemoveAt(i);
                        _instantSkills.RemoveAt(i);
					}
					
					break;
				}
			}

			return retSkill;
		}
		
        public virtual void SetLevel(uint level)
        {
	        Level = level;

	        var dungeon = Dungeon as DungeonInstance;
	        var gradeMod = 1f;
	        if (dungeon != null)
	        {
		        gradeMod += dungeon.MonsterMod;
	        }
	        
	        
	        var sheet = _spec;
	        
	        var constValueMaxHP = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.maxHP", 500);
	        var constValueAttackPower = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.attackPower", 500);
	        var constValueDefence = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.defence", 500);
	        var constValueHit = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.hit", 500);
	        var constValueEvasion = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.evasion", 500);
	        var constValueCrit = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.crit", 500);
	        var constValueResilience = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.resilience", 500);

	        StatMap[StatType.StMaxHp].OrigStat = (long)(GetStat(StatType.StMaxHp, sheet, Level, constValueMaxHP) * gradeMod);
	        StatMap[StatType.StAttackSpeed].OrigStat = GetStat(StatType.StAttackSpeed, sheet, Level, 0);
		    StatMap[StatType.StAttackPower].OrigStat = (long)(GetStat(StatType.StAttackPower, sheet, Level, constValueAttackPower) * gradeMod);
		    StatMap[StatType.StDefence].OrigStat = (long)(GetStat(StatType.StDefence, sheet, Level, constValueDefence) * gradeMod);
		    StatMap[StatType.StHit].OrigStat = (long)(GetStat(StatType.StHit, sheet, Level, constValueHit) * gradeMod);
		    StatMap[StatType.StEvasion].OrigStat = (long)(GetStat(StatType.StEvasion, sheet, Level, constValueEvasion) * gradeMod);
		    StatMap[StatType.StCrit].OrigStat = (long)(GetStat(StatType.StCrit, sheet, Level, constValueCrit) * gradeMod);
		    StatMap[StatType.StResilience].OrigStat =  (long)(GetStat(StatType.StResilience, sheet, Level, constValueResilience) * gradeMod);

		    CurHP = MaxHP;
        }

        public static long GetStat(StatType statType, MonsterInfoSpec sheet, uint level, int constValue )
        {
	        switch (statType)
	        {
		        case StatType.StMaxHp:
			        return CalcStat(sheet.MaxHP ,sheet.MaxHPMod, level, constValue);
		        case StatType.StAttackSpeed:
			        return sheet.AttackSpeed;
		        case StatType.StAttackPower:
					return CalcStat(sheet.AttackPower,sheet.AttackPowerMod, level, constValue);
		        case StatType.StDefence:
					return CalcStat(sheet.Defence, sheet.DefenceMod, level, constValue);
		        case StatType.StHit:
			        return CalcStat(sheet.Hit, sheet.HitMod, level, constValue);
		        case StatType.StEvasion:
			        return CalcStat(sheet.Evasion ,sheet.EvaisionMod, level, constValue);
		        case StatType.StCrit:
			        return CalcStat(sheet.Crit, sheet.CritMod, level, constValue);
		        case StatType.StResilience:
			        return CalcStat (sheet.Resilience , sheet.ResilienceMod ,level, constValue);
	        }

	        return 0;
        }

        static long CalcStat(long baseStat, float levelMod, uint level, int factor)
        {
	        if (level == 0)
	        {
		        return baseStat;
	        }
	        return (long)(baseStat + Math.Pow(level*levelMod*level/(level+5), (1+(level*level/(factor*(level-2.5))))));
        }

        public static ArtModelInfoSpec GetModelSpec(ulong uid)
        {
	        var spec = GameDataManager.Instance.GetData<MonsterInfoSpec>(uid);
	        if (spec == null || spec.ModelUid == 0)
	        {
		        return null;
	        }

	        var modelSpec = GameDataManager.Instance.GetData<ArtModelInfoSpec>(spec.ModelUid);
	        return modelSpec;
        }

        public static string GetModelName(ulong uid)
        {
			var spec =  GameDataManager.Instance.GetData<MonsterInfoSpec>(uid);
			if (spec == null || spec.ModelUid == 0)
			{
				return null;
			}

			var modelSpec = GameDataManager.Instance.GetData<ArtModelInfoSpec>(spec.ModelUid);
			if (modelSpec == null)
			{
				return null;
			}

			return modelSpec.ModelName;
        }
        public static float GetAttributeModifier(GameDataManager gamedata, DungeonCriteriaType dungeonType, ulong stageUid, uint grade=1 )
        {
	        if (dungeonType == DungeonCriteriaType.DctChapter)
	        {
		        var gradeStr = string.Format("instance_grade.{0}", grade);
		        var gradeSpec = gamedata.GetData<InstanceGradeInfoSpec>(gradeStr);
		        if (gradeSpec == null)
		        {
			        CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "Invalid grade uid : {0}", gradeStr);
			        return 1.0f;
		        }

		        var attributeModifier = 1.0f + gradeSpec.AttributeModifier;

		        return attributeModifier;
	        } else if (dungeonType == DungeonCriteriaType.DctRift)
			{
				var gradeStr = string.Format("rift_grade.{0}", grade);
				var gradeSpec = gamedata.GetData<InstanceGradeInfoSpec>(gradeStr);
				if (gradeSpec == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid rift grade uid : {0}", gradeStr);
					return 1.0f;
				}

				var attributeModifier = 1.0f + gradeSpec.AttributeModifier;

				return attributeModifier;

			}else if ( dungeonType == DungeonCriteriaType.DctLabyrinth)
			{
				var curStageUid = stageUid;
				var stageSpec = gamedata.GetData<InstanceLabyrinthInfoSpec>(curStageUid);

				if (stageSpec == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", curStageUid);
					return 1.0f;
				}

				var attributeModifier = 1.0f + stageSpec.AttributeModifier;

				return attributeModifier;
			}

			return 1.0f;
        }
    }
	
	public class MonsterTutorial : Monster
	{
		private DungeonTutorial _dungeonTutorial;
		
        public MonsterTutorial(Dungeon dungeon)
			: base(dungeon)
        {
	        _dungeonTutorial = dungeon as DungeonTutorial;
	        EventManager.Register(CombatEventType.OnBeingHit,  OnBeingHit);
        }

        protected override bool LoadInternal(ulong uid)
        {
	        if (base.LoadInternal(uid) == false)
	        {
		        return false;
	        }


	        return true;
        }
        
        protected override SkillActionLogNode DoSkill(SkillActive thisSkill, Unit target)
        {
	        var targetType = _dungeonTutorial.GetPauseTargetType();
	        if (targetType == PauseTargetType.PauseTarget_None)
	        {
		        return base.DoSkill(thisSkill, target);
	        }

	        if (targetType == PauseTargetType.PauseTarget_Knight && ClassType != ClassType.CtWarrior)
	        {
		        return null;
	        }
	        if (targetType == PauseTargetType.PauseTarget_Rogue && ClassType != ClassType.CtRogue)
	        {
		        return null;
	        }
	        if (targetType == PauseTargetType.PauseTarget_Mage && ClassType != ClassType.CtMage)
	        {
		        return null;
	        }
	        if (targetType == PauseTargetType.PauseTarget_Druid && ClassType != ClassType.CtDruid)
	        {
		        return null;
	        }
	        if (targetType == PauseTargetType.PauseTarget_Player && CombatSideType != CombatSideType.Player)
	        {
		        return null;
	        }
	        if (targetType == PauseTargetType.PauseTarget_Monster && CombatSideType != CombatSideType.Enemy)
	        {
		        return null;
	        }

	        return base.DoSkill(thisSkill, target);
        }

        public override void OnEnterCombat(CombatLogNode logNode)
        {
	        base.OnEnterCombat(logNode);
	        
	        var stage = Dungeon.CurStage as StageTutorial;
	        if (stage == null)
	        {
		        return;
	        }

	        var spec = stage.GetSpec();
	        if (spec.IsManaMax)
	        {
		        ResetMana(MaxMana);
	        }
        }
        bool OnBeingHit(EventParam eventParam, CombatLogNode logNode)
        {
	        var bossUid = GameDataManager.GetUidByString("tutorial.troll.1");
	        var spec = GetSpec();
	        if (spec.Uid == bossUid)
	        {
		        return OnBossTroll1(eventParam, logNode);
	        }
	        
	        var goblinUid = GameDataManager.GetUidByString("tutorial.goblin.1");
	        if (spec.Uid == goblinUid)
	        {
		        return OnMinionGoblin1(eventParam, logNode);
	        }
	        
	        return false;
        }

        bool OnBossTroll1(EventParam eventParam, CombatLogNode logNode)
        {
	        var skillComp = eventParam.GetSkillActor() as SkillComp;

	        if (skillComp == null)
	        {
		        return false;
	        }

	        var skill = skillComp.ParentActor as Skill;

	        if (skill == null || skill.GetSkillActionType() == SkillActionType.Attack)
	        {
		        return false;
	        }
	        var caster = eventParam.GetCaster();
	        if (caster.ClassType == ClassType.CtMage)
	        {
		        var damageLogNode = logNode as DamageSkillCompLogNode;
		        if (damageLogNode != null)
		        {
			        damageLogNode.Damage = this.CurHP;
		        }
		        //ResetHP(0);// kill 
		        return true;
	        }
	        return false;
	        
        }
        
        bool OnMinionGoblin1(EventParam eventParam, CombatLogNode logNode)
        {
	        var skillComp = eventParam.GetSkillActor() as SkillComp;

	        if (skillComp == null)
	        {
		        return false;
	        }

	        var skill = skillComp.ParentActor as Skill;

	        if (skill == null || skill.GetSkillActionType() == SkillActionType.Attack)
	        {
		        return false;
	        }
	        
			var damageLogNode = logNode as DamageSkillCompLogNode;
			if (damageLogNode != null)
			{
				damageLogNode.Damage = this.CurHP;
			}
			//ResetHP(0);// kill 
			return true;
	        
        }

	}

	public class MonsterTest : Monster
	{
		public override float MoveSpeed 
		{
			get { return 0f; }
		}
		
        public MonsterTest(Dungeon dungeon)
			: base(dungeon)
        {
        }

        
        protected override bool OnDeadCompletely(EventParam eventParam, CombatLogNode logNode)
        {
	        CurHP = 1;
	        return false;
        }
		
	}
}
