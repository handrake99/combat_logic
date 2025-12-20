

using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class PauseManager
    {
	    private Dungeon _dungeon;
	    private Unit _owner;
	    
        private List<PauseComp> _pauseList = new List<PauseComp>();
        private int _curCount;

        public PauseManager(Dungeon dungeon, Unit owner)
        {
	        _dungeon = dungeon;
	        _owner = owner;
        }

        public void AddPause(PauseComp pauseComp)
        {
	        if (pauseComp == null)
	        {
		        return;
	        }
			_pauseList.Add(pauseComp);
        }
        
		// public void AddDungeonPause(uint count, PauseTargetType targetType, string dungeonStateStr)
		// {
		// 	var pauseComp = PauseFactory.CreateInstance<DungeonPauseComp>(PauseCommandType.PauseCommand_Dungeon);
		// 	if (pauseComp == null) 
		// 	{
		// 		return ;
		// 	}
		// 	
		// 	var dungeonState = CorgiEnum.ParseEnum<DungeonState>(dungeonStateStr);
		// 	pauseComp.Initialize(_owner, (int)count, (int)targetType, (int) dungeonState);
		//
		// 	_pauseList.Add(pauseComp);
		//
		// }
		// public void AddUnitFSMPause(uint count, PauseTargetType targetType, string unitStateStr, string unitStateEventStr)
		// {
		// 	var pauseComp = PauseFactory.CreateInstance<UnitFSMPauseComp>(PauseCommandType.PauseCommand_UnitFSM);
		// 	if (pauseComp == null) 
		// 	{
		// 		return ;
		// 	}
		// 	var unitState = CorgiEnum.ParseEnum<UnitState>(unitStateStr);
		// 	var unitStateEvent = CorgiEnum.ParseEnum<UnitStateEvent>(unitStateEventStr);
		//
		// 	pauseComp.Initialize(_owner, (int)count, (int)targetType, (int)unitState, (int)unitStateEvent);
		//
		// 	_pauseList.Add(pauseComp);
		//
		// }

		public void OnEnterDungeon()
		{
			_curCount = 0;
		}

		public PauseTargetType GetCurPauseTargetType()
		{
			if (_pauseList.Count <= _curCount)
			{
				return PauseTargetType.PauseTarget_None;
			}
			var curPauseComp = _pauseList[_curCount];
			if (curPauseComp == null)
			{
				return PauseTargetType.PauseTarget_None;
				
			}

			return curPauseComp.GetPauseTargetType();

		}
		
		public void OnPauseEvent(Unit unit, PauseCommandType command, DungeonLogNode logNode, params int[] args)
		{
			if (_pauseList.Count <= _curCount)
			{
				return ;
			}

			var curPauseComp = _pauseList[_curCount];
			if (curPauseComp == null || curPauseComp.Command != command)
			{
				return ;
			}

			if (curPauseComp.CheckCondition(unit, args) == false)
			{
				return ;
			}

			_curCount++;
			_dungeon.Pause(curPauseComp.NextUid, logNode);
		}
		
    }
    
}