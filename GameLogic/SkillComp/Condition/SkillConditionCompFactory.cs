using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public static class SkillConditionCompFactory
    {
        // Static Factory
        static Dictionary<ConditionCompType, Type> _conditionMap = new Dictionary<ConditionCompType, Type>();
//	    static Dictionary<int, Type> _seCondTypeMap = new Dictionary<int, Type>();
	    
		public static void Init()
		{
				
			// common
			_conditionMap.Add(ConditionCompType.DungeonType, typeof(DungeonTypeConditionComp));
			_conditionMap.Add(ConditionCompType.DungeonTime, typeof(DungeonTimeConditionComp));
			_conditionMap.Add(ConditionCompType.RoleType, typeof(RoleTypeConditionComp));
			_conditionMap.Add(ConditionCompType.ClassType, typeof(ClassTypeConditionComp));
			_conditionMap.Add(ConditionCompType.CheckAttributePower, typeof(CheckAttributePowerConditionComp));
			
			// skill
			_conditionMap.Add(ConditionCompType.SkillBaseUid, typeof(SkillBaseUidConditionComp));
			_conditionMap.Add(ConditionCompType.SkillActorType, typeof(SkillActorTypeConditionComp));
			
			_conditionMap.Add(ConditionCompType.SkillType, typeof(SkillTypeConditionComp));
			_conditionMap.Add(ConditionCompType.SkillAreaType, typeof(SkillAreaTypeConditionComp));
			_conditionMap.Add(ConditionCompType.SkillAttributeType, typeof(SkillAttributeTypeConditionComp));
			
			_conditionMap.Add(ConditionCompType.SkillHitCount, typeof(SkillHitCountConditionComp));
			_conditionMap.Add(ConditionCompType.SkillResultType, typeof(SkillResultTypeConditionComp));
			
			_conditionMap.Add(ConditionCompType.CheckCritical, typeof(CheckCriticalConditionComp));
			
//			// target
			_conditionMap.Add(ConditionCompType.CheckContinuous, typeof(CheckContinuousConditionComp));
			_conditionMap.Add(ConditionCompType.CheckContinuousSelf, typeof(CheckContinuousOwnerConditionComp));
			_conditionMap.Add(ConditionCompType.CurHPPercent, typeof(CurHPPercentConditionComp));
			_conditionMap.Add(ConditionCompType.Immune, typeof(ImmuneConditionComp));
			_conditionMap.Add(ConditionCompType.Mez, typeof(MezConditionComp));
			_conditionMap.Add(ConditionCompType.Barrier, typeof(BarrierConditionComp));
		}

        public static List<ConditionComp> Create(ulong conUid, Unit owner)
        {
	        var uid = conUid;
	        
			var groupSheet = owner.Dungeon.GameData.GetData<SkillConditionGroupInfoSpec>(uid);
			
            List<ConditionComp> conditionList = new List<ConditionComp>();
            
			if (groupSheet == null)
			{
				var conComp = CreateInner(uid, owner);
				if (conComp != null)
				{
					conditionList.Add(conComp);
				}
			}
			else
			{
				var uidCount = groupSheet.ConditionUids.Count;
				var caseCount = groupSheet.ConditionCaseTypes.Count;
				if (uidCount != caseCount + 1)
				{
                    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Condition GroupCount uids{0}, case{1}", uidCount, caseCount);
					return null;
				}

				for(var i=0; i<uidCount; i++)
				{
					var curUid = groupSheet.ConditionUids[i];
					if (curUid == 0)
					{
						continue;
					}

					var conComp = CreateInner(curUid, owner);
					if (conComp == null)
					{
						continue;
					}

					if (i > 0)
					{
						var curCase = groupSheet.ConditionCaseTypes[i-1];
						conComp.CaseType = curCase;
					}
					
					conditionList.Add(conComp);
				}

			}
            return conditionList;
        }

        static ConditionComp CreateInner(ulong conUid, Unit owner)
        {
	        var uid = conUid;
	        
            var sheet = owner.Dungeon.GameData.GetData<SkillConditionInfoSpec>(uid);
            if (sheet == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Condition uid : {0}", uid);
                return null;
            }
            
			var conditionType = CorgiEnum.ParseEnum<ConditionCompType>(sheet.ConditionType);

			if (_conditionMap.ContainsKey(conditionType) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ConditionType {0}:{1}/{2}"
					,  owner.Dungeon.GameData.GetStrByUid(conUid), conditionType.ToString(), sheet.ConditionType);
				return null;
			}
		    
			var type = _conditionMap[conditionType];
			if (type == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Condition SkillComp type : <0>", conditionType.ToString());
				return null;
			}
			var inst = Activator.CreateInstance(type) as ConditionComp;

			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : <0>", conditionType.ToString());
				return null;
			}

			inst.SetDefault(owner);

			if (inst.Load(uid) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Load Condition : <0>", conUid);
				return null;
			}
			return inst;
	        
        }
	    
	    
	    
    }
}