using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class RestoreManaSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new RestoreManaSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as RestoreManaSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }

            curLog.Result = target.ApplyMana((int)BaseAmount);

            return true;
            
        }
        
    }
}