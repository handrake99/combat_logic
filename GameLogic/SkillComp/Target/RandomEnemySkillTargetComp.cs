using System;
using System.Collections.Generic;
using IdleCs.Managers;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class RandomEnemySkillTargetComp : SkillTargetComp
	{
		public RandomEnemySkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var units = Owner.Dungeon.GetEnemies(logNode.OwnerId);

			if (units == null)
			{
				return null;
			}

			var unitCount = units.Count;
			if (unitCount <= 0)
			{
				return null;
			}

			var index = Owner.Dungeon.Random.Range(0, unitCount-1);
			
			AddTarget(units[index]);

			return GetAndResetTargetList();
		}
	}
}
