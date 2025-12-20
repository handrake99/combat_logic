using System;
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class CasterSkillTargetComp : SkillTargetComp
	{
		public CasterSkillTargetComp()
		{
		}
		
		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}
			
			AddTarget(logNode.CasterId);
			
			return GetAndResetTargetList();
		}
	}
}