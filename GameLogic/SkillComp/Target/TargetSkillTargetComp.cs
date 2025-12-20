using System;
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class TargetSkillTargetComp : SkillTargetComp
	{
		public TargetSkillTargetComp()
		{
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}
			
			AddTarget(logNode.TargetId);
			
			return GetAndResetTargetList();
		}
	}
}