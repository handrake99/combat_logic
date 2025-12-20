using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class DamageTransferedSkillComp : DamageSkillComp
    {
        public DamageTransferedSkillComp()
        {
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
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DamageAtkFactorSkillComp uid : {0}", uid);
                return false;
            }

            // Damage = spec.BaseAmount;
            // AtkFactor = spec.ApFactor;

            ForceHit = true;
            IgnoreDefence = true;
            IgnoreCritical = true;
            IgnoreEvent = true;

            return true;
        }
        
        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            var curLog = new DamageSkillCompLogNode(Owner, target, this);
            
	        curLog.IgnoreDefence = ForceHit;
	        curLog.IgnoreDefence = IgnoreDefence;
	        curLog.IgnoreCritical = IgnoreCritical;
	        curLog.IgnoreEvent = IgnoreEvent;
            
            return curLog;
        }
        
        // TransferDamage Passive 로부터 올라온것만 처리된다.
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var dmgLogNode = logNode as DamageSkillCompLogNode;
            var passiveLogNode = logNode.Parent as TransferDamagePassiveSkillCompLogNode;
            if (dmgLogNode == null || passiveLogNode == null)
            {
                return false;
            }

            dmgLogNode.Damage = (int)(passiveLogNode.TransferDamge / passiveLogNode.TargetCount);

            if (DoApplyDamage(dmgLogNode) == false)
            {
                return false;
            }
	        
            return true;
        }
    }
}