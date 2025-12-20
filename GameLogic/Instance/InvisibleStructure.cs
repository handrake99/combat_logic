using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class InvisibleStructure : Unit
    {
		/// <summary>
		/// static data
		/// </summary>

        public InvisibleStructure(Dungeon dungeon)
			: base(dungeon)
        {
        }

        protected override bool LoadInternal(ulong uid)
        {
		    CombatSideType = CombatSideType.Player;

		    UnitSize = 0.0f;

		    var level = Level;
		    
		    var levelUidStr = string.Format("structure_level.{0}", level);
		    var levelSheet = Dungeon.GameData.GetData<LevelInfoSpec>(levelUidStr);

		    if (levelSheet == null)
		    {
			    return false;
		    }
		    
		    StatMap[StatType.StMaxHp].OrigStat = levelSheet.MaxHP;
		    StatMap[StatType.StAttackSpeed].OrigStat = levelSheet.AttackSpeed;
		    StatMap[StatType.StAttackPower].OrigStat = levelSheet.AttackPower;
		    StatMap[StatType.StDefence].OrigStat = levelSheet.Defence;
		    StatMap[StatType.StHit].OrigStat = levelSheet.Hit;
		    StatMap[StatType.StEvasion].OrigStat = levelSheet.Evasion;
		    StatMap[StatType.StCrit].OrigStat = levelSheet.Crit;
		    StatMap[StatType.StResilience].OrigStat =  levelSheet.Resilience;
		    StatMap[StatType.StCritDmg].OrigStat =  0;
		    
	        return base.LoadInternal(uid);
        }

        protected override void Tick(ulong deltaTime, TickLogNode logNode)
        {
		    var preCount = ContinuousList.Count;
		    foreach (var effectInst in ContinuousList)
		    {
			    effectInst.TickInCombat(deltaTime, logNode);
				var postCount = ContinuousList.Count;
			    if (effectInst.IsLive == false)
			    {
				    _isUpdatedEffect = true;
			    }

			    if (preCount != postCount)
			    {
				    CorgiCombatLog.LogError(CombatLogCategory.Unit, "[Combat] Continuous List was changed.");
				    break;
			    }
		    }
		    
		    OnUpdateEffect(logNode);
        }
        // called before load
        public void SetLevel(uint level)
        {
	        Level = level;
        }
        
        // called after load
        public void UpdateSkills(List<ulong> skillList, List<ulong> skillTimestamps = null)
        {
	        
	        for(var i=0; i<skillList.Count; i++)
	        {
		        var skillUid = skillList[i];
		        
				if (skillUid == 0)
				{
					continue;
				}
				
				var curSkill = SkillFactory.Create(skillUid, this) as SkillPassive;

				if (curSkill == null)
				{
					continue;
				}

				if (skillTimestamps != null && skillTimestamps.Count > i)
				{
					var timestamp = skillTimestamps[i];
					var curExternalSkill = curSkill as SkillExternal;
					if (curExternalSkill != null)
					{
						curExternalSkill.SetTimestamp(timestamp);
					}
				}
				
				AddPassiveSkill(curSkill);
	        }
        }
        
        public bool AddSkill(ulong skillUid, ulong skillTimestamp)
        {
			var curSkill = SkillFactory.Create(skillUid, this) as SkillPassive;

			if (curSkill == null)
			{
				return false;
			}

			if (skillTimestamp != 0)
			{
				var curExternalSkill = curSkill as SkillExternal;
				if (curExternalSkill != null)
				{
					curExternalSkill.SetTimestamp(skillTimestamp);
				}
			}
			 
			AddPassiveSkill(curSkill);
			return true;
        }

        public bool AddSkillAndTriggerEvent(ulong skillUid, CombatLogNode logNode, CombatEventType eventType)
        {
	        var curSkill = SkillFactory.Create(skillUid, this) as SkillPassive;

	        if (curSkill == null)
	        {
		        return false;
	        }

	        AddPassiveSkill(curSkill);

	        var eventParam = new EventParamUnit(this);
	        curSkill.OnEvent(eventType, eventParam, logNode);
	        return true;
        }
        
        public bool RemoveSkill(ulong skillUid)
        {
	        foreach (var skill in PassiveSkills)
	        {
		        if (skill != null && skill.Uid == skillUid)
		        {
			        RemovePassiveSkill(skill);
			        return true;
		        }
	        }

	        return false;
        }
        

	    public void OnEnterStage()
	    {
	    }
        
    }
}