using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class HealMaxHPPercentSkillComp : HealSkillComp
    {
		protected override bool LoadInternal(ulong uid)
		{
			return base.LoadInternal(uid);
		}

		public override SkillCompLogNode CreateLogNode(Unit target)
		{
			return new HealSkillCompLogNode(this.Owner, target, this);
		}
		
		

		protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
		{
			var curLog = logNode as HealSkillCompLogNode;
			if (curLog == null)
			{
				return false;
			}

			if (BaseFactor > 0)
			{
				curLog.Heal = BaseFactor * Owner.MaxHP;
                curLog.AddDetailLog($"Heal :{curLog.Heal}, Factor:{BaseFactor}, MaxHP{Owner.MaxHP}");
			}
			else
			{
				return false;
			}

			if (DoApplyHeal(curLog) == false)
			{
				return false;
			}

			return true;

		}
    }
}