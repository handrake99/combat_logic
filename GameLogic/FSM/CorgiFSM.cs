using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class CorgiFSM : ITickable 
    {
        private Dictionary<UnitState, FSMUnitState> _stateMap = new Dictionary<UnitState, FSMUnitState>();
        private Dictionary<UnitTrigger, List<UnitState>> _triggerMap = new Dictionary<UnitTrigger, List<UnitState>>();

        //dynamic
        private UnitState _curState;
        private Unit _owner;

        public UnitState CurState => _curState;
        
        public CorgiFSM(Unit owner, UnitState initState)
        {
	        _owner = owner;
	        _curState = initState;
        }

        private void ChangeState(UnitState newState, DungeonLogNode dungeonLogNode)
        {
	        if (newState == _curState)
	        {
		        return;
	        }

	        var prevState = _curState;

			 CorgiCombatLog.Log(CombatLogCategory.Unit, "{0} change state {1} to {2} in dungeon[{3}]", _owner.Name, _curState, newState, _owner.Dungeon.DungeonType);
			 
	        _stateMap[_curState].OnExit(dungeonLogNode);
	        _curState = newState;
	        _stateMap[_curState].OnEnter(prevState, dungeonLogNode);
	        
        }
        
	    public void TickInCombat(ulong deltaTime, LogNode logNode)
        {
	        if (_stateMap.ContainsKey(_curState) == false )
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid state FSM {0}\n", _curState.ToString());
		        return;
	        }

	        var combatLogNode = (CombatLogNode) logNode;

	        var nextState = _stateMap[_curState].Tick(deltaTime, combatLogNode);

	        ChangeState(nextState, combatLogNode.DungeonLogNode);

	        OnTick(deltaTime, combatLogNode);
        }

	    void OnTick(ulong deltaTime, CombatLogNode logNode)
	    {
		    foreach (var state in _stateMap.Values)
		    {
			    state.OnTick(deltaTime, logNode);
		    }
	    }

	    public void Trigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (_triggerMap.ContainsKey(trigger) == false)
		    {
		        CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid trigger({0}) FSM", trigger.ToString());
			    
			    return;
		    }
            var list = _triggerMap[trigger];

            if (list.Contains(_curState) == false)
            {
		        CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid trigger FSM({0}) in trigger({1})\n", _curState.ToString(), trigger.ToString());
			    
			    return;
            }

            var cur = _stateMap[_curState];

            if (cur == null)
            {
		        CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid state FSM in {0} state", _curState);
	            return;
            }
            var nextState = cur.OnTrigger(trigger, logNode);

            if (_stateMap.ContainsKey(_curState) == false
                || _stateMap.ContainsKey(nextState) == false)
            {
	            CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid state setting");
	            return;
            }

            ChangeState(nextState, logNode.DungeonLogNode);
	    }
	    

	    public CorgiFSM RegisterState(UnitState state, FSMUnitState fsmNode)
	    {
		    _stateMap.Add(state, fsmNode);

		    return this;
	    }
	    
	    public CorgiFSM RegisterTrigger(UnitTrigger trigger, UnitState curState)
	    {

		    List<UnitState> stateList;

		    if (_triggerMap.ContainsKey(trigger) == false)
		    {
			    stateList = new List<UnitState>();
			    _triggerMap.Add(trigger, stateList);
		    }
		    else
		    {
			    stateList = _triggerMap[trigger];
		    }

		    stateList.Add(curState);

		    return this;
	    }
    }
}
