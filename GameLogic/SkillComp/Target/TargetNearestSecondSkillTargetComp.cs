using System;
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	public class TargetNearestSecondSkillTargetComp : SkillTargetComp
	{
		public TargetNearestSecondSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var caster = Owner.Dungeon.GetUnit(logNode.CasterId);

			var unit = Owner.Dungeon.GetNearestEnemy(caster, 2);

			if (unit == null)
			{
				return null;
			}

			AddTarget(unit);

			return GetAndResetTargetList();
		}
	}
}
