using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class ConsumeManaSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new ConsumeManaSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as ConsumeManaSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }

            curLog.Result = target.ApplyMana((int)BaseAmount * (-1));

            return true;
            
        }
        
    }
}
