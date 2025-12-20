
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class FriendLowestHPSkillTargetComp : SkillTargetComp
	{
		protected uint UnitCount = 1;
		
		public FriendLowestHPSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var unitList = new List<Unit>();
			var friends = Owner.Dungeon.GetFriends(logNode.OwnerId);
			if (friends == null)
			{
				return GetAndResetTargetList();
			}

			unitList.AddRange(friends);

			unitList.Sort(delegate(Unit x, Unit y)
			{
				var xRate = (float)x.CurHP / x.MaxHP;
				var yRate = (float)y.CurHP / y.MaxHP;

				if (xRate < yRate) return -1;
				else if (xRate > yRate) return 1;

				if (x.CurHP < y.CurHP) return -1;
				else if (x.CurHP > y.CurHP) return 1;

				return 0;
			});

			uint count = 0;
			foreach (var curUnit in unitList)
			{
				if (curUnit == null || !curUnit.IsLive())
				{
					continue;
				}
				
				AddTarget(curUnit);
				count++;
				if (count >= UnitCount)
				{
					break;
				}
			}

			return GetAndResetTargetList();
		}
	}
}
