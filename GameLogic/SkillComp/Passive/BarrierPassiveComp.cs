
using System;
using System.Collections.Generic;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
    public abstract class BarrierPassiveComp : PassiveSkillComp
    {
	    public long MaxAbsorbAmount { get; protected set; }
	    public float AbsorbRate { get; protected set; }

	    public BarrierPassiveComp()
            : base()
        {
			EventManager.Register(CombatEventType.OnSkillEffectApply, this.OnEnter);
			EventManager.Register(CombatEventType.OnBeingHitAlways, this.OnHit);
        }

	    protected override bool LoadInternal(ulong uid)
	    {
		    if (base.LoadInternal(uid) == false)
		    {
			    return false;
		    }
		    
		    MaxAbsorbAmount = BaseAbsolute;
		    AbsorbRate = BasePercent;
		    
		    return true;
	    }
	    public virtual int GetBarrierAmount()
	    {
		    return 0;
	    }
	    
	    public virtual int GetOrigBarrierAmount()
	    {
		    return 0;
	    }
	    
	    public virtual int GetEnhancedBarrierAmount()
	    {
		    return 0;
	    }
	    
	    public override SkillCompLogNode CreateLogNode(Unit target)
	    {
		    return new BarrierPassiveSkillCompLogNode(Owner, target, this);
	    }

	    protected virtual bool OnEnter(EventParam eventParam, CombatLogNode logNode)
	    {
		    return false;
	    }
	    
	    protected virtual bool OnHit(EventParam eventParam, CombatLogNode logNode)
	    {
		    return false;
	    }

	    
    }
}
