using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	public class FriendTargetNearestSkillTargetComp : SkillTargetComp
	{
		public FriendTargetNearestSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var caster = Owner.Dungeon.GetUnit(logNode.CasterId);

			var unit = Owner.Dungeon.GetNearestFriend(caster);

			if (unit == null)
			{
				return null;
			}

			AddTarget(unit);

			return GetAndResetTargetList();
		}
	}
}
