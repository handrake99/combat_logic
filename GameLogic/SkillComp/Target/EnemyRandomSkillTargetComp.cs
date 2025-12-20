using System.Collections.Generic;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class EnemyRandomSkillTargetComp : SkillTargetComp
	{
		public EnemyRandomSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var unitList = Owner.Dungeon.GetEnemies(logNode.OwnerId);
			var unitCount = unitList.Count;

			if (unitCount <= 0)
			{
				return GetAndResetTargetList();
			}

			var index = Owner.Dungeon.Random.Range(0, unitCount);

			AddTarget(unitList[index]);
			
			return GetAndResetTargetList();
		}
	}
}
