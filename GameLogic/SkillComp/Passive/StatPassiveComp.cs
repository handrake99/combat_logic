using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Library;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class StatPassiveComp : PassiveSkillComp 
	{
		private StatType _statType = StatType.StNone;
        private StatType _statFactorType;

		public StatType StatType
		{
			get { return _statType; }
		}
		
		public StatPassiveComp()
			: base()
		{
			PassiveType = PassiveSkillCompType.Stat;
		}

		protected override bool LoadInternal(ulong uid)
		{
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}

			var spec = GetSpec();
			if (spec == null)
			{
				return false;
			}

			_statType = CorgiEnum.ParseEnumPascal<StatType>(spec.PassiveSubType);
			if (_statType == StatType.StNone)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid stat type : {0} in {1}", spec.PassiveSubType, Owner.Dungeon.GameData.GetStrByUid(Uid));
				return false;
			}
			
	        try
	        {
		        var parameter = JObject.Parse(spec.Params);
		        var statTypeStr = CorgiJson.ParseString(parameter, "statType");
		        //var skillFactorTypeStr = CorgiJson.ParseString(parameter, "skillFactorType");

		        _statFactorType = CorgiEnum.ParseEnum<StatType>(statTypeStr);

		        if (_statType == _statFactorType)
		        {
			        // 같은 스탯을 해당 스탯 비례 올릴순 없다.(무한 루프 방지)
			        _statFactorType = StatType.StNone;
		        }
	        }
	        catch (Exception e)
	        {
		        CorgiCombatLog.LogFatal(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
		        return false;
	        }
			
			return true;
		}

		public long GetBaseAbsolute(CombatLogNode logNode)
		{
		    var baseAbsolute = BaseAbsolute;
		    
		    if (_statFactorType != StatType.StNone)
		    {
			    var statValue = Owner.GetStat(_statFactorType);
				var increasePercent =
					Owner.Dungeon.GameData.GetConfigFloat("config.skill_slot.mastery.increase_percent", 0.01f);
				var masteryPerLevel = Owner.Dungeon.GameData.GetConfigNumber("config.skill_slot.mastery.level", 100);
				var newBasePercent = CalcBasePercent(BasePercent, Mastery, ParentActor.Level, increasePercent, masteryPerLevel);

			    baseAbsolute = (long)(newBasePercent * statValue);
		    }
		    
			return baseAbsolute;
		}

		public float GetBasePercent(CombatLogNode logNode)
		{
		    var basePercent = BasePercent;
		    if (_statFactorType != StatType.StNone)
		    {
			    //var statValue = Owner.GetStat(_statFactorType);
			    //basePercent = basePercent * statValue;
			    return 0f;
			    
		    }

		    return basePercent;
		}
		
		
	}
}
