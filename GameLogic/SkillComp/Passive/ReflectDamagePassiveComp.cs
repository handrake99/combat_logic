using System;
using IdleCs.GameLog;
using IdleCs.Library;


namespace IdleCs.GameLogic
{
    public class ReflectDamagePassiveComp : PassiveSkillComp
    {
	    public float ReflectRate { get; protected set; }

	    public ReflectDamagePassiveComp()
            : base()
        {
			EventManager.Register(CombatEventType.OnBeingHit, OnBeingHit);
			EventManager.Register(CombatEventType.OnBeingPostHit, OnBeingPostHit);
        }

	    protected override bool LoadInternal(ulong uid)
	    {
		    if (base.LoadInternal(uid) == false)
		    {
			    return false;
		    }
		    
		    ReflectRate = BasePercent;
		    
		    return true;
	    }

	    private long _reflectDamage;
	    protected bool OnBeingHit(EventParam eventParam, CombatLogNode logNode)
	    {
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}
		    
			var oriDamage = dmgLogNode.Damage;

			_reflectDamage = (long)(oriDamage * ReflectRate);

			return false; // return true in OnActivePostHit. for Only ONE
	    }
	    
	    protected bool OnBeingPostHit(EventParam eventParam, CombatLogNode logNode)
	    {
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}
			
			var passiveNode = new ReflectDamagePassiveSkillCompLogNode(this.Owner, Owner, this);
			passiveNode.SetParent(dmgLogNode);
			
			var casterId = dmgLogNode.CasterId;
            var target = Owner.Dungeon.GetUnit(casterId);
			
			var newDmgLogNode = new DamageSkillCompLogNode(Owner, target, this);
			passiveNode.AddChild(newDmgLogNode);

			newDmgLogNode.Damage = _reflectDamage;
			
			newDmgLogNode.ForceHit = false;
			newDmgLogNode.IgnoreImmune = false;
			newDmgLogNode.IgnoreEvent = true;
			newDmgLogNode.IgnoreEnhance = true;
			newDmgLogNode.IgnoreCritical = true;
			newDmgLogNode.IgnoreBarrier = false;
			newDmgLogNode.IgnoreDefence = false;

			DamageSkillComp.DoApplyDamage(Owner, null, newDmgLogNode);

			logNode.AddChild(passiveNode);

			return true;
	    }

	    
    }
}
