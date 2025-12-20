using System;
using System.Collections.Generic;
using System.ComponentModel;
using Corgi.DBSchema;
using Corgi.GameData;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public static class PauseFactory
	{
		private static readonly Dictionary<PauseCommandType, Type> _pauseMap = new Dictionary<PauseCommandType, Type>();

		public static void Init()
		{
			_pauseMap.Clear();
			_pauseMap.Add(PauseCommandType.PauseCommand_Dungeon, typeof(DungeonPauseComp));
			_pauseMap.Add(PauseCommandType.PauseCommand_UnitFSM, typeof(UnitFSMPauseComp));
		}

		public static PauseComp CreateInstance(PauseCommandType commandType, Unit owner) 
		{
			if (_pauseMap.ContainsKey(commandType) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Tutorial," Invalid SkillType when create instance, {0}", commandType);
				return null;
			}
		    
			var type = _pauseMap[commandType];
			if (type == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Tutorial," invalid skillType SkillComp type : <0>", commandType);
				return null;
			}
			
			var inst = Activator.CreateInstance(type) as PauseComp;
			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Tutorial," Failed Create Instance : <0>", commandType);
				return null;
			}
			
			inst.SetOwner(owner);
		
			return inst;
		}
	}
}
