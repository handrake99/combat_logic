using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class DecreaseManaCostSelfSkillComp : ActiveSkillComp
    {
        public DecreaseManaCostSelfSkillComp()
        {
        }
        
        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new DecreaseManaCostSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as DecreaseManaCostSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }

            // target 은 본인만.
            if (Owner.ObjectId != target.ObjectId)
            {
                return false;
            }

            // actor 는 스킬 타입
            if (ParentActor.SkillActorType != SkillActorType.Skill)
            {
                return false;
            }

            var skill = ParentActor as SkillActive;

            // 일반공격은 제외
            if (skill == null || skill.SkillType == SkillType.Attack)
            {
                return false;
            }

            curLog.Result = target.ChangeManaCost((-1) * (int)BaseAmount);

            return true;
            
        }
    }
}