using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using UnityEngine;

namespace IdleCs.GameLogic
{
    public class Npc : Unit, ICorgiInterface<CharacterInfoSpec>
	{
		private CharacterInfoSpec _spec;

		public CharacterInfoSpec GetSpec()
		{
			return _spec;
		}
		
		/// <summary>
		/// static data
		/// </summary>
        public override string Name
        {
            get
            {
                if (CombatSideType == CombatSideType.Player)
                {
                    return base.Name + "_A";

                }
                else
                {
                    return base.Name + "_D";
                }
            }
        }
		
		public override string CharName { get { return _spec.CharName; } }
		public override string ModelName { get { return _spec.ModelName; } }
		public override string PortraitName { get { return _spec.Portrait; } }
		
		public override float MoveSpeed 
		{
			get { return 2.5f; }
		}

        public Npc(Dungeon dungeon)
			: base(dungeon)
        {
			SetReload();
        }

        protected override bool LoadInternal(ulong uid)
        {
            var sheet = Dungeon.GameData.GetData<CharacterInfoSpec>(uid);
	        if (sheet == null) { return false; }

	        _spec = sheet;
	        
		    var levelUidStr = string.Format("{0}.{1}", sheet.LevelPrefix ,Level);
		    var levelSheet = Dungeon.GameData.GetData<LevelInfoSpec>(levelUidStr);

		    if (levelSheet == null)
		    {
			    return false;
		    }

			ClassType = sheet.ClassType;
	        RoleType = sheet.RoleType;
	        
		    CombatSideType = CombatSideType.Player;

		    AttackRange = sheet.AttackRange;
		    UnitSize = 0.5f;
		    
		    StatMap[StatType.StMaxHp].OrigStat = levelSheet.MaxHP;
		    StatMap[StatType.StAttackSpeed].OrigStat = levelSheet.AttackSpeed;
		    StatMap[StatType.StAttackPower].OrigStat = levelSheet.AttackPower;
		    StatMap[StatType.StDefence].OrigStat = levelSheet.Defence;
		    StatMap[StatType.StHit].OrigStat = levelSheet.Hit;
		    StatMap[StatType.StEvasion].OrigStat = levelSheet.Evasion;
		    StatMap[StatType.StCrit].OrigStat = levelSheet.Crit;
		    StatMap[StatType.StResilience].OrigStat =  levelSheet.Resilience;
		    
		    StatMap[StatType.StCritDmg].OrigStat =  50;
		    
			var attack = SkillFactory.Create(sheet.AttackUid, this) as SkillAttack;

			if (attack == null)
			{
				return false;
			}

			Attack = attack;
			
	        // load skills 
	        var count = 0;
	        foreach (var curUid in sheet.InitialSkillUids)
	        {
				var skillItemInfo = Dungeon.GameData.GetData<SkillItemSpec>(curUid);

				if (skillItemInfo == null)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.NPC,"invalid skill item : {0}", curUid);
					 return false;
				}
				
				var skill = SkillFactory.Create(skillItemInfo.SkillUid, this) as SkillActive;
				if (skill == null)
				{
					continue;
				}
				
				AddActiveSkill(skill);

				count++;
				if (count >= 4)
				{
					break;
				}


	        }
	        
	        OnLoadInternal();
	        
	        return base.LoadInternal(uid);
        }
        
        /// <summary>
        /// called before load
        /// </summary>
        /// <param name="level"></param>
        public void SetLevel(uint level)
        {
	        Level = level;
	        
        }
        
        /// <summary>
        /// called before load
        /// </summary>
        /// <param name="level"></param>
        public void SetSkills(List<ulong> charSkillUids)
        {
	        var index = 0;
	        var thisSpec = GetSpec();

	        if (thisSpec == null)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.NPC,"cant load skill");
		        return;
	        }

	        ActiveSkills.Clear();
	        
	        foreach (var skillUid in charSkillUids)
	        {
		        var skillSheet = Dungeon.GameData.GetData<SkillInfoSpec>(skillUid);
		        if (skillSheet == null || skillSheet.SkillAttribute == SkillAttributeType.SatNone)
		        {
			        continue;
		        }

		        var attrIndex = (int) skillSheet.SkillAttribute;
		        var skillIndex = (attrIndex-1) * 4 + index;

		        if (skillIndex >= thisSpec.InitialSkillUids.Count)
		        {
					CorgiCombatLog.LogError(CombatLogCategory.NPC,"cant load skill index {0}", skillIndex);
					continue;
		        }

		        var curUid = thisSpec.InitialSkillUids[skillIndex];
		        
				var skillItemInfo = Dungeon.GameData.GetData<SkillItemSpec>(curUid);

				if (skillItemInfo == null)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.NPC,"invalid skill item : {0}", curUid);
					 continue;
				}
				
				var skill = SkillFactory.Create(skillItemInfo.SkillUid, this) as SkillActive;
				if (skill == null)
				{
					continue;
				}
				
				AddActiveSkill(skill);

				index++;


	        }
        }

        
        // called after load
        public void UpdateLevel(uint level)
        {
	        Level = level;
	        
	        var sheet = _spec;
	        
		    var levelUidStr = string.Format("{0}.{1}", sheet.LevelPrefix ,Level);
		    var levelSheet = Dungeon.GameData.GetData<LevelInfoSpec>(levelUidStr);

		    if (levelSheet == null)
		    {
			    return;
		    }
	        
		    StatMap[StatType.StMaxHp].OrigStat = levelSheet.MaxHP;
		    StatMap[StatType.StAttackSpeed].OrigStat = levelSheet.AttackSpeed;
		    StatMap[StatType.StAttackPower].OrigStat = levelSheet.AttackPower;
		    StatMap[StatType.StDefence].OrigStat = levelSheet.Defence;
		    StatMap[StatType.StHit].OrigStat = levelSheet.Hit;
		    StatMap[StatType.StEvasion].OrigStat = levelSheet.Evasion;
		    StatMap[StatType.StCrit].OrigStat = levelSheet.Crit;
		    StatMap[StatType.StResilience].OrigStat =  levelSheet.Resilience;
		    
		    StatMap[StatType.StCritDmg].OrigStat =  50;
        }
        
        void OnLoadInternal()
        {
	        uint maxPower = 0;
	        SkillAttributeType mainAttrType = SkillAttributeType.SatNone;
	        foreach (var curSkill in ActiveSkills)
	        {
		        if (curSkill == null)
		        {
			        continue;
		        }

		        var gradeSheet = Dungeon.GameData.GetData<SkillGradeSpec>("skill_grade." + ((int) curSkill.GradeType));
		        if (gradeSheet == null)
		        {
			        CorgiCombatLog.LogError(CombatLogCategory.NPC,"invalid grade sheet {0}", this.Uid);
			        continue;
		        }

		        AttributePowerMap[curSkill.AttributeType] += gradeSheet.AttributeValue;
	        }
        }

	    public void OnEnterStage()
	    {
	    }
	    
        public void SetCombatSide(CombatSideType sideType)
        {
            CombatSideType = sideType;
        }
    }
    
	public class NpcTutorial : Npc 
	{
		private DungeonTutorial _dungeonTutorial;
		
        public NpcTutorial(Dungeon dungeon)
			: base(dungeon)
        {
	        _dungeonTutorial = dungeon as DungeonTutorial;
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
	        var curSkillIndex = CurSkillIndex;
	        
	        base.OnEnterCombat(logNode);
	        
	        var stage = Dungeon.CurStage as StageTutorial;
	        if (stage == null)
	        {
		        return;
	        }

	        var spec = stage.GetSpec();
	        
	        if (spec.IsNextSkill)
	        {
		        _curSkillIndex = curSkillIndex - 1;
		        if (_curSkillIndex < 0)
		        {
			        _curSkillIndex = 0;
		        }
		        
				_curActiveSkill = GetNextActiveSkill();
				if (_curActiveSkill != null)
				{
					 _maxMana = _curActiveSkill.GetManaCost();
				}
	        }
	        if (spec.IsManaMax)
	        {
		        ResetMana(MaxMana);
	        }
        }

	}

	public class NpcArena : Npc
	{
		public NpcArena(Dungeon dungeon) : base(dungeon){}
	
		public override float MoveSpeed
		{
			get
			{
				if (UnitState == UnitState.Exploration) return (float)(1.5 * base.MoveSpeed);
				return base.MoveSpeed;
			}
		}
	}
}