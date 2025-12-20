using System.Threading;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class BarrierDamageByTransferDamagePassiveComp : BarrierDamagePassiveComp
    {
        public BarrierDamageByTransferDamagePassiveComp()
        {
        }
        
        protected override bool LoadInternal(ulong uid)
        {
            var parentSkillInst = ParentActor as SkillEffectInst;
            // should involed by skill continuous
            if (parentSkillInst == null)
            {
                return false;
            }
            
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
            parentSkillInst.RegisterEvent(CombatEventType.OnSkillEffectApply, OnUpdated);

            return true;
        }
        
        bool OnUpdated(EventParam eventParam, CombatLogNode logNode)
        {
			var curLogNode = logNode.Parent as TransferDamagePassiveSkillCompLogNode;
			if (curLogNode == null)
			{
				return false;
			}

			var addedDamage = (int)(curLogNode.TransferDamge / curLogNode.TargetCount);

			Amount += addedDamage;

            OrigAmount = Amount;
            
            return true;
        }

        protected override bool OnEnter(EventParam EventParam, CombatLogNode logNode)
        {
            return true;
        }
    }
}