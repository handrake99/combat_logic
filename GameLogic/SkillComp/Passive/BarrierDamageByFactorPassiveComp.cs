using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public class BarrierDamageByFactorPassiveComp : BarrierDamagePassiveComp
    {
        public BarrierDamageByFactorPassiveComp() : base()
        {
        }

        protected override bool LoadInternal(ulong uid)
        {
	        if (base.LoadInternal(uid) == false)
	        {
		        return false;
	        }

	        var spec = GetSpec();

	        try
	        {
		        var parameter = JObject.Parse(spec.Params);
		        var statTypeStr = CorgiJson.ParseString(parameter, "absorbFactorType");
		        var absorbFactorType = CorgiEnum.ParseEnum<StatType>(statTypeStr);
		        var absorbFactor = CorgiJson.ParseFloat(parameter, "absorbFactor");

		        if (absorbFactorType == StatType.StNone)
		        {
			        return false;
		        }

		        if (absorbFactor < 0)
		        {
			        return false;
		        }
		        
		        var increasePercent =
			        Owner.Dungeon.GameData.GetConfigFloat("config.skill_slot.mastery.increase_percent", 0.01f);
		        var masteryPerLevel = Owner.Dungeon.GameData.GetConfigNumber("config.skill_slot.mastery.level", 100);
		        absorbFactor = CalcBasePercent(absorbFactor, Mastery, ParentActor.Level, increasePercent, masteryPerLevel);

                Amount = (int)(MaxAbsorbAmount + absorbFactor * Owner.GetStat(absorbFactorType));
	        }
	        catch (Exception e)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
		        return false;
	        }
	        
	        return true;
        }
    }
}
