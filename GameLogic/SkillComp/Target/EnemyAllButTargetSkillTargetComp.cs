using System;
using System.Collections.Generic;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class EnemyAllButTargetSkillTargetComp : SkillTargetComp
	{
		public EnemyAllButTargetSkillTargetComp()
		{
			IsAreaEffect = true;
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var unitList = Owner.Dungeon.GetEnemies(logNode.CasterId);

			if (unitList == null)
			{
				return GetAndResetTargetList();
			}
			

			foreach (var curUnit in unitList) 
			{
				if (curUnit == null || curUnit.ObjectId == logNode.TargetId)
				{
					continue;
				}
				
				AddTarget(curUnit);
			}

			return GetAndResetTargetList();
		}
	}
}