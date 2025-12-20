using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class RestoreNextManaSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new RestoreNextManaSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as RestoreNextManaSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }
            
            curLog.Result = target.RestoreNextMana((uint)BaseAmount);

            return true;
            
        }
        
    }
}
