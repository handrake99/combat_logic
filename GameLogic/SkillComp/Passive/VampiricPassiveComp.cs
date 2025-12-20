using System;
using System.Collections.Generic;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
    public class VampiricPassiveComp : PassiveSkillComp
    {
	    public float VampiricRate { get; protected set; }

	    public VampiricPassiveComp()
            : base()
        {
			EventManager.Register(CombatEventType.OnActiveHit, OnActiveHit);
			EventManager.Register(CombatEventType.OnActivePostHit, OnActivePostHit);
        }

	    protected override bool LoadInternal(ulong uid)
	    {
		    if (base.LoadInternal(uid) == false)
		    {
			    return false;
		    }
		    
		    VampiricRate = BasePercent;
		    
		    return true;
	    }

	    private long _healAmount;
	    protected bool OnActiveHit(EventParam eventParam, CombatLogNode logNode)
	    {
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}
		    
			var oriDamage = dmgLogNode.Damage;

			_healAmount = (long)(oriDamage * VampiricRate);

			return false; // return true in OnActivePostHit. for Only ONE
	    }
	    
	    protected bool OnActivePostHit(EventParam eventParam, CombatLogNode logNode)
	    {
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}
			
			var passiveNode = new VampiricPassiveSkillCompLogNode(this.Owner, Owner, this);
			passiveNode.SetParent(dmgLogNode);
			
			var newLogNode = new HealSkillCompLogNode(Owner, Owner, this);
			passiveNode.AddChild(newLogNode);

			newLogNode.Heal = _healAmount;
			
			newLogNode.ForceHit = false;
			newLogNode.IgnoreImmune = false;
			newLogNode.IgnoreEvent = true;
			newLogNode.IgnoreEnhance = true;
			newLogNode.IgnoreCritical = true;
			newLogNode.IgnoreBarrier = false;
			newLogNode.IgnoreDefence = true;
			
			HealSkillComp.DoApplyHeal(Owner, null, newLogNode);

			logNode.AddChild(passiveNode);

			return true;
	    }
    }
}
