using System;

using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	public class HealAtkFactorSkillComp : HealSkillComp
	{
		public HealAtkFactorSkillComp()
		{
		}

		protected override bool LoadInternal(ulong uid)
		{
			return base.LoadInternal(uid);
		}

		public override SkillCompLogNode CreateLogNode(Unit target)
		{
			var curLog = new HealSkillCompLogNode(this.Owner, target, this);
			
	        curLog.IgnoreCritical = IgnoreCritical;
	        curLog.IgnoreEvent = IgnoreEvent;

	        return curLog;
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
				curLog.Heal = BaseAmount + BaseFactor * Owner.AttackPower;
			}
			else
			{
				curLog.Heal = BaseAmount;
			}
			
            curLog.AddDetailLog($"baseHeal:{curLog.Heal}, ownerAttackPower:{Owner.AttackPower}, factor:{BaseFactor}");

			// owner Critical 적용
			//if(curLog.IgnoreCrit == false)
			//{
			//curLog.Caster.ApplyCritical(output, output.ForceCritical);
			//}

			if (DoApplyHeal(curLog) == false)
			{
				return false;
			}

			return true;

		}
	}
}