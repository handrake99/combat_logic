using System;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class AbsorbBuffSkillComp : ActiveSkillComp
    {
	    private ulong _instUid = 0;
		    
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
	        
			IsEachTarget = spec.IsEachTarget;

	        if (string.IsNullOrEmpty(spec.Params) == false)
	        {
				try
				{
					var parameter = JObject.Parse(spec.Params);
					var uidStr = CorgiJson.ParseString(parameter, "uid");

					_instUid = GameDataManager.GetUidByString(uidStr);;

				}
				catch (Exception e)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
					return false;
				}
	        }

	        return true;
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new AbsorbContinuousSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as AbsorbContinuousSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }
            
            // try interrupt
	        foreach(var curInst in target.ContinuousList)
			{
				if (curInst.BenefitType != ContinuousBenefitType.Buff)
				{
					continue;
				}

				if (_instUid != 0 && _instUid != curInst.Uid)
				{
					continue;
				}

				curLog.DoAbsorb(Owner, target, curInst);
			}
	        
			return true;
        }
        
    }
}
