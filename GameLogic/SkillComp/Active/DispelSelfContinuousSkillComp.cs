using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	/// <summary>
	/// Continuous에 의해 불렸을때만 효과 적용
	/// 자기 자신의 Buff/Debuff 를 원하는 수만큼 해제한다.
	/// </summary>
    public class DispelSelfContinuousSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new DispelSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
	        if (ParentActor.SkillActorType != SkillActorType.SkillContinuous)
	        {
		        return false;
	        }

            var curLog = logNode as DispelSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }

            var skillEffectInst = ParentActor as SkillEffectInst;

            if (skillEffectInst == null)
            {
	            return false;
            }

            curLog.DoDispel(skillEffectInst);
            
			return true;
        }
        
    }
}
