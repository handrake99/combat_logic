using System;

using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	
	/// <summary>
	/// Do Damage from any active skill comp
	/// used by passive skill comp
	/// </summary>
    [System.Serializable]
	public class DamageSkillOutputSkillComp : DamageSkillComp
	{
		//private float _finalDamage = 0f;
		
		public DamageSkillOutputSkillComp()
		{
		}
		
        // protected override bool LoadInternal(ulong uid)
        // {
	       //  // DO NOT load!!
	       //  return false;
        // }

		// // called explicitly
		// public void DoApplyPre(int damage, DamageSkillCompLogNode dmgNode)
		// {
		// 	if (damage <= 0)
		// 	{
		// 		_finalDamage = 0;
		// 		return;
		// 	}
		//
		// 	_finalDamage = damage;
		// }

		public override SkillCompLogNode CreateLogNode(Unit target)
		{
            var curLog = new DamageSkillCompLogNode(this.Owner, target, this);
            
	        curLog.IgnoreCritical = true;
	        curLog.IgnoreEnhance = true;
	        curLog.IgnoreEvent = true;

	        return curLog;
		}

        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
	        var thisLogNode = logNode as DamageSkillCompLogNode;
	        if (thisLogNode == null || thisLogNode.Parent == null || thisLogNode.Parent.Parent == null)
	        {
		        // Parent : EventLogNode / Parent of Parent : DamageLogNode
		        return false;
	        }

	        var preDamageLogNode = logNode.Parent.Parent as DamageSkillCompLogNode;
	        if (preDamageLogNode == null)
	        {
		        return false;
	        }

	        thisLogNode.Damage = preDamageLogNode.FinalDamage * this.BaseFactor;
	        
            thisLogNode.AddDetailLog($"fDamage:{thisLogNode.Damage}, Factor:{BaseFactor}, BaseDamage:{preDamageLogNode.FinalDamage}");

	        if (DoApplyDamage(thisLogNode) == false)
	        {
		        return false;
	        }

	        return true;

        }
	}
}