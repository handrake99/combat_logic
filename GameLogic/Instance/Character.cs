
using System;
using System.Collections.Generic;
using Corgi.DBSchema;
using Corgi.GameData;
using Google.Protobuf;
using IdleCs.Library;

using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Utils;

using IdleCs.Managers;
using UnityEditor;


namespace IdleCs.GameLogic
{
	/// <inheritdoc />
	/// <summary>
	/// inherited from Unit
	/// </summary>
    public class Character : Unit, ICorgiInterface<CharacterInfoSpec>
	{
		private SharedCharInfo _charInfo;
		private CharacterInfoSpec _spec;
		
		/// <summary>
		/// static data
		/// </summary>
		public override string Name { get { return _spec.Name; } }
		public override string CharName { get { return _spec.CharName; } }
		public override string ModelName { get { return _spec.ModelName; } }
		public override string PortraitName { get { return _spec.Portrait; } }

		private int _grade = 0;
		public int Grade
		{
			get { return _grade; }
		}
		
		
		private List<SkillPassive> _talentSkills;
		public List<SkillPassive> TalentSkills { get {return _talentSkills; }}
		
		private List<SkillActive> _relicSkills;
		public List<SkillActive> RelicSkills { get {return _relicSkills; }}


		private List<SkillPassive> _equipMainSkills;
		public List<SkillPassive> EquipMainSkills { get {return _equipMainSkills; }}
		
		private List<SkillPassive> _equipSubSkills;
		public List<SkillPassive> EquipSubSkills { get {return _equipSubSkills; }}
		
		// binding passive
		private List<SkillPassive> _bindingStoneSkills;
		public List<SkillPassive> BindingSkills { get {return _bindingStoneSkills; }}

		private List<Equip> _equips;
		public List<Equip> Equips { get {return _equips; }}

		/// <summary>
		/// Dynamic Data
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private List<SkillEffectInst> _relicPreContinuousList = new List<SkillEffectInst>();
		private List<SkillEffectInst> _relicPostContinuousList = new List<SkillEffectInst>();
		
		
        public Skill GetSkill(int index)
        {
            if (index < CorgiLogicConst.CharacterSkillCount)
            {
                return ActiveSkills[index];
            }

            return null;
        }

        public CharacterInfoSpec GetSpec()
        {
	        return _spec;
        }
        
		public override float MoveSpeed 
		{
			get { return 2.5f; }
		}

        public Character(Dungeon dungeon)
			: base(dungeon)
        {
			_talentSkills = new List<SkillPassive>();
			_relicSkills = new List<SkillActive>();
			
			_equipMainSkills = new List<SkillPassive>();
			_equipSubSkills = new List<SkillPassive>();
			_bindingStoneSkills = new List<SkillPassive>();
			
			_equips = new List<Equip>();
			SetReload();
        }

        protected override void Initialize()
        {
	        base.Initialize();
	        
	        _equips.Clear();
	        _equipMainSkills.Clear();
	        _equipSubSkills.Clear();
	        _talentSkills.Clear();
	        _relicSkills.Clear();
        }
        
        protected override bool LoadInternal(IMessage dbObject)
        {
	        Initialize();
	        
	        var db = dbObject as DBCharacter;

	        if (db == null)
	        {
		        return false;
	        }
	        if (base.LoadInternal(dbObject) == false)
	        {
		        return false;
	        }
	        
            Uid = db.Uid;
	        DBId = db.Dbid;
	        
	        Level = (uint)db.Level;

	        // load sheet
	        if (LoadInternal(db.Uid) == false)
	        {
		        return false;
	        }

	        // load equips
	        var equipList = new List<string>()
		        {db.WeaponId, db.ArmorId, db.HelmetId, db.BootsId, db.GauntletId, db.CloakId};
	        if (LoadEquips(equipList) == false)
	        {
		        return false;
	        }
	        
	        // load general talent
	        StatMap[StatType.StMaxHp].AddStat = db.STMaxHP * GetCommonTalentStat(StatType.StMaxHp);
		    StatMap[StatType.StAttackSpeed].AddStat = db.STAttackSpeed * GetCommonTalentStat(StatType.StAttackSpeed);
		    StatMap[StatType.StAttackPower].AddStat = db.STAttackPower * GetCommonTalentStat(StatType.StAttackPower);
		    StatMap[StatType.StDefence].AddStat = db.STDefence * GetCommonTalentStat(StatType.StDefence);
		    StatMap[StatType.StHit].AddStat = db.STHit * GetCommonTalentStat(StatType.StHit);
		    StatMap[StatType.StEvasion].AddStat = db.STEvasion * GetCommonTalentStat(StatType.StEvasion);
		    StatMap[StatType.StCrit].AddStat = db.STCrit * GetCommonTalentStat(StatType.StCrit);
		    StatMap[StatType.StResilience].AddStat =  db.STResilience * GetCommonTalentStat(StatType.StResilience);
		    StatMap[StatType.StCritDmg].AddStat =  db.STCritDmg * GetCommonTalentStat(StatType.StCritDmg);

		    // load expert talent
	        foreach (var skillUid in db.ExpertTalents)
	        {
		        AddTalentSkill(skillUid);
	        }
	        
	        // load passive skills
	        var grade = db.Grade;
	        _grade = grade;

	        for (var i = 1; i <= grade; i++)
	        {
		        var uidStr = string.Format("{0}.{1}", _spec.GradePrefix, i);
		        var gradeSheet = Dungeon.GameData.GetData<CharacterGradeInfoSpec>(uidStr);
		        if (gradeSheet == null)
		        {
			        CorgiCombatLog.LogError(CombatLogCategory.Character,"invalid grade info {0}", uidStr);
			        continue;
		        }
		        AddPassiveSkill(gradeSheet.PassiveSkillUid);
	        }

	        
	        OnLoadInternal();
	        
	        return true;
        }

        protected override bool LoadInternal(CorgiSharedObject sObject)
        {
	        Initialize();
	        
	        var sharedObject = sObject as SharedCharInfo;
	        if (sharedObject == null)
	        {
		        return false;
	        }

	        if (base.LoadInternal(sObject) == false)
	        {
		        return false;
	        }

	        _charInfo = sharedObject;

	        Level = (uint)sharedObject.level;

			// load sheet
	        if (LoadInternal(_charInfo.uid) == false)
	        {
		        return false;
	        }
	        
//	        var settingDbId = sharedObject.defaultSettingId;
//	        if (string.IsNullOrEmpty(settingDbId))
//	        {
//		        return false;
//	        }
//
//	        if (LoadInternalCombatSetting(settingDbId) == false)
//	        {
//		        return false;
//	        }
	        
	        // load equips
	        var equipList = new List<string>()
		        {sharedObject.weaponId, sharedObject.armorId, sharedObject.helmetId, sharedObject.bootsId, sharedObject.gauntletId, sharedObject.cloakId};
	        
	        if (LoadEquips(equipList) == false)
	        {
		        return false;
	        }

	        // load general talent
	        StatMap[StatType.StMaxHp].AddStat = sharedObject.ST_MaxHP * GetCommonTalentStat(StatType.StMaxHp);
		    StatMap[StatType.StAttackSpeed].AddStat = sharedObject.ST_AttackSpeed * GetCommonTalentStat(StatType.StAttackSpeed);
		    StatMap[StatType.StAttackPower].AddStat = sharedObject.ST_AttackPower * GetCommonTalentStat(StatType.StAttackPower);
		    StatMap[StatType.StDefence].AddStat = sharedObject.ST_Defence * GetCommonTalentStat(StatType.StDefence);
		    StatMap[StatType.StHit].AddStat = sharedObject.ST_Hit * GetCommonTalentStat(StatType.StHit);
		    StatMap[StatType.StEvasion].AddStat = sharedObject.ST_Evasion * GetCommonTalentStat(StatType.StEvasion);
		    StatMap[StatType.StCrit].AddStat = sharedObject.ST_Crit * GetCommonTalentStat(StatType.StCrit);
		    StatMap[StatType.StResilience].AddStat =  sharedObject.ST_Resilience * GetCommonTalentStat(StatType.StResilience);
		    StatMap[StatType.StCritDmg].AddStat =  sharedObject.ST_CritDmg * GetCommonTalentStat(StatType.StCritDmg);
		    
	        // load expert talent
	        foreach (var skillUid in sharedObject.expertTalents)
	        {
		        AddTalentSkill(skillUid);
	        }
	        
	        // load passive skills
	        var grade = sharedObject.grade;
	        _grade = grade;

	        for (var i = 1; i <= grade; i++)
	        {
		        var uidStr = string.Format("{0}.{1}", _spec.GradePrefix, i);
		        var gradeSheet = Dungeon.GameData.GetData<CharacterGradeInfoSpec>(uidStr);
		        if (gradeSheet == null)
		        {
			        CorgiCombatLog.LogError(CombatLogCategory.Character,"invalid grade info {0}", uidStr);
			        continue;
		        }
		        AddPassiveSkill(gradeSheet.PassiveSkillUid);
	        }
	        
	        OnLoadInternal();

	        return true;
        }
        public bool LoadCombatSetting(Deck deck)
        {
            if (deck == null || deck.CharacterId != DBId)
            {
                return false;
            }

            ResetActiveSkills();
            ResetRelicSkills();
            ResetAttributeMap();
            
			CorgiCombatLog.Log(CombatLogCategory.Character, "[Character] === Load Deck ===");
            foreach (var activeSkill in deck.ActiveSkills)
            {
                AddActiveSkill(activeSkill);
                CorgiCombatLog.Log(CombatLogCategory.Character,"[Character] Skill {0} : Level {1}", activeSkill?.Name, activeSkill?.Level);
            }

        
            foreach (var relicSkill in deck.RelicSkills)
            {
                AddRelicSkill(relicSkill);
                CorgiCombatLog.Log(CombatLogCategory.Character, "[Character] Relic {0} : Level {1}", relicSkill?.Name, relicSkill?.Level);
            }
			CorgiCombatLog.Log(CombatLogCategory.Character, "[Character] === Load End  ===");
        
            
            foreach (var relicUid in deck.Relics)
            {
	            var relicSheet = Dungeon.GameData.GetData<RelicInfoSpec>(relicUid);
	            if (relicSheet == null)
	            {
		            continue;
	            }
	            
		        StatMap[relicSheet.MainStatType].AddStat += (int)relicSheet.MainStatValue;
            }
            
			for (var i= 0; i < CorgiLogicConst.CharacterSkillCount; i++)
			{
				 var activeSkill = ActiveSkills[i];

				 if (activeSkill == null)
				 {
					 continue;
				 }

				 var relicSkill = _relicSkills[i];
				 
				 if (relicSkill == null)
				 {
					  continue;
				 }

				 activeSkill.OnInvokeSkill = relicSkill.DoSkill;
			}
			
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
			        CorgiCombatLog.LogError(CombatLogCategory.Character,"invalid grade sheet {0}", this.Uid);
			        continue;
		        }

		        AttributePowerMap[curSkill.AttributeType] += gradeSheet.AttributeValue;
	        }
	        
            return true;
	        
        }
	    
        protected override bool LoadInternal(ulong uid)
        {
            var sheet = Dungeon.GameData.GetData<CharacterInfoSpec>(uid);
	        if (sheet == null) { return false; }

	        uint level = Level;
	        if (level > 200)
	        {
		        level = 200;
	        }
	        
		    var levelUidStr = string.Format("{0}.{1}", sheet.LevelPrefix ,level);
		    var levelSheet = Dungeon.GameData.GetData<LevelInfoSpec>(levelUidStr);

		    if (levelSheet == null)
		    {
			    return false;
		    }

	        _spec = sheet;

			ClassType = sheet.ClassType;
	        RoleType = sheet.RoleType;
	        
		    CombatSideType = CombatSideType.Player;

		    AttackRange = sheet.AttackRange;
		    UnitSize = 0.5f;

		    // main stat
		    StatMap[StatType.StMaxHp].OrigStat = levelSheet.MaxHP;
		    StatMap[StatType.StAttackSpeed].OrigStat = levelSheet.AttackSpeed;
		    StatMap[StatType.StAttackPower].OrigStat = levelSheet.AttackPower;
		    StatMap[StatType.StDefence].OrigStat = levelSheet.Defence;
		    StatMap[StatType.StHit].OrigStat = levelSheet.Hit;
		    StatMap[StatType.StEvasion].OrigStat = levelSheet.Evasion;
		    StatMap[StatType.StCrit].OrigStat = levelSheet.Crit;
		    StatMap[StatType.StResilience].OrigStat =  levelSheet.Resilience;
		    StatMap[StatType.StCritDmg].OrigStat =  0;
		    
		    // second stat
			var attack = SkillFactory.Create(sheet.AttackUid, this) as SkillAttack;

			if (attack == null)
			{
				return false;
			}

			Attack = attack;
	        
		    
	        return base.LoadInternal(uid);
        }
        
        void OnLoadInternal()
        {
	        uint maxPower = 0;

	        // equip stat
	        foreach (var curEquip in Equips)
	        {
		        if (curEquip == null)
		        {
			        continue;
		        }

		        foreach (var statStruct in curEquip.MainStats)
		        {
			        if (statStruct.StatType == StatType.StNone)
			        {
				        continue;
			        }
					StatMap[statStruct.StatType].AddStat += (int)statStruct.StatFinal;
		        }
		        foreach (var statStruct in curEquip.SubStats)
		        {
			        if (statStruct.StatType == StatType.StNone)
			        {
				        continue;
			        }
					StatMap[statStruct.StatType].AddStat += (int)statStruct.StatFinal;
		        }
	        }
	        
	        // almanac stat
	        var bridge = Dungeon.Bridge;
	        var almanacStat = bridge.GetAlmanacStat(this);
	        if (almanacStat != null)
	        {
				StatMap[StatType.StMaxHp].AddStat += (long)almanacStat.ST_MaxHP;
				//StatMap[StatType.StAttackSpeed].AddStat += (long) almanacStat.ST_AttackSpeed;
				StatMap[StatType.StAttackPower].AddStat += (long) almanacStat.ST_AttackPower;
				StatMap[StatType.StDefence].AddStat += (long) almanacStat.ST_Defence;
				StatMap[StatType.StHit].AddStat += (long) almanacStat.ST_Hit;
				StatMap[StatType.StEvasion].AddStat += (long) almanacStat.ST_Evasion;
				StatMap[StatType.StCrit].AddStat += (long) almanacStat.ST_Crit;
				StatMap[StatType.StResilience].AddStat += (long) almanacStat.ST_Resilience;
				StatMap[StatType.StCritDmg].AddStat += (long) almanacStat.ST_CritDmg;
	        }

	        var bindingStones = bridge.GetBindingStones(this);
	        if (bindingStones != null)
	        {
		        foreach (var bindingStone in bindingStones)
		        {
			        StatMap[bindingStone.StatType].AddStat += bindingStone.GetStat();
		        }
	        }
	        
	        //Attack.SetSkillAttributeType(mainAttrType);
	        
	        CurHP = MaxHP;
        }

        bool LoadEquips(List<string> equipIds)
        {
	        _equips.Clear();
	        _equipMainSkills.Clear();
	        _equipSubSkills.Clear();
	        
	        var bridge = Dungeon.Bridge;

	        foreach (var equipId in equipIds)
	        {
		        if (string.IsNullOrEmpty(equipId))
		        {
			        continue;
		        }

		        var equip = bridge.CreateEquip(this, equipId);

		        if (equip == null)
		        {
			        continue;
		        }
		        _equips.Add(equip);
		        
		        // main sub skill
		        if (equip.MainSkillUid != 0)
		        {
			        var skill = SkillFactory.Create(equip.MainSkillUid, this) as SkillPassive;

			        if (skill == null)
			        {
				        CorgiCombatLog.LogError(CombatLogCategory.Character," ({0}) Invalid Equip Main Skill ({1})", Uid,
					        equip.MainSkillUid);
			        }
			        else
			        {
				        skill.SetLevel(1); // 1레벨 고정
				        _equipMainSkills.Add(skill);
			        }
		        }
		        
		        if (equip.SubSkillUid != 0 && equip.IsMaxLevel)
		        {
			        var skill = SkillFactory.Create(equip.SubSkillUid, this) as SkillPassive;

			        if (skill == null)
			        {
				        CorgiCombatLog.LogError(CombatLogCategory.Character," ({0}) Invalid Equip Unitque Skill ({1})", Uid,
					        equip.SubSkillUid);
			        }
			        else
			        {
				        skill.SetLevel(1); // 1레벨 고정
				        _equipSubSkills.Add(skill);
			        }
		        }
	        }

	        return true;
        }

	    public void OnEnterStage()
	    {
	    }
	    
	    protected override SkillActionLogNode DoAttack(Unit target)
	    {
			CorgiCombatLog.Log(CombatLogCategory.Relic, "DoAttack Relic State Pre({0}) <> Post({1})", _relicPreContinuousList.Count, _relicPostContinuousList.Count);

		    return base.DoAttack(target);
	    }

	    protected override SkillActionLogNode DoSkill(SkillActive thisSkill, Unit target)
	    {
		    SkillActionLogNode relicLogNode = null;

			CorgiCombatLog.Log(CombatLogCategory.Relic,"DoSkill 1 Relic State Pre({0}) <> Post({1})", _relicPreContinuousList.Count, _relicPostContinuousList.Count);
		    var actionLogNode = base.DoSkill(thisSkill, target);
		    
			CorgiCombatLog.Log(CombatLogCategory.Relic," DoSkill 2 Relic Continuous Moved Pre({0})->Post({1})", _relicPreContinuousList.Count, _relicPostContinuousList.Count);
			foreach (var relicCon in _relicPreContinuousList)
			{
				 CorgiCombatLog.Log(CombatLogCategory.Relic,"Remove Continuous {0}", relicCon.Name);
			}

			var beforePreCount = _relicPreContinuousList.Count;
			var beforePostCount = _relicPostContinuousList.Count;
			
			_relicPreContinuousList.Clear();
			_relicPreContinuousList.AddRange(_relicPostContinuousList);
			_relicPostContinuousList.Clear();

			CorgiCombatLog.Log(CombatLogCategory.Relic,"DoSkill 3 Relic Continuous Moved Pre({0})->Post({1})", _relicPreContinuousList.Count, _relicPostContinuousList.Count);

			var afterPreCount = _relicPreContinuousList.Count;
			var afterPostCount = _relicPostContinuousList.Count;
			
			if (beforePreCount != afterPreCount || beforePostCount != afterPostCount)
			{
				_isUpdatedEffect = true;
				OnUpdateEffect(actionLogNode);
			}
			
		    return actionLogNode;
	    }

	    public override void OnSkill(SkillActive thisSkill, CombatLogNode logNode)
	    {
			_isUpdatedEffect = true;
		    
		    base.OnSkill(thisSkill, logNode);
	    }

	    int GetCommonTalentStat(StatType statType)
	    {
		    var uidPrefix = "common_talent_stat.";
		    for (int i = 0; i < 8; i++)
		    {
			    var uidStr = uidPrefix + (i+1);
			    var sheet = Dungeon.GameData.GetData<CommonTalentStatInfoSpec>(uidStr);
			    if (sheet == null)
			    {
				    continue;
			    }

			    if (sheet.StatType == statType)
			    {
				    return (int)sheet.StatValue;
			    }
		    }

		    return 0;
	    }

	    public override void OnEnterCombat(CombatLogNode logNode)
	    {
	        // reset binding stones
	        ResetBindingSkills();
	        
	        var bindingStones = Dungeon.Bridge.GetBindingStones(this);
	        if (bindingStones != null)
	        {
		        foreach (var bindingStone in bindingStones)
		        {
			        // add binding stone passive 
			        var skillUid = bindingStone.GetPassiveUid(Dungeon);
					AddBindingSkill(skillUid, bindingStone.Level);
		        }
	        }
	        
	        base.OnEnterCombat(logNode);
	    }

	    protected override void OnEnterCombatForSkill(CombatLogNode logNode)
	    {
		    base.OnEnterCombatForSkill(logNode);

		    var eventParam = new EventParamUnit(this);

		    foreach (var skill in TalentSkills)
		    {
			    skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }
		    
		    foreach (var skill in EquipMainSkills)
		    {
			    skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }
		    foreach (var skill in EquipSubSkills)
		    {
			    skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }
		    
		    foreach (var skill in BindingSkills)
		    {
			    skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }

		    for (var i = 0; i < ActiveSkills.Count; i++)
		    {
			    var activeSkill = ActiveSkills[i];
			    var relicSkill = RelicSkills[i];

			    if (activeSkill == null || relicSkill == null)
			    {
				    continue;
			    }
			    
			    activeSkill.SetChanagedManaCost(relicSkill.GetManaCost());
		    }

		    foreach (var skill in RelicSkills)
		    {
			    skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }
	    }

	    protected void AddPassiveSkill(ulong skillUid, uint level=1)
		{
			var bridge = Dungeon.Bridge;
			if (skillUid == 0)
			{
				return;
			}
			
            var thisSkill = bridge.CreateSkill(this, skillUid) as SkillPassive;
            if (thisSkill == null)
            {
                throw new CorgiException("invalid talent skill uid {0}", skillUid);
            }
            thisSkill.SetLevel(level);
            
            AddPassiveSkill(thisSkill);
	    }
	    
	    protected void AddBindingSkill(ulong skillUid, uint level=1)
		{
			var bridge = Dungeon.Bridge;
			if (skillUid == 0)
			{
				return;
			}
			
            var thisSkill = bridge.CreateSkill(this, skillUid) as SkillPassive;
            if (thisSkill == null)
            {
                throw new CorgiException("invalid talent skill uid {0}", skillUid);
            }
            thisSkill.SetLevel(level);
            
            AddBindingSkill(thisSkill);
	    }
	    
		void AddTalentSkill(ulong skillUid)
		{
			var bridge = Dungeon.Bridge;
			
			SkillPassive thisSkill = null;
			foreach (var curSkill in _talentSkills)
			{
				if (curSkill == null)
				{
					continue;
				}

				if (curSkill.Uid == skillUid)
				{
					thisSkill = curSkill;
					break;
				}
			}

			if (thisSkill == null)
			{
				// new talent passive skill
				thisSkill = bridge.CreateSkill(this, skillUid) as SkillPassive;
				if (thisSkill == null)
				{
					throw new CorgiException("invalid talent skill uid {0}", skillUid);
				}
				
				_talentSkills.Add(thisSkill);
			}
			else
			{
				// old talent passive skill - add level
				thisSkill.AddLevel();
			}
			
		}

		void AddRelicSkill(SkillActive relicSkill)
		{
			_relicSkills.Add(relicSkill);
		}
		
		protected void ResetRelicSkills()
		{
			_relicSkills.Clear();
		}
		
		
		public SkillActive GetCurRelicSkill()
		{
			if (_relicSkills.Count == 0)
			{
				return null;
			}

			var skillIndex = CurSkillIndex - 1;
			if (skillIndex < 0)
			{
				skillIndex = _relicSkills.Count - 1;
			}
			
			return _relicSkills[skillIndex];
		}
		
		protected void AddBindingSkill(SkillPassive skill)
		{
			if (skill == null) { return; }
			_bindingStoneSkills.Add(skill);
		}
		
		protected void ResetBindingSkills()
		{
			_bindingStoneSkills.Clear();
		}

		public override void ApplySkillEffect(SkillEffectInst inst, CombatLogNode logNode)
		{
			if (inst == null)
			{
				return;
			}

			if (inst.EffectInstType == SkillEffectInstType.Relic)
			{
				_relicPostContinuousList.Add(inst);
				CorgiCombatLog.Log(CombatLogCategory.Relic,"Add Continuous {0}", inst.Name);
				_isUpdatedEffect = true;
				
				logNode.AddDetailLog($"Apply Relic Continuous {inst.Name}/{inst.StackCount}/{inst.MaxStack}");
			}
			else
			{
				base.ApplySkillEffect(inst, logNode);
			}
		}

		protected override void DoSkillEffectAction(Action<SkillEffectInst> action)
		{
			base.DoSkillEffectAction(action);

			foreach (var skillInst in _relicPreContinuousList)
			{
				if (skillInst == null)
				{
					continue;
				}
			
				var curSkill = CurAction;
				if (curSkill == null || curSkill.SkillType == SkillType.Relic)
				{
					continue;
				}
				
				skillInst.DoSkillEffectAction(this, action);
			}
			
			foreach (var skillInst in _relicPostContinuousList)
			{
				if (skillInst == null)
				{
					continue;
				}
			
				var curSkill = CurAction;
				if (curSkill == null || curSkill.SkillType == SkillType.Relic)
				{
					continue;
				}
				
				skillInst.DoSkillEffectAction(this, action);
			}
		}
		
		public override void DoSkillEffectAuraAction(Unit target, Action<SkillEffectInst> action)
		{
			base.DoSkillEffectAuraAction(target, action);
			
			for(var i=0; i<_relicPreContinuousList.Count; i++)
			{
				var inst = _relicPreContinuousList[i];
				var auraInst = inst as SkillEffectAuraInst;
				if (auraInst == null)
				{
					continue;
				}
				inst.DoSkillEffectAction(target, action);
			}
			for(var i=0; i<_relicPostContinuousList.Count; i++)
			{
				var inst = _relicPostContinuousList[i];
				var auraInst = inst as SkillEffectAuraInst;
				if (auraInst == null)
				{
					continue;
				}
				inst.DoSkillEffectAction(target, action);
			}
		}
		
        public static ArtModelInfoSpec GetModelSpec(ulong uid, int grade)
        {
	        var spec = GameDataManager.Instance.GetData<CharacterInfoSpec>(uid);
	        if (spec == null)
	        {
		        return null;
	        }
	        
			var uidStr = string.Format("{0}.{1}", spec.GradePrefix, grade);
			var gradeSpec = GameDataManager.Instance.GetData<CharacterGradeInfoSpec>(uidStr);
			if (gradeSpec == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Character,"invalid grade info {0}", uidStr);
				return null;
			}

	        var modelSpec = GameDataManager.Instance.GetData<ArtModelInfoSpec>(gradeSpec.ModelUid);
			if (modelSpec == null)
			{
				CorgiCombatLog.Log(CombatLogCategory.Character,"invalid model info {0}", gradeSpec.ModelUid);
				return null;
			}
	        
	        return modelSpec;
        }
		
        public static string GetModelName(ulong uid)
        {
			var spec =  GameDataManager.Instance.GetData<CharacterInfoSpec>(uid);
			if (spec == null)
			{
				return null;
			}
			
//			var gradeSpec = GameDataManager.Instance.GetData<CharacterGradeInfoSpec>(spec.G)
//
//			var modelSpec = GameDataManager.Instance.GetData<ArtModelInfoSpec>(spec.ModelUid);
//			if (modelSpec == null)
//			{
//				return null;
//			}
//
//			return modelSpec.ModelName;
	        return null;
        }

    }
	
	public class CharacterTutorial : Character
	{
		private DungeonTutorial _dungeonTutorial;
		
        public CharacterTutorial(Dungeon dungeon)
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
	        
	        
	        // tutorial code
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

	public class CharacterTest : Character
	{
        public CharacterTest(Dungeon dungeon)
			: base(dungeon)
        {
        }

        protected override bool OnDeadCompletely(EventParam eventParam, CombatLogNode logNode)
        {
	        if (Dungeon.GetAllowExtermination() == false)
	        {
				CurHP = 1;
	        }
	        return false;
        }
	}
}
