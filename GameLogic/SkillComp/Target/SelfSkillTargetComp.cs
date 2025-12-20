using System;
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class SelfSkillTargetComp : SkillTargetComp 
	{
		public SelfSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}
			
			AddTarget(logNode.OwnerId);
			
			return GetAndResetTargetList();
		}
	}
}