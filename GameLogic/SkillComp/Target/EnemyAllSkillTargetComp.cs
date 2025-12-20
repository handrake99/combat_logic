using System;
using System.Collections.Generic;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class EnemyAllSkillTargetComp : SkillTargetComp
	{
		public EnemyAllSkillTargetComp()
		{
			IsAreaEffect = true;
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var unitList = Owner.Dungeon.GetEnemies(logNode.OwnerId);
			if (unitList == null)
			{
				return GetAndResetTargetList();
			}
			
			foreach (var curUnit in unitList)
			{
				if (curUnit == null)
				{
					continue;
				}
				AddTarget(curUnit);
			}

			return GetAndResetTargetList();
		}
	}
}