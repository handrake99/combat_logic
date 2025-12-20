using System;
using System.Collections.Generic;
using System.Linq;
using Corgi.GameData;
using IdleCs.Utils;

using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;

namespace IdleCs.GameLogic
{
    public class Rift 
    {
	    bool _isFirst = true;
	    private ulong _curSharedRiftTick = 0L;
	    
	    private ulong _sendingCumulativeDamageInterval;
	    private readonly Dictionary<string, ulong> _lastCumulativeDamageSentTimeMap = new Dictionary<string, ulong>();

        private SharedRift _sharedRift;
        private readonly GameDataManager _gameData;
        
        private readonly Dictionary<string, DungeonRift> _dungeonMap = new Dictionary<string, DungeonRift>();
        
        private readonly Queue<string> _partyMemberEnterQueue = new Queue<string>();	// 균열 던전에 입장한 파티원들 기록. 틱마다 균열 던전의 큐에 업데이해즘

        public Rift()
        {
	        _gameData = GameDataManager.Instance;
	        _sendingCumulativeDamageInterval = (ulong)_gameData.GetConfigNumber("config.rift.party.accdamage.sec", 5) * 1000;
        }
        public bool IsOver
        {
	        get
	        {
		        if (_sharedRift == null)
		        {
			        return false; // if in loading, not over
		        }
		        return _sharedRift.endTimestamp < CorgiTime.UtcNowULong;
	        }
        }
        
        
        // Rift 를 삭제할 타이밍
        public bool IsOverForDestroy()
        {
	        if(IsOver)
	        {
		        foreach (var dungeon in _dungeonMap.Values)
		        {
			        if (dungeon == null)
			        {
				        continue;
			        }

			        if (dungeon.State != DungeonState.Destroy)
			        {
				        // 아직 진행중인 dungeon이 있다면 끝난게 아님
				        return false;
			        }
		        }

		        return true;
	        }

	        return false;
        }

        public bool OpenRift(SharedRift sharedRift)
        {
	        if (sharedRift == null)
	        {
		        return false;
	        }

	        _sharedRift = sharedRift;
	        
			_sharedRift.maxHp = GetRiftMonsterHP(_sharedRift);
			_sharedRift.curHp = _sharedRift.maxHp;

	        return true;
        }
        
        public bool InitRift(SharedRift sharedRift)
        {
	        if (sharedRift == null)
	        {
		        return false;
	        }

	        _sharedRift = sharedRift;

	        if (_sharedRift.maxHp == 0)
	        {
		        _sharedRift.maxHp = GetRiftMonsterHP(_sharedRift);
		        _sharedRift.curHp = _sharedRift.maxHp;
	        }
	        
	        return true;
        }

        public bool JoinRift(string characterId, string dungeonKey, ICombatBridge bridge)
        {
	        if (_dungeonMap.ContainsKey(characterId))
	        {
		        // already Join 
		        return false;
	        }

	        var dungeon = new DungeonRift(bridge);
	        
	        dungeon.CharacterId = characterId;
	        dungeon.DungeonKey = dungeonKey;

	        _dungeonMap.Add(characterId, dungeon);

	        return true;
        }

        public DungeonLogNode OnJoinRift(string characterId, SharedRift riftInfo, ICombatBridge bridge)
        {
	        if (_dungeonMap.ContainsKey(characterId) == false)
	        {
		        // already Join 
		        return null;
	        }
	        var dungeon = _dungeonMap[characterId];
	        
	        // 둘중 하나는 null이 아니어야한다.
	        if(riftInfo == null && _sharedRift == null)
	        {
		        return null;
	        }

	        if (riftInfo != null)
	        {
				if (_sharedRift != null && _sharedRift.dungeonId != riftInfo.dungeonId)
				{
					// 이전 rift가 안끝났는데, 새로운 rift try가 들어옴
					return null;
				}

				_sharedRift = riftInfo;
				_sharedRift.maxHp = GetRiftMonsterHP(_sharedRift);
				_sharedRift.curHp = _sharedRift.maxHp;
	        }

	        dungeon.InitBossHP(_sharedRift.maxHp, _sharedRift.curHp);
	        
	        if (dungeon.Load(_sharedRift) == false)
	        {
		        return null;
	        }
	        
	        
	        var sharedDungeon = new SharedDungeon();
	        
	        sharedDungeon.Init(dungeon);
	        
            var finalList = bridge.CreateUnitList(dungeon);

            var ret = dungeon.EnterDungeon(finalList, _sharedRift.stageUid);

            if (ret != null)
            {
	            _sharedRift.JoinRiftChallenge(characterId);
	            _partyMemberEnterQueue.Enqueue(characterId);
	            SetPartyBuff(characterId);
	            _lastCumulativeDamageSentTimeMap[characterId] = 0;
            }
            
            return ret;
        }
        
        long GetRiftMonsterHP(SharedRift sharedRift)
        {
	        var stageUid = sharedRift.stageUid;
	        
		    var sheetData = _gameData.GetData<InstanceRiftInfoSpec>(stageUid);

		    if (sheetData == null )
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", stageUid);
			    return -1;
		    }

		    if (sheetData.BossUids.Count <= 0)
		    {
			    return -1;
		    }

		    var monsterUid = sheetData.BossUids[0];// only first mosnter
	        
	        var grade = sharedRift.grade;
		    var gradeStr = string.Format("rift_grade.{0}", grade);
		    var gradeSpec = _gameData.GetData<InstanceGradeInfoSpec>(gradeStr);
		    if (gradeSpec == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid rift dungeon grade uid : {0}", gradeStr);
			    return 0;
		    }

