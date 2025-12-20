using System;
using System.Collections.Generic;
using IdleCs;

using IdleCs.GameLog;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class SkillEffectStandard : SkillEffectInst
    {
	    public SkillEffectStandard(Unit caster, ContinuousSkillComp skillComp, Unit target)
            : base(caster, skillComp, target)
	    {
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
			var ret = base.LoadInternal(uid);
		    if (ret == false)
			{
				return false;
			}
		    
		    EffectInstType = SkillEffectInstType.Standard;
		    
		    return true;

	    }
	    
		public override void OnDoApply(SkillEffectInst inst, CombatLogNode logNode)
		{
			//do nothing
			base.OnDoApply(inst, logNode);
		}
		
		public override void DoSkillEffectAction(Unit target, Action<SkillEffectInst> action)
		{
			if (target.ObjectId != this.Target.ObjectId)
			{
				return;
			}
			action(this);
		}
    }
}