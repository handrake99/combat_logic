
using System;
using System.Collections.Generic;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
    public class ConvertToTrueDamagePassiveComp : PassiveSkillComp
    {
	    public float ConvertRate { get; protected set; }

	    public ConvertToTrueDamagePassiveComp()
            : base()
        {
			EventManager.Register(CombatEventType.OnActiveHit, this.OnActiveHit);
			EventManager.Register(CombatEventType.OnActivePostHit, this.OnActivePostHit);
        }

	    protected override bool LoadInternal(ulong uid)
	    {
		    if (base.LoadInternal(uid) == false)
		    {
			    return false;
		    }
		    
		    ConvertRate = BasePercent;
		    if (ConvertRate >= 1.0)
		    {
			    ConvertRate = 1.0f;
		    }

		    if (ConvertRate < 0)
		    {
			    ConvertRate = 0f;
		    }
		    
		    return true;
	    }


	    private long _convertToDamage;
	    
	    protected bool OnActiveHit(EventParam eventParam, CombatLogNode logNode)
	    {
		    _convertToDamage = 0;
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}

			var oriDamage = dmgLogNode.Damage;

			_convertToDamage = (long)(oriDamage * ConvertRate);

			dmgLogNode.Damage = dmgLogNode.Damage - _convertToDamage;
			
			return false; // return true in OnActivePostHit. for Only ONE
	    }

	    protected bool OnActivePostHit(EventParam eventParam, CombatLogNode logNode)
	    {
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null )
			{
				return false;
			}

			if (_convertToDamage == 0)
			{
				return false;
			}

			var targetId = dmgLogNode.TargetId;
            var target = Owner.Dungeon.GetUnit(targetId);

            if (target == null)
            {
	            return false;
            }
			var passiveNode = new ConvertToTrueDamagePassiveSkillCompLogNode(this.Owner, target, this);
			passiveNode.SetParent(dmgLogNode);

			var newDmgLogNode = new DamageSkillCompLogNode(Owner, target, this);
			passiveNode.AddChild(newDmgLogNode);

			newDmgLogNode.Damage = _convertToDamage;
			
			newDmgLogNode.ForceHit = false;
			newDmgLogNode.IgnoreImmune = false;
			newDmgLogNode.IgnoreEvent = true;
			newDmgLogNode.IgnoreEnhance = true;
			newDmgLogNode.IgnoreCritical = true;
			newDmgLogNode.IgnoreBarrier = false;
			newDmgLogNode.IgnoreDefence = true;

			DamageSkillComp.DoApplyDamage(Owner, null, newDmgLogNode);
			
			logNode.AddChild(passiveNode);

		    return true;
	    }

	    
    }
}
