using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class AbsorbManaSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new AbsorbManaSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as AbsorbManaSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }

            curLog.TargetResult = target.ApplyMana((int)BaseAmount * (-1));

            var absorbAmount = curLog.TargetResult * (-1);
            if (absorbAmount > 0)
            {
                curLog.OwnerResult = Owner.RestoreNextMana((uint)absorbAmount);
            }

            return true;
        }
        
    }
}
