using System;

using IdleCs.Utils;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    interface ITickable
    {
        void TickInCombat(ulong deltaTick, LogNode logNode);
    }


    public abstract class CorgiCombatObject : CorgiObject, ITickable
    {
	    public void TickInCombat(ulong deltaTick, LogNode logNode)
	    {
		    var parentNode = (TickLogNode) logNode;

		    if (parentNode == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.System, "invalid log node");
			    return;
		    }
		    
		    Tick(deltaTick, parentNode);
	    }

	    protected abstract void Tick(ulong deltaTime, TickLogNode logNode);
    }
}