using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class SummonerSkillTargetComp : SkillTargetComp
    {
		public SummonerSkillTargetComp()
		{
			IsAreaEffect = true;
		}

		public override List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			if (logNode == null)
			{
				return null;
			}

			if (Owner.Summoner == null)
			{
				return null;
			}

			AddTarget(Owner.Summoner);

			return GetAndResetTargetList();
		}
        
    }
}