
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public abstract class CorgiFSMState : ITickable
    {
	    public void TickInCombat(ulong deltaTick, LogNode logNode)
	    {
		    var parentNode = (CombatLogNode) logNode;

		    if (parentNode == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid log node");
			    return;
		    }
		    
		    Tick(deltaTick, parentNode);
	    }
        
        public abstract void Tick(ulong deltaTime, CombatLogNode logNode);
    }

}