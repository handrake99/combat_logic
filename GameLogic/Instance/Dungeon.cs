using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Utils;

using IdleCs.Managers;
using IdleCs.Network;
	
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IdleCs.GameLogic
{
    public class Dungeon : CorgiObject, ITickable
    
    {
	    // tick
	    //private ulong _prevTimeStamp = CorgiTime.UtcNowULong;
	    
	    private ulong _curTick = 0L;
	    
	    // stage tick include exploration time
	    private ulong _curStageTick = CorgiLogicConst.MinimumStageTime;
	    public ulong CurStageTick => _curStageTick;
	    
	    // combat tick 
	    private ulong _curCombatTick = 0;
	    public ulong CurCombatTick => _curCombatTick;

	    /// <summary>
	    /// static data
	    /// </summary>
	    /// 
	    private ICombatBridge _bridge;

	    private GameDataManager _gameData;

	    public GameDataManager GameData => _gameData;

	    private DungeonType _dungeonType;

	    public ICombatBridge Bridge => _bridge;

	    public CombatRandom Random { get; private set; }
	    public DungeonType DungeonType => _dungeonType;
	    public AreaType AreaType { get; protected set; }
	    

	    /// <summary>
	    /// dynamic data
	    /// </summary>
	    ///
	    private bool _isPause = false;
	    public bool IsPause
	    {
		    get { return _isPause; }
	    }
	    private readonly List<Unit> _charList = new List<Unit>();
	    private readonly List<Unit> _structureList = new List<Unit>();

	    bool _isFirst = true;
	    protected CorgiValue<DungeonState> _state;
	    protected int _curStageIndex = 0;
	    protected Stage _curStage = null;
	    protected ulong _nextStageUid = 0;
	    protected bool IsWin;


	    public ulong StackCount = 0;
	    public ulong DoApplyCallCount = 0;

	    
	    public List<Unit> CharList { get { return _charList; }}
	    public List<Unit> MonsterList => (_curStage == null) ? null : _curStage.MonsterList;
	    public List<Unit> StructureList { get { return _structureList; }}
	    public DungeonState State { get { return _state.Value; }}
	    public CorgiValue<DungeonState> OriginalState
	    {
		    get { return _state; }
	    }

	    public int CurStageIndex => _curStageIndex;
	    public Stage CurStage => _curStage;

	    protected virtual int GetStageCount()
	    {
		    return 0;
	    }
	    
	    public List<ulong> AffixList { get; protected set; }
	    public List<ulong> PartyBuffList { get; protected set; }

	    public virtual bool GetIsChallenging()
	    {
		    return false;
	    }

	    public virtual uint GetChallengeCount()
	    {
		    return 0;
	    }

	    public virtual bool GetAllowExtermination()
	    {
		    return true;
	    }

	    public Dungeon(ICombatBridge bridge, DungeonType dungeonType)
	    {
		    SetReload();
		    _bridge = bridge;
		    _dungeonType = dungeonType;
		    
		    Random = new CombatRandom();
		    var seed = (int)CorgiTime.UtcNow.Ticks & 0x0000FFFF;
		    Random.SetSeed(seed);
		    
		    AffixList = new List<ulong>();
		    PartyBuffList = new List<ulong>();
		    _gameData = GameDataManager.Instance;
		    
		    //CorgiLog.Log(CorgiLogType.Info, "called in common logic);
	    }

	    protected bool AddExternalSkill(ulong skillUid, ulong skillTimestamps)
	    {
		    if (skillUid == 0)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"[Dungeon] invalid external buff input {0}", skillUid);
			    return false;
		    }

		    if (skillTimestamps == 0)
		    {
			    return false;
		    }
		    
		    var structure = new InvisibleStructure(this);
		    if (structure.Load(0) == false)
		    {
			    return false;
		    }
		    
		    
		    structure.AddSkill(skillUid, skillTimestamps);

		    _structureList.Add(structure);

		    return true;
	    }

	    protected bool RemoveExternalSkill(ulong skillUid)
	    {
		    foreach (var structure in _structureList)
		    {
			    var thisStructure = structure as InvisibleStructure;
			    if (thisStructure != null && thisStructure.RemoveSkill(skillUid))
			    {
				    _structureList.Remove(thisStructure);
				    return true;
			    }
		    }

			CorgiCombatLog.Log(CombatLogCategory.Dungeon, "[Dungeon] invalid external buff Remove {0}", skillUid);
		    return false;
	    }
        
        public Unit GetUnit(string objectId)
        {
            for (var i = 0; i < CharList.Count; i++)
            {
                Unit curUnit = CharList[i];
                if (curUnit != null && curUnit.IsSame(objectId))
                {
                    return curUnit;
                }
            }

            var monsterList = MonsterList;
            if (monsterList != null)
            {
	            for (var i = 0; i < monsterList.Count; i++)
	            {
		            Unit curUnit = monsterList[i];
		            if (curUnit != null && curUnit.IsSame(objectId))
		            {
			            return curUnit;
		            }
	            }
            }
            
            if (StructureList != null)
            {
	            for (var i = 0; i < StructureList.Count; i++)
	            {
		            Unit curUnit = StructureList[i];
		            if (curUnit != null && curUnit.IsSame(objectId))
		            {
			            return curUnit;
		            }
	            }
            }
            
            //CorgiLog.LogError(string.Format("invalid Unit Object Id[{0}]", objectId));
            
            return null;
        }
        
	    public List<Unit> GetFriends(string unitId)
	    {
		    var unit = GetUnit(unitId);

		    return GetFriends(unit);
	    }

	    public List<Unit> GetFriends(Unit unit)
	    {
		    if ((_state != DungeonState.EnterCombat && _state != DungeonState.InCombat) || _curStage == null)
		    {
				CorgiCombatLog.LogError(CombatLogCategory.Dungeon,string.Format("not in combat now. "));
			    return null;
		    }
		    
		    var retList = new List<Unit>();
		    if (unit.CombatSideType == CombatSideType.Player)
		    {
			    foreach (var curUnit in CharList)
			    {
				    retList.Add(curUnit);
			    }
		    }
		    else
		    {
			    var monsterList = MonsterList;

			    if (monsterList != null)
			    {
					foreach (var curUnit in MonsterList)
					{
						 retList.Add(curUnit);
					}
			    }
		    }

		    return retList;
		    
	    }
	    
	    
	    public List<Unit> GetEnemies(string unitId)
	    {
		    var unit = GetUnit(unitId);
		    if (unit == null)
		    {
			    return null;
		    }
		    if ((_state != DungeonState.EnterCombat && _state != DungeonState.InCombat )
		        || _curStage == null)
		    {
				CorgiCombatLog.LogError(CombatLogCategory.Skill,string.Format("not in combat now. "));
			    return null;
		    }
		    
		    var retList = new List<Unit>();
		    if (unit.CombatSideType == CombatSideType.Player)
		    {
			    var monsterList = MonsterList;
			    if (monsterList != null)
			    {
					foreach (var curUnit in monsterList)
					{
						 retList.Add(curUnit);
					}
			    }
		    }
		    else
		    {
			    foreach (var curUnit in CharList)
			    {
				    retList.Add(curUnit);
			    }
		    }

		    return retList;
	    }

	    public void Pause(ulong nextUid, DungeonLogNode logNode)
	    {
		    _isPause = true;

		    if (logNode != null)
		    {
				//logNode.IsPause = true;
				logNode.PauseLogNode = new PauseLogNode();
				logNode.PauseLogNode.NextGuideUid = nextUid;
		    }
	    }

	    public void Resume()
	    {
		    _isPause = false;
		    CorgiCombatLog.Log(CombatLogCategory.Dungeon, "[Tutorial] Resume Dungeon");
	    }

	    protected void UpdateCharacters(List<Unit> unitList)
	    {
            UpdateCharList(unitList);
	        
	        UpdateCharacters(false);
	    }

	    private void UpdateCharList(List<Unit> unitList)
	    {
		    _charList.Clear();
		    var newCharList = FillNpc(unitList);
			 
		    _charList.AddRange(newCharList);
	    }

	    public Unit CreateCharacter(SharedMemberInfo memberInfo)
	    {
            var unit = new Character(this);
            var charInfo = memberInfo.character;
            
            if (unit.Load(charInfo) == false)
            {
                CorgiLog.LogError("invalid character info");
                return null;
            }

            return unit;
	    }
	    
	    // room 의 member info 가 업데이트 될때 리셋
	    public void UpdateMemberInfos(List<SharedMemberInfo> memberInfos)
	    {
		    if (memberInfos == null)
		    {
			    return;
		    }
		    
            _charList.Clear();
            
            var characters = new List<Unit>();
            
            foreach (var memberInfo in memberInfos)
            {
                // characters
                var unit = CreateCharacter(memberInfo);
                if (unit == null)
                {
                    CorgiLog.LogError("invalid character info");
                    continue;
                }
                
                characters.Add(unit);
            }
            
			var newCharList = FillNpc(characters);
			 
	        _charList.AddRange(newCharList);
	        
	        UpdateCharacters();
	    }

        public virtual DungeonLogNode EnterDungeon(List<Unit> unitList, ulong curStageUid)
        {
	        var retLogNode = new DungeonLogNode(_state);
            if (unitList == null)
            {
                return null;
            }
            
		    _gameData = GameDataManager.Instance; // set current gamedata
		    
		    UpdateCharList(unitList);

	        _nextStageUid = curStageUid;
	        
	        OnEnterDungeon();
	        
            var dungeonInfo = new SharedDungeon();
            dungeonInfo.Init(this);
            retLogNode.SharedInstance = dungeonInfo;
	        
            _state.Set(DungeonState.Exploration);
            
            return retLogNode;
        }

        protected void OnEnterDungeon()
        {
	        // sorting
	        _charList.Sort(delegate(Unit x, Unit y)
	        {
		        var a = (int)x.ClassType;
		        var b = (int)y.ClassType;
		        if (a == 0 && b == 0) return 0;
		        else if (a < b) return -1;
		        else if (a > b) return 1;

		        return 0;
	        });
	        
	        // // set character positions
	        SetCharPositions(_charList);

			CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Enter Dungeon : {0}", ObjectId);
        }

        //-초대 받은 파티멤버를 던전의 캐릭터리스트에 추가시킨다.
        //	추가되는 파티멤버는 동일한 클래스의 NPC를 삭제 시키고 추가된다. 
        public bool JoinDungeon(Unit joinUnit)
        {
	        if (joinUnit == null)
	        {
		        return false;
	        }
	        
	        return true;
        }

        //-파티멤버가 파티를 떠날 때, 자신의 클래스에 맞는 NPC를 채워놓고 떠난다. 
        public void LeaveDungeon(string dbId)
        {    
	        int findIndex = _charList.FindIndex(element =>
	        {
		        var character = element as Character;
		        if (null != character)
		        {
			        return dbId == element.DBId;
		        }

		        return false;
	        });
	        
	        uint minLevel = 0;
	        Unit addUnit = null;
	        
	        var leaveUnit = _charList[findIndex];
	        if (RoleType.RtDealer == leaveUnit.RoleType)
	        {
		        //-1. 나가는 놈이 딜러라면, 클래스타입 팀을 구성하고,

		        var defClassTypeList = Enum.GetValues(typeof(ClassType)).Cast<ClassType>()
			        .Where(element => { return element != ClassType.CtNone; }).ToList();

		        _charList.All(element =>
		        {
			        defClassTypeList.Remove(element.ClassType);
			        return true;
		        });

		        foreach (var remainClassType in defClassTypeList)
		        {
			        
			        minLevel = _charList.Min(element => element.Level);
			        addUnit = CreateNpc(remainClassType, minLevel);

			        addUnit.ResetPosition(leaveUnit.InitialPosition, leaveUnit.Position,leaveUnit.ArrivalPosition);

			        _charList[findIndex] = null;
			        _charList[findIndex] = addUnit;

			        break;
		        }
	        }
	        else
	        {
		        //-2. 나가는 놈이 딜러가 아니면 나간 놈과 동일한 클래스로 채운다.
		        
		        minLevel = _charList.Min(element => element.Level);
		        addUnit = CreateNpc(leaveUnit.ClassType, minLevel);

		        addUnit.ResetPosition(leaveUnit.InitialPosition, leaveUnit.Position,leaveUnit.ArrivalPosition);
		        
		        _charList[findIndex] = null;
		        _charList[findIndex] = addUnit;
	        }

	        //->>
	        minLevel = _charList.Where(element => 
	        {
		        var character = element as Character;
		        return (null != character);

	        }).ToList().Min(element => element.Level);
	        
	        //-reset npc by min level,
	        foreach (var character in _charList)
	        {
		        var npc = character as Npc;
		        if (null != npc){ UpdateNpc(npc,  minLevel); }
	        }
	        
	        //_bridge.NtfUpdateUnit(addUnit, leaveUnit.ObjectId);
	        _bridge.CachedUpdateUnit(addUnit, leaveUnit.ObjectId);
        }
        
        public virtual void UpdateStage(ulong curStageUid)
        {
	        _nextStageUid = curStageUid;
        }
        
        // character info update
        public void UpdateCharacters(bool isDataUpdated = false)
        {
	        // update characters db data
	        _bridge.UpdateUnitList(this);
	        
	        uint minLevel = 0;
	        
            foreach (var unit in _charList)
            {
	            var character = unit as Character;
	            if (character == null)
	            {
		            continue;
	            }
	            
	            if (minLevel == 0 || minLevel > character.Level)
	            {
		            minLevel = character.Level;
	            }
            }

            var skillUids = new List<ulong>();
	        foreach (var unit in _charList)
	        {
		        var character = unit as Character;
		        if (character == null)
		        {
			        continue;
		        }

		        if (isDataUpdated)
		        {
			        character.Load(character.Uid);
		        }
		        
                var deck = GetCurrentDeck(unit);
                if (character.LoadCombatSetting(deck) == false)
                {
	                //CorgiLog.LogError("invalid character deck info");
	                CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "LoadCombatSetting error by deckinfo from UpdateCharacters. characteruid[{0}]", character.Uid);
                    continue;
                }
                
                if (skillUids.Count == 0 && unit.ActiveSkills.Count == 4)
                {
                    skillUids.Add(unit.ActiveSkills[0].Uid);
                    skillUids.Add(unit.ActiveSkills[1].Uid);
                    skillUids.Add(unit.ActiveSkills[2].Uid);
                    skillUids.Add(unit.ActiveSkills[3].Uid);
                }
	        }
	        
            foreach (var unit in _charList)
            {
	            var npc = unit as Npc;
	            if (npc == null)
	            {
		            continue;
	            }
	            npc.UpdateLevel(minLevel);
	            npc.SetSkills(skillUids);

	            if (isDataUpdated)
	            {
		            npc.Load(npc.Uid);
	            }
            }
	        
	        _charList.Sort(delegate(Unit x, Unit y)
	        {
		        var a = (int)x.ClassType;
		        var b = (int)y.ClassType;
		        if (a == 0 && b == 0) return 0;
		        else if (a < b) return -1;
		        else if (a > b) return 1;

		        return 0;
	        });
	        
	        // // set character positions
	        SetCharPositions(_charList);
        }

        protected virtual Deck GetCurrentDeck(Unit unit)
        {
	        return null;
        }
        

        public virtual Stage CreateNewStage(ulong nextStageUid)
        {
	        return null;
        }
        
        protected virtual void Exploration(DungeonLogNode logNode)
        {
			foreach (var curUnit in CharList)
			{
				curUnit.OnExploration(logNode);
			}
			
            var dungeonInfo = new SharedDungeon();
            dungeonInfo.Init(this);

            logNode.SharedInstance = dungeonInfo;
        }

        protected virtual void EnterStage(DungeonLogNode logNode)
        {
	        if (_curStage != null )
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"invalid state. prev stage was not over yet.");
		        return ;
	        }

	        uint preRevision = 0;
	        if (_gameData != null)
	        {
		        //CorgiLog.LogLine("Prev Game data is {0}", _gameData.Revision);
		        preRevision = _gameData.Revision;
	        }
		    _gameData = GameDataManager.Instance; // set current gamedata
		    var curRevision = _gameData.Revision;
		    
			//CorgiLog.LogLine("New Game data is {0}", _gameData.Revision);
	        
	        UpdateCharacters(preRevision != curRevision);

	        _curStage = CreateNewStage(_nextStageUid);
	        if (_curStage == null)
	        {
		        throw new CorgiException("Critical!, Invalid stage uid[{0}] when Enter Stage.", GameData.GetStrByUid(_nextStageUid));
	        }
	        
	        var sceneTime = _curStage.GetBossSceneTime();
	        if (sceneTime > 0)
	        {
		        _state.Set(DungeonState.InBossScene);
	        }
	        else
	        {
				_state.Set(DungeonState.EnterCombat);
	        }

            OnEnterStage(logNode);
        }

        protected void OnEnterStage(DungeonLogNode logNode)
        {
	        _nextStageUid = 0;
	        _curStageTick = 0;
	        _curCombatTick = 0;

	        //CorgiLog.LogLine("On EnterStage state {0} / {1}" ,_state.ToString(), _curStage?.Uid);
	        CorgiCombatLog.Log(CombatLogCategory.Dungeon, "On EnterStage state [{0}]/[{1}]", _state.Value.ToString(), (null != _curStage) ? (_curStage?.Uid) : 0);  
	        
        }

	    protected void OnEnterCombat(DungeonLogNode logNode)
	    {
		    _curCombatTick = 0;
		    
	        var curLog = new StageLogNode(_curStage);
		    curLog.DungeonLogNode = logNode;
		    
		    DoUnitAction((unit) =>
		    {
		        unit.OnEnterCombat(curLog);
		    });
		    
		    DoUnitAction((unit) =>
		    {
				unit.OnUpdateEffect(curLog);
		    });
		    
		    logNode.SetCombatLog(curLog);
	    }

	    private ulong _waitingTime;
	    private DungeonState _waitNextState;
	    private bool _isRequesting;
	    

	    bool IsWaiting{get{
		    return _waitingTime > CorgiLogicConst.CombatFrame || _isRequesting;
	    }}
	    public void DoWaiting(ulong waitingTime, DungeonState nextState, List<RequestParam> paramList)
	    {
		    if (waitingTime == 0)
		    {
			    _state.Set(nextState);
			    return;
		    }
		    
		    _waitingTime = waitingTime;
		    _waitNextState = nextState;
		    if (paramList == null || paramList.Count == 0)
		    {
				CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Waiting Start : {0}", waitingTime);
			    return;
		    }

			if (_bridge.RequestData(string.Empty, paramList, "OnLoadRequestData") == false)
			{
				 throw new CorgiException("can't request data");
			}
		    
			_isRequesting = true;

		    CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Waiting Start For DataLoading : {0}", waitingTime);
	    }

	    public void TickWaiting(ulong deltaTime, LogNode logNode)
	    {
		    var dungeonNode = (DungeonLogNode) logNode;
		    
		    var childNode = new TickLogNode(deltaTime);
		    childNode.DungeonLogNode = dungeonNode;
		    
			foreach (var curUnit in CharList)
			{
				 curUnit.TickInCombat(deltaTime, childNode);
			}
		    
		    if (_waitingTime > deltaTime)
		    {
			    _waitingTime -= deltaTime;
		    }
		    else
		    {
			    _waitingTime = 0;
		    }
		    
			//CorgiLog.LogLine("Waiting ing {0}", deltaTime);
			if (_bridge.IsRequestCompleted() == false)
			{
				_isRequesting = true;
			}
			else
			{
//				if (_bridge.IsUpdatedParty())
//				{
//					var finalList = _bridge.CreateUnitList(this);
//					_charList.Clear();
//					_charList.AddRange(finalList);
//					UpdateLevel();
//					UpdateCurrentDeck();
//				}
				
				_isRequesting = false;
			}

			// End Waiting & no reset waiting
		    if (IsWaiting == false && _waitNextState != DungeonState.None)
		    {// end Waiting
			    _waitingTime = 0;
			    _state.Set(_waitNextState);

			    CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Waiting End");
		    }
		    dungeonNode.SetCombatLog(childNode);
	    }

	    public void TickInCombat(ulong deltaTime, LogNode logNode)
	    {
		    DoApplyCallCount = 0;

		    var dungeonNode = (DungeonLogNode) logNode;

		    if (dungeonNode == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid dungeon log node");
			    return;
		    }

		    _curCombatTick += deltaTime;
		    
		    var childNode = new TickLogNode(deltaTime);
		    childNode.DungeonLogNode = dungeonNode;
		    
		    DoUnitAction((unit) =>
		    {
				 unit.TickInCombat(deltaTime, childNode);
		    });

		    if (State == DungeonState.InCombat && (IsStageWin() || IsStageLose()))
		    {
			    if (IsStageWin())
			    {
				    IsWin = true;
			    }
			    if(IsStageLose())
			    {
				    IsWin = false;
			    }
			    
			    OnFinishCombat(childNode);
		    }
		    
		    dungeonNode.SetCombatLog(childNode);
			//childNode.LogDebug(_bridge);
	    }
	    
	    // private ulong _tempTimer;
	    //
	    // protected bool IsTimerOver{get{
		   //  return _tempTimer <= CorgiLogicConst.CombatFrame;
	    // }}

	    
	    protected CorgiTimer StopTimer = new CorgiTimer();
	    
	    // public void StopTimer(ulong waitingTime)
	    // {
		   //  //_tempTimer = waitingTime;
		   //  _stopTimer.StartTimer(waitingTime);
	    //
		   //  CorgiLog.Log(CorgiLogType.Debug, "Do Timer {0}", waitingTime);
	    // }

	   //  public void TickTimer()
	   //  {
		  //   _stopTimer.Tick();
		  // //   if (_tempTimer > deltaTime)
		  // //   {
			 // //    _tempTimer -= deltaTime;
				// // CorgiLog.Log(CorgiLogType.Debug, "Run Timer {0}/{1}", deltaTime, _tempTimer);
		  // //   }
		  // //   else
		  // //   {
			 // //    _tempTimer = 0;
		  // //   }
	   //  }

	    protected virtual void DoUnitAction(Action<Unit> action)
	    {
			foreach (var curUnit in CharList)
			{
				action(curUnit);
			}

			var monsterList = MonsterList;
			if (monsterList != null)
			{
				var monCount = monsterList.Count;
				for(var i=0; i<monCount ; i++)
				{
					var curUnit = monsterList[i];
					action(curUnit);
				}
			}
			
			foreach (var curUnit in _structureList)
			{
				action(curUnit);
			}
	    }

	    public virtual void OnFinishCombat(CombatLogNode logNode)
	    {
			_state.Set(DungeonState.FinishStage);
			
	        DoUnitAction((unit) =>
	        {
		        unit.OnFinishCombat(logNode);
		        
	        });
	    }
	    
	    
        protected void FinishStage(DungeonLogNode logNode)
        {
	        //_curStageIndex++;
	        
	        var childNode = new StageLogNode(_curStage);
		    childNode.DungeonLogNode = logNode;

	        OnFinishStage(childNode);
	        
	        logNode.SetCombatLog(childNode);
	        
	        CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Finish stage. roomid[{0}], stageuid[{1}]. _curStage will be null", _bridge.Id(), _curStage.Uid);
	        //_curStage = null;
        }

        protected virtual void OnFinishStage(CombatLogNode logNode)
        {
	        DoUnitAction((unit) =>
	        {
		        unit.OnFinishStage(logNode);
		        
	        });
	        
	        //CorgiLog.LogLine("Finish Stage : {0}", ObjectId);
        }

        void FinishDungeon()
        {
	        //test
	        // go to next dungeon?
	        OnFinishDungeon();
        }

        void OnFinishDungeon()
        {
	        CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Finish Dungeon : {0}", ObjectId);
        }

	    public void OnAction(CombatLogNode skillActionLogNode)
		{
			//CorgiLog.LogWarning("DungeonManager OnSkill Called");

			DoUnitAction((unit) =>
			{
				unit.OnUpdateEffect(skillActionLogNode);
				
			});
		}

        public void OnDeadCompletely(Unit unit, CombatLogNode logNode)
        {
	        var eventParam = new EventParamUnit(unit);
	        
	        var monsterAliveCount = 0;
	        var characterAliveCount = 0;
	        Unit aliveMonster = null;
	        Unit aliveCharacter = null;
	        var isBossLive = false;
	        var isBossExist = false;

	        var monsterList = MonsterList;

	        if (monsterList != null)
	        {
		        foreach (var curUnit in monsterList)
		        {
			        var monster = curUnit as Monster;
			        if (monster == null)
			        {
				        continue;
			        }
			        if (monster.IsBoss)
			        {
				        isBossExist = true;
			        }
			        
			        if (curUnit.IsLive())
			        {
				        ++monsterAliveCount;
				        aliveMonster = curUnit;
				        if (monster.IsBoss)
				        {
					        isBossLive = true;
				        }
			        }
		        }
	        }

	        foreach(var curUnit in _charList)
            {
                if(curUnit.IsLive())
                {
	                ++characterAliveCount;
	                aliveCharacter = curUnit;
                }
            }

	        // check combat over
			if (isBossExist && isBossLive == false)
			{
				if (monsterList != null)
				{
					foreach (var mon in monsterList)
					{
						if (mon.IsLive())
						{
							mon.ResetHP(0);
							DieLogNode dieNode = new DieLogNode(mon);
							logNode.AddChild(dieNode);
							
							//CorgiLog.LogWarningForce($"new DieLogNode in {logNode}");
						}
					}
					
				}
	            return;
            }

	        if(characterAliveCount == 0)
	        {
		        return;
	        }

	        if (monsterAliveCount == 1)
	        {
		        aliveMonster?.OnEvent(CombatEventType.OnFriendDeadAll, eventParam, logNode);
	        }
	        else if (characterAliveCount == 1)
	        {
		        aliveCharacter?.OnEvent(CombatEventType.OnFriendDeadAll, eventParam, logNode);
	        }

        }

        public void OnStageCompleted(ulong stageUid)
        {
            if (State != DungeonState.Win && State != DungeonState.Lose)
            {
	            if (GetIsChallenging())
	            {
		            return;
	            }
                throw new CorgiException("invalid dungeon state({0}) in OnStageCompleted", State);
            }

            _nextStageUid = stageUid;

            _state.Set(DungeonState.RewardCompleted);
        }


        protected virtual void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
	        StackCount = 0;
	        DoApplyCallCount = 0;
	        switch (_state.Value)
	        {
		        case DungeonState.Exploration:
			        // _bridge.UpdateUnit();//-파티에 가입하거나 떠나는 캐릭터의 정보를 클라이언트에게 알려주는 시점.
			        Exploration(logNode);
			        DoWaiting(GetExploreTime(), DungeonState.EnterStage, GetRequireParams());
			        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.Exploration);
			        break;
				case DungeonState.EnterStage:
					EnterStage(logNode);
			        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.EnterStage);
					break;
				case DungeonState.InBossScene:
					var waitingTime = _curStage.GetBossSceneTime();
			        DoWaiting(waitingTime, DungeonState.EnterCombat, null);
					break;
				case DungeonState.EnterCombat:
					OnEnterCombat(logNode);
			        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.EnterCombat);
					_state.Set(DungeonState.InCombat);
					break;
				case DungeonState.InCombat:
					TickInCombat(deltaTick, logNode);
					break;
				case DungeonState.FinishStage:
					FinishStage(logNode);
			        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.FinishStage);
					break;
				case DungeonState.Win: 
				case DungeonState.Lose:

					// 시간이 지나면 강제로 RewardCompleted로 보내버린다.
					if (StopTimer.IsOver())
					{
						Bridge.DoDestroy(6);
					}
					else
					{
						//-애초에 이벤트 방식이 아닌 폴링 방식 구조를 잡았기 때문에. 여기로 계속 호출될 수가 있음.........
						Bridge.DungeonStateWaitLog(_state.Value.ToString());
						StopTimer.Tick();
					}
					
					break;
				case DungeonState.RewardCompleted:
					_state.Set(DungeonState.StageCompleted);
					//DoWaiting(CorgiLogicConst.StageDelay, DungeonState.StageCompleted, null);
			        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.RewardCompleted);
					break;
				case DungeonState.StageCompleted:
					ResetDungeonState();
					_curStage = null;
                    // for stage complete 
					DoWaiting(CorgiLogicConst.StageDelay, DungeonState.Exploration, null);
					break;
				case DungeonState.FinishDungeon:
					_state.Set(DungeonState.Destroy);
					break;
				case DungeonState.Destroy:
					OnDestroy();
					break;
				case DungeonState.None:
				default:
					CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"An Unsupported StageState : {0} -> {1}\n",_state.Prev, _state.Value);
					break;
			}
	        
        }

        protected void ResetDungeonState()
        {
			_waitingTime = 0;
			_waitNextState = DungeonState.None;
        }
	    
		public List<DungeonLogNode> UpdateState()
		{
			
			var retLogNodeList = new List<DungeonLogNode>();

			if (_isFirst)
			{
				_curTick = CorgiTime.UtcNowULong;
				_isFirst = false;
			}

			
			var newTick = CorgiTime.UtcNowULong;
			var deltaTick = (newTick - _curTick);
			
			
			var curDelta = deltaTick;
			var maxTick = CorgiLogicConst.CombatFrame;
			
			// for safe code
			var maxCount = 10;
			var curCount = 0;
			  
			// fixed tick with 100ms
			while (curDelta > maxTick && curCount++ < maxCount)
			{
				if (!_isPause)
				{
#if COMBAT_SERVER_DEBUG
					 var stopWatch = new Stopwatch();
					 stopWatch.Start();
#endif
					 _state.OnUpdated();
					 var retLogNode = new DungeonLogNode(DungeonState.None);
					 retLogNode.UpdateDungeonState(_state.Value);
					 
					 if (IsWaiting)
					 {
						  // waiting
						  TickWaiting(maxTick, retLogNode);
					 }
					 else
					 {
						  Tick(maxTick, retLogNode);
					 }
					 retLogNode.UpdateDungeonState(_state.Prev);
					 
					 retLogNode.SharedInstance = GetSharedInstance();
					 retLogNode.SetRoot(); // for reference

					 retLogNodeList.Add(retLogNode);
					 
					 if (_state.Value != _state.Prev)
					 {
						 //CorgiLog.LogLine("Change DungeonState {0} to {1}", _state.Prev, _state.Value);
						 CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Change DungeonState [{0}] to [{1}] int Dungeon[{2}]", _state.Prev, _state.Value, DungeonType);
						 if (CurStage != null)
						 {
							 CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Stage State [{0}] ", GameDataManager.GetStringByUid(CurStage.Uid));
						 }
					 }
					 
#if COMBAT_SERVER_DEBUG
					 stopWatch.Stop();
					 CorgiCombatLog.Log(CombatLogType.Dungeon,$"DungeonTick {stopWatch.Elapsed}/{curCount}/{deltaTick}");
#endif
				}
				
				curDelta -= maxTick;
				_curTick += maxTick;
				_curStageTick += maxTick;
			}
			
#if COMBAT_SERVER_DEBUG
			CorgiCombatLog.Log(CombatLogType.Dungeon,$"[Dungeon] update state {curCount}/{maxCount}: {curDelta}/{deltaTick}");
			CorgiCombatLog.Log(CombatLogType.Dungeon,$"[Dungeon] update state {newTick}:{_curTick}");
#endif

			return retLogNodeList;
		}

		protected virtual SharedInstance.SharedInstance GetSharedInstance()
		{
			var sharedDungeon = new SharedDungeon();
			sharedDungeon.Init(this);

			return sharedDungeon;
		}
		
	    public float GetDistance(Unit actor, Unit target)
	    {
		    if (actor == null || target == null)
			    return 99999f;

		    return CorgiPosition.Distance(actor.Position, target.Position);
	    }

	    public Unit GetNearestEnemy(Unit actor, int order = 1, bool isAlive = true)
	    {
		    //float minDistance = 9999f;
		    Unit nearestUnit = null;

		    List<Unit> targetList = GetEnemies(actor.ObjectId);

		    if (targetList == null || targetList.Count < order)
		    {
			    return null;
		    }
		    
		    targetList.Sort(delegate(Unit unit1, Unit unit2) {  
			    var d1 = GetDistance(actor, unit1);
			    var d2 = GetDistance(actor, unit2);

			    if (d1 == d2) return 0;
			    else if (d1 < d2) return -1;
			    else if (d1 > d2) return 1;

			    return 0;
		    });

		    var curIndex = 0;

		    foreach (var curUnit in targetList)
		    {
			    //float distance = GetDistance(actor, curUnit);
			    //if (distance < minDistance && curUnit.IsLive() == isAlive)
			    if (curUnit.IsLive() == isAlive)
			    {
				    curIndex++;
				    if (curIndex >= order)
				    {
						 nearestUnit = curUnit;
						 break;
				    }
			    }
		    }
		    return nearestUnit;
	    }
	    
	    public Unit GetNearestFriend(Unit actor, int order = 1, bool isAlive = true)
	    {
		    //float minDistance = 9999f;
		    Unit nearestUnit = null;

		    List<Unit> targetList = GetFriends(actor.ObjectId);

		    if (targetList == null || targetList.Count < order)
		    {
			    return null;
		    }
		    
		    targetList.Sort(delegate(Unit unit1, Unit unit2) {  
			    var d1 = GetDistance(actor, unit1);
			    var d2 = GetDistance(actor, unit2);

			    if (d1 == d2) return 0;
			    else if (d1 < d2) return -1;
			    else if (d1 > d2) return 1;

			    return 0;
		    });

		    var curIndex = 0;

		    foreach (var curUnit in targetList)
		    {
			    //float distance = GetDistance(actor, curUnit);
			    //if (distance < minDistance && curUnit.IsLive() == isAlive)
			    if (curUnit.IsLive() == isAlive && curUnit.ObjectId != actor.ObjectId)
			    {
				    curIndex++;
				    if (curIndex >= order)
				    {
						 nearestUnit = curUnit;
						 break;
				    }
			    }
		    }
		    return nearestUnit;
	    }

	    public virtual List<Unit> FillNpc(List<Unit> unitList)
	    {
		    // check char skill for npc skill
            var skillUids = new List<ulong>();
            
		    foreach (var unit in unitList)
		    {
			    if (unit == null)
			    {
				    continue;
			    }
			    
                if (skillUids.Count == 0 && unit.ActiveSkills.Count == 4)
                {
                    skillUids.Add(unit.ActiveSkills[0].Uid);
                    skillUids.Add(unit.ActiveSkills[1].Uid);
                    skillUids.Add(unit.ActiveSkills[2].Uid);
                    skillUids.Add(unit.ActiveSkills[3].Uid);
                }

                break;
		    }
		    
            var classTypeList = new List<ClassType>();
            var units = new List<Unit>();
            uint minLevel = 0;

            uint dealerCount = 0;
            
            foreach (var unit in unitList)
            {
	            if (unit == null)
	            {
		            continue;
	            }
	            
	            classTypeList.Add(unit.ClassType);
	            units.Add(unit);

	            if (RoleType.RtDealer == unit.RoleType)
	            {
		            ++dealerCount;
	            }
	            
	            if (minLevel == 0 || minLevel > unit.Level)
	            {
		            minLevel = unit.Level;
	            }
	            
                if (units.Count == 4)
                {
                    return units;
                }           
            }

//            if (units.Count == 4)
//            {
//	            return units;
//            }
		    
            // test code // check load
            for (var i = 0; i < 4; i++)
            {
                var index = i + 1;
                var charUid = GameDataManager.GetUidByString("character.npc." + index);
                var sheet = GameData.GetData<CharacterInfoSpec>(charUid);

                if (sheet == null)
                {
                    continue;
                }
                var classType = sheet.ClassType;

                if ((2 <= dealerCount) && (RoleType.RtDealer == sheet.RoleType))
                {
	                continue;
                }
                
                if (classTypeList.Contains(classType))
                {
                    continue;
                }

                var inst = CreateNpcInstance();
                inst.SetLevel(minLevel);
                
                if (inst.Load(charUid) == false)
                {
                    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"[Dungeon] invalid char uid {0}", charUid);
                    continue;
                }
                
                inst.SetSkills(skillUids);

                units.Add(inst);
                if (units.Count == 4)
                {
                    break;
                }
            }
            
            return units;
	    }

	    public virtual Npc CreateNpcInstance()
	    {
		    return new Npc(this);
	    }

	    public Unit CreateNpc(ClassType classType, uint level)
	    {
		    var charUid = GameDataManager.GetUidByString("character.npc." + (int)classType);
		    var sheet = GameData.GetData<CharacterInfoSpec>(charUid);
		    if (null != sheet)
		    {
			    var instance = new Npc(this);
			    instance.SetLevel(level);

			    if (instance.Load(charUid))
			    {
				    return instance;
			    }
		    }

		    return null;
	    }

	    public void UpdateNpc(Npc npc, uint level)
	    {
		    var charUid = GameDataManager.GetUidByString("character.npc." + (int)npc.ClassType);
		    var sheet = GameData.GetData<CharacterInfoSpec>(charUid);
		    if (null != sheet)
		    {
			    var initPos = npc.InitialPosition;
			    var pos = npc.Position;
			    var arrivePos = npc.ArrivalPosition;
			    
			    npc.SetLevel(level);
			    npc.Load(charUid);
			    
			    npc.ResetPosition(initPos, pos, arrivePos);
		    }
	    }
	    
	    public bool IsStageWin()
	    {
		    bool isOver = true;
	
		    var monsterList = MonsterList;

		    if (monsterList != null)
		    {
				foreach(var unit in monsterList)    
				{
					if (unit != null)
					{
						 if (unit.IsLive())
						 {
							  isOver = false;
							  break;
						 }
					}
				}
		    }

		    return isOver;
	    }
	    
	    public bool IsStageLose()
	    {
		    bool isOver = true;
			foreach(var unit in CharList)    
		    {
			    if (unit != null)
			    {
				    if (unit.IsLive())
				    {
					    isOver = false;
					    break;
				    }
			    }
		    }
	  
		    return isOver;
	    }

	    public virtual List<RequestParam> GetRequireParams()
	    {
		    return null;
	    }
	    
	    public virtual void SetCharPositions(List<Unit> charList)
	    {
		    if (charList == null)
		    {
			    return;
		    }
		    
		    var count = charList.Count;
		    string uidStr = $"art_formation.base.{count}";

		    SetCharPositions(charList, uidStr);
	    }

	    protected void SetCharPositions(List<Unit> charList, string uidStr)
	    {
		    var count = charList.Count;
		    
		    var formationSheet = GameData.GetData<ArtFormationInfoSpec>(uidStr);
		    if (formationSheet == null)
		    {
			    throw new CorgiException("invalid formation info {0}", uidStr);
		    }

		    var jsonArray = JArray.Parse(formationSheet.CharPositions);
		    if (jsonArray.Count < count)
		    {
			    throw new CorgiException("invalid formation info count {0}", uidStr);
		    }

		    for (var i = 0; i < count; i++)
		    {
			    var posArray = jsonArray[i] as JArray;
			    if (posArray == null || posArray.Count != 2)
			    {
				    throw new CorgiException("invalid formation info position info{0}", uidStr);
			    }

			    var positions = CorgiJson.ParseArrayFloat(posArray);
			    if (positions.Count < 2)
			    {
				    throw new CorgiException("invalid formation info position info{0}", uidStr);
			    }

			    var charInst = charList[i];
			    var charPos = new CorgiPosition(positions[0], positions[1], 0, 1);
			    charInst.InitStartPosition(charPos);
		    }
	    }

	    /// <summary>
	    /// Aura 적용
	    /// </summary>
	    /// <param name="action"></param>
	    public virtual void DoSkillEffectAuraAction(Unit target, Action<SkillEffectInst> action)
	    {
		    foreach (var unit in CharList)
		    {
			    if (unit.ObjectId == target.ObjectId)
			    {
				    continue;
			    }
			    
			    unit.DoSkillEffectAuraAction(target, action);
		    }

		    var monsterList = MonsterList;

		    if (monsterList != null)
		    {
				foreach (var unit in monsterList)
				{
					if (unit.ObjectId == target.ObjectId)
					{
						 continue;
					}
					
					unit.DoSkillEffectAuraAction(target, action);
				}
			    
		    }
		    
		    foreach (var unit in _structureList)
		    {
			    if (unit.ObjectId == target.ObjectId)
			    {
				    continue;
			    }
			    
			    unit.DoSkillEffectAuraAction(target, action);
		    }

	    }

	    protected virtual ulong GetExploreTime()
	    {
		    return CorgiLogicConst.ExploreDelay;
	    }

	    public virtual void OnPauseEvent(Unit unit, PauseCommandType command, DungeonLogNode logNode, params int[] args)
	    {
	    }

	    public bool SummonMonster(Unit owner, List<ulong> monsterUids, CombatLogNode logNode)
	    {
		    if (_curStage == null)
		    {
			    return false;
		    }

		    return _curStage.SummonMonster(owner, monsterUids, logNode);
	    }
	    
        public void UpdateAutoHuntingBuff(ulong buffEndTimestamp)
        {
            var uidStr = "skill.common.haste.1";

            var skillUid = GameDataManager.GetUidByString(uidStr);

            RemoveExternalSkill(skillUid);

            AddExternalSkill(skillUid, buffEndTimestamp);
        }

	    protected bool _Test_OutcomeControl()
	    {
		    return _bridge.Test_BoolValue();
	    }

	    public virtual void ApplyDungeonFeature(DamageSkillCompLogNode dmgNode)
	    {
		    return;
	    }
    }

}