	        var gradeMod = 1f + gradeSpec.MonsterMod;

	        var level = sharedRift.level;
	        
			var constValue = _gameData.GetConfigNumber("config.combat.factor.stat.maxHP", 500);
            var sheet = _gameData.GetData<MonsterInfoSpec>(monsterUid);
	        var hpStat = (long)(Monster.GetStat(StatType.StMaxHp, sheet, level, constValue) * gradeMod);
	        
	        return hpStat;
        }


        public string GetDungeonKey(string characterId)
        {
	        if (_dungeonMap.ContainsKey(characterId) == false)
	        {
		        return String.Empty;
	        }

	        var dungeon = _dungeonMap[characterId];

	        return dungeon.DungeonKey;
	        
        }

        public List<DungeonLogNode> UpdateState(string characterId)
        {
	        // check dungeon for characterId
	        if (_dungeonMap.ContainsKey(characterId) == false)
	        {
		        return null;
	        }

	        var dungeon = _dungeonMap[characterId];

	        dungeon.UpdatePartyMemberEnterQueue(_partyMemberEnterQueue);

	        if (dungeon.IsLoaded == false)
	        {
		        return null;
	        }

	        if (dungeon.State == DungeonState.Destroy)
	        {
		        _dungeonMap.Remove(characterId);
		        return null;
	        }
	        
	        UpdateCumulativeDamage(characterId);
	        var logList = dungeon.UpdateState();

	        return logList;
        }

        public SharedRift GetSharedRiftInfo()
        {
	        if (_sharedRift == null)
	        {
		        return null;
	        }
	        return _sharedRift.Clone();
        }

        // share SharedRift for Client
        public SharedRift UpdateRiftInfo()
        {
			if (_isFirst)
			{
				_curSharedRiftTick = CorgiTime.UtcNowULong;
				_isFirst = false;
				
				return _sharedRift?.Clone();
			}
			
			
			var newTick = CorgiTime.UtcNowULong;
			var deltaTick = (newTick - _curSharedRiftTick);
			
			var curDelta = deltaTick;
			var maxTick = CorgiLogicConst.UpdateFrameForClient;
			
			if (_sharedRift != null && curDelta > maxTick)
			{
				_curSharedRiftTick = newTick;

				return _sharedRift.Clone();
			}
            return null;
        }

        public void OnRiftDungeonCompleted(string characterId)
        {
	        if (_dungeonMap.ContainsKey(characterId) == false)
	        {
		        CorgiCombatLog.LogFatal(CombatLogCategory.Dungeon, "invalid Rift Dungeon State");
		        return;
	        }
	        
	        _sharedRift.CompleteRiftChallenge(characterId);

	        var dungeon = _dungeonMap[characterId];
	        dungeon.OnRiftDungeonCompleted(0);

	        _lastCumulativeDamageSentTimeMap.Remove(characterId);
        }
        
        public void OnRiftDungeonStop(string characterId)
        {
	        if (_dungeonMap.ContainsKey(characterId) == false)
	        {
		        CorgiCombatLog.LogFatal(CombatLogCategory.Dungeon, "invalid Rift Dungeon State");
		        return;
	        }
	        
	        _sharedRift.CompleteRiftChallenge(characterId);

	        var dungeon = _dungeonMap[characterId];
	        dungeon.OnRiftDungeonStop();
	        _dungeonMap.Remove(characterId);
	        
	        _lastCumulativeDamageSentTimeMap.Remove(characterId);
        }

        public void OnUpdateRiftInfo()
        {
	        foreach (var dungeon in _dungeonMap.Values)
	        {
		        if (dungeon == null)
		        {
			        continue;
		        }

		        dungeon.OnUpdateRiftInfo(_sharedRift.curHp);
	        }
        }

        public ulong GetStageUid()
        {
	        return _sharedRift.stageUid;
        }

        public uint GetGrade()
        {
	        return _sharedRift.grade;
        }
        
        private void SetPartyBuff(string characterId)
        {
	        var charIdList = new List<string>(_dungeonMap.Keys);
	        
	        var dungeon = _dungeonMap[characterId];
	        if (!dungeon.SetPartyBuff(charIdList))
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.System, "failed to SetPartyBuff");
	        }
        }

        public void ClearPartyMemberEnterQueue()
        {
	        if (_partyMemberEnterQueue.Count > 0)
	        {
		        _partyMemberEnterQueue.Clear();
	        }
        }

        private void UpdateCumulativeDamage(string characterId)
        {
	        if (!_lastCumulativeDamageSentTimeMap.ContainsKey(characterId))
	        {
		        return;
	        }
	        
	        var curCombatTick = _dungeonMap[characterId].CurCombatTick;

	        if (curCombatTick - _lastCumulativeDamageSentTimeMap[characterId] < _sendingCumulativeDamageInterval)
	        {
		        return;
	        }

	        var dungeon = _dungeonMap[characterId];
	        foreach (var otherDungeon in _dungeonMap.Values)
	        {
		        otherDungeon.HaveToSendCumulativeDamage = true;
		        otherDungeon.CumulativeDamageMap[characterId] = dungeon.CumulativeDamage;
	        }
	        dungeon.CumulativeDamage = 0;
	        _lastCumulativeDamageSentTimeMap[characterId] += _sendingCumulativeDamageInterval;
        }
    }
}