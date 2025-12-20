using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class FriendAllButSelfSkillTargetComp : SkillTargetComp
    {
		public FriendAllButSelfSkillTargetComp()
		{
			IsAreaEffect = true;
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			var unitList = Owner.Dungeon.GetFriends(logNode.OwnerId);
			if (unitList == null)
			{
				return GetAndResetTargetList();
			}
			foreach (var curUnit in unitList)
			{
				if (curUnit == null || curUnit.ObjectId == Owner.ObjectId)
				{
					continue;
				}
				
				AddTarget(curUnit);
			}

			return GetAndResetTargetList();
		}
        
    }
}