

using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class InterruptSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new InterruptSkillCompLogNode(Owner, target, this);
        }

        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            if (target.IsCasting == false && target.IsChanneling == false)
            {
                return false;
            }

            var curLog = logNode as InterruptSkillCompLogNode;
            
            if (curLog == null)
            {
                return false;
            }
            // try interrupt
            
			if(target.InterruptCasting(Owner, curLog))
            {
                target.OnEvent(CombatEventType.OnCastingCancel, new EventParamSkillComp(Owner.Dungeon, this, curLog), curLog);
				Owner.OnEvent(CombatEventType.OnCastingInterrupt, new EventParamSkillComp(Owner.Dungeon, this, curLog), curLog);
				
			}

            return true;
        }
    }
}