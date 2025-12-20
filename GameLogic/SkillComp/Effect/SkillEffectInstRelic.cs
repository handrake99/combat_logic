using System;
using System.Collections.Generic;
using IdleCs;

using IdleCs.GameLog;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class SkillEffectInstRelic: SkillEffectAuraInst
    {
	    public SkillEffectInstRelic(Unit caster, ContinuousSkillComp skillComp, SkillTargetComp skillTargetComp, List<ConditionComp> conditionComps)
            : base(caster, skillComp, skillTargetComp, conditionComps)
	    {
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
			var ret = base.LoadInternal(uid);
		    if (ret == false)
			{
				return false;
			}
		    
		    EffectInstType = SkillEffectInstType.Relic;
		    
		    return true;

	    }
	    

		public override bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
		{
			return base.OnEvent(eventType, eventParam, logNode);
		}
		
		// public override void DoSkillEffectAction(Unit target, Action<SkillEffectInst> action)
		// {
		// 	action(this);
		// }
    }
}
