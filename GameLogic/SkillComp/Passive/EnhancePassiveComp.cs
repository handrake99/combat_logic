//

using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public class EnhancePassiveComp : PassiveSkillComp
    {
        private EnhanceType _enhanceType = EnhanceType.None;


        private StatType _statFactorType;
        private SkillFactorType _skillFactorType;
        
        public EnhancePassiveComp()
            : base()
        {
	        PassiveType = PassiveSkillCompType.Enhance;
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

			_enhanceType = CorgiEnum.ParseEnum<EnhanceType>(spec.PassiveSubType);
			if (_enhanceType == EnhanceType.None)
			{
				return false;
			}
			
	        try
	        {
		        var parameter = JObject.Parse(spec.Params);
		        var statTypeStr = CorgiJson.ParseString(parameter, "statType");
		        var skillFactorTypeStr = CorgiJson.ParseString(parameter, "skillFactorType");

		        _statFactorType = CorgiEnum.ParseEnum<StatType>(statTypeStr);
		        _skillFactorType = CorgiEnum.ParseEnum<SkillFactorType>(skillFactorTypeStr);
	        }
	        catch (Exception e)
	        {
		        CorgiCombatLog.LogFatal(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
		        return false;
	        }
			
			return true;
		}

		public override bool GetEnhanceValue(EnhanceType enhanceType, ref float absValue, ref float percentPlus,
			ref float percentMinus, SkillActionLogNode logNode)
		{
		    if (_enhanceType != enhanceType)
		    {
			    return false;
		    }

		    // Check Conditions
		    var activeCount = CheckActive(logNode);
		    if (activeCount <= 0)
		    {
			    return false;
		    }

		    
		    var enhanceTypeStr = _enhanceType.ToString();

		    var isProb = enhanceTypeStr.EndsWith("Prob");

		    for (var i = 0; i < activeCount; i++)
		    {
				if (isProb)
				{
					GetEnhanceValueProb(ref absValue, ref percentPlus, ref percentMinus);
					return true;
				}
				
				GetEnhanceValue(ref absValue, ref percentPlus, ref percentMinus, logNode);
			    
		    }

            return true;
			
		}
			
	    public override bool GetEnhanceValue(EnhanceType enhanceType, ref float absValue, ref float percentPlus, ref float percentMinus, SkillCompLogNode logNode)
	    {
		    if (_enhanceType != enhanceType)
		    {
			    return false;
		    }

		    // Check Conditions
		    var activeCount = CheckActive(logNode);
		    if (activeCount <= 0)
		    {
			    return false;
		    }

		    var enhanceTypeStr = _enhanceType.ToString();

		    var isProb = enhanceTypeStr.EndsWith("Prob");

		    for (var i = 0; i < activeCount; i++)
		    {
				if (isProb)
				{
					GetEnhanceValueProb(ref absValue, ref percentPlus, ref percentMinus);
					return true;
				}
				
				GetEnhanceValue(ref absValue, ref percentPlus, ref percentMinus, logNode.Parent as SkillActionLogNode);
		    }

            return true;
	    }

	    void GetEnhanceValueProb(ref float absValue, ref float percentPlus, ref float percentMinus)
	    {
			var stackCount = StackCount;
            absValue += (BasePercent * stackCount);
	    }

	    void GetEnhanceValue(ref float absValue, ref float percentPlus, ref float percentMinus, SkillActionLogNode actionLogNode)
	    {
		    var baseAbsolute = BaseAbsolute;
		    var basePercent = BasePercent;
		    

		    if (_statFactorType != StatType.StNone)
		    {
			    var statValue = Owner.GetStat(_statFactorType);
				var increasePercent =
					Owner.Dungeon.GameData.GetConfigFloat("config.skill_slot.mastery.increase_percent", 0.01f);
				var masteryPerLevel = Owner.Dungeon.GameData.GetConfigNumber("config.skill_slot.mastery.level", 100);
				var newBasePercent = CalcBasePercent(BasePercent, Mastery, ParentActor.Level, increasePercent, masteryPerLevel);

			    baseAbsolute += (long)(statValue * newBasePercent);
			    basePercent = 0f;
		    }
		    
		    if (_skillFactorType != SkillFactorType.None)
		    {
			    var skillFactorValue = 1L;

			    if (actionLogNode != null)
			    {
					if (_skillFactorType == SkillFactorType.CastingTime)
					{
						 skillFactorValue = (long)actionLogNode.CastingTime;
					}else if (_skillFactorType == SkillFactorType.ManaCost)
					{
						 skillFactorValue = actionLogNode.ManaCost;
					}
			    }
			    
			    baseAbsolute = baseAbsolute * skillFactorValue;
			    basePercent = basePercent * skillFactorValue;
		    }
		    
			var stackCount = StackCount;
			
            absValue += (int)(baseAbsolute * stackCount);
            
            if (basePercent > 0)
            {
                percentPlus += basePercent * stackCount;
            }
            else
            {
                for (int i = 0; i < stackCount; i++)
                {
                    percentMinus *= (1 + basePercent);
                }
            }
		    
	    }
    }
}
