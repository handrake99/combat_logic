using System.Collections.Generic;
using Corgi.GameData;
using Corgi.Protocol;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class DungeonAdventure : Dungeon, ICorgiInterface<AdventureDungeonInfoSpec>
    {
        /// <summary>
        /// static data
        /// </summary>
        private AdventureDungeonInfoSpec _spec;

        private List<AdventureStageInfoSpec> _stageList = new List<AdventureStageInfoSpec>();


        /// <summary>
        /// dynamic data
        /// </summary>
        public bool IsChallenging { get; set; }
        private string _challengeCharacterId = null;
        private ulong _challengeStageUid = 0;
        private uint _challengeCount = 0;

        public bool IsFailed;
        private int _failedCount = 0;
        
        protected override int GetStageCount()
        {
            return _stageList.Count;
        }
        
	    public override bool GetIsChallenging()
	    {
		    return IsChallenging;
	    }

	    public override uint GetChallengeCount()
	    {
		    return _challengeCount;
	    }

        public int IsStageChanged(ulong newStageUid)
        {
            if (_curStage == null)
            {
                return 0;
            }

            var curStageUid = _curStage.Uid;

            var curStageSpec = GameData.GetData<AdventureStageInfoSpec>(curStageUid);
            var newStageSpec = GameData.GetData<AdventureStageInfoSpec>(newStageUid);

            if (curStageSpec == null || newStageSpec == null)
            {
                return 0;
            }
            
            if (curStageSpec.StageIndex == newStageSpec.StageIndex)
            {
                return 0;
            }

            if (curStageSpec.StageIndex > newStageSpec.StageIndex)
            {
                return -1;
            }

            return 1;
            
        }

        public int IsChapterChanged(ulong newStageUid)
        {
            if (_curStage == null)
            {
                return 0;
            }

            var curStageUid = _curStage.Uid;

            var curStageSpec = GameData.GetData<AdventureStageInfoSpec>(curStageUid);
            var newStageSpec = GameData.GetData<AdventureStageInfoSpec>(newStageUid);

            if (curStageSpec == null || newStageSpec == null)
            {
                return 0;
            }

            if (curStageSpec.ChapterIndex == newStageSpec.ChapterIndex)
            {
                return 0;
            }

            if (curStageSpec.ChapterIndex > newStageSpec.ChapterIndex)
            {
                return -1;
            }

            return 1;
        }

        public AdventureDungeonInfoSpec GetSpec()
        {
            return _spec;
        }

        public DungeonAdventure(ICombatBridge bridge)
            : base(bridge, DungeonType.Adventure)
        {
            IsChallenging = false;
        }

        protected override bool LoadInternal(ulong uid)
        {
            var sheetData = GameData.GetData<AdventureDungeonInfoSpec>(uid);
            if (sheetData == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid adventure dungeon uid : {0}\n", GameDataManager.GetStringByUid(uid));
                return false;
            }

            _spec = sheetData;
            
            // load area type
            var areaSheet = GameData.GetData<ArtAreaInfoSpec>(_spec.AreaUid);
            if (areaSheet == null)
            {
                AreaType = AreaType.AreaNone;
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid Area Type : {0}\n", GameDataManager.GetStringByUid(_spec.AreaUid));
            }
            else
            {
                AreaType = areaSheet.AreaType;
            }

            // load stages
            string stagePrefix = sheetData.StagePrefix;

            if (stagePrefix == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage prefix : {0}\n", uid);
                return false;
            }

            var stageCount = sheetData.StageCount;

            _stageList.Clear();

            for (int i = 1; i <= stageCount; i++)
            {
                string curStageUid = stagePrefix + "." + i;

                var stageSpec = GameData.GetData<AdventureStageInfoSpec>(curStageUid);

                if (stageSpec == null)
                {
                    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}\n", curStageUid);
                    return false;
                }

                _stageList.Add(stageSpec);
            }

            return true;
        }
        
        protected override Deck GetCurrentDeck(Unit unit)
        {
            if (IsChallenging)
            {
                return Bridge.GetCoPartyDeck(unit);
            }
            else
            {
                return Bridge.GetPersonalDeck(unit);
            }
        }

        public override void UpdateStage(ulong curStageUid)
        {
	        _nextStageUid = curStageUid;

            var curStageIndex = 0;
            foreach (var stage in _stageList)
            {
                if (stage == null)
                {
                    continue;
                }

                if (stage.Uid == curStageUid)
                {
                    _curStageIndex = curStageIndex;
                    return;
                }

                curStageIndex++;
            }
        }

        public override DungeonLogNode EnterDungeon(List<Unit> unitList, ulong curStageUid)
        {
            int index = 0;
            foreach (var curSpec in _stageList)
            {
                if (curSpec?.Uid == curStageUid)
                {
                    _curStageIndex = index;
                }

                index++;
            }
            
            return base.EnterDungeon(unitList, curStageUid);
        }

        protected override void OnFinishStage(CombatLogNode logNode)
        {
            base.OnFinishStage(logNode);

            var isWin = IsStageWin();
            
            if (_state.Value == DungeonState.FinishStage || _state.Value == DungeonState.OnChallengeFinish)
            {
                if (IsChallenging)
                {
                     Bridge.ChallengeFinish(_challengeCharacterId, _curStage.Uid, isWin);
                }
                else
                {
                     Bridge.StageFinish(_challengeCharacterId, _curStage.Uid, isWin);
                }
            }

            //isWin = Test_OutcomeControl();//-use for test when no-connection hunting mode
            
	        if (isWin)
	        {
				_state.Set(DungeonState.Win);
                IsFailed = false;
            }else
            {
                IsFailed = true;
                _failedCount++;
                // if (_failedCount >= 10)
                // {
                //     IsFailed = false;
                // }
				_state.Set(DungeonState.Lose);
	        }


            //DoTimer(CorgiLogicConst.RewardDelay);
            StopTimer.StartTimer(CorgiLogicConst.RewardDelay);
        }
        

        public void OnChallengeStarted(string characterId, ulong stageUid)
        {
            if (IsChallenging)
            {
                // already challenge
                return;
            }
            
            var configSheet = GameData.GetConfig("config.adventure.stage.final");

            if (configSheet != null && configSheet.Value.UidValue == stageUid)
            {
                // last stage uid cant challenge
                _challengeCount = 0;
                return;
            }
            _challengeCharacterId = characterId;
            if (stageUid == 1UL) // for stress test mode
            {
                _challengeStageUid = _stageList[_curStageIndex].Uid;
            }
            else
            {
                _challengeStageUid = stageUid;
            }
            IsChallenging = true;
            
            ResetDungeonState();
            _state.Set(DungeonState.OnChallengeStart);
            
            CorgiCombatLog.Log(CombatLogCategory.Dungeon,"OnChallengeStarted. CharId[{0}], stageuid[{1}]", characterId, stageUid);
        }
        
        public void OnChallengeCompleted(ulong stageUid)
        {
            if (State != DungeonState.Win && State != DungeonState.Lose)
            {
                throw new CorgiException("Invalid dungeon state[{0}]. Can't OnStageCompleted", State.ToString());
            }

            _nextStageUid = stageUid;
            
            if (_challengeCount > 0)
            {
                if (State == DungeonState.Win)
                {
                    _challengeCount--;
                }else if (State == DungeonState.Lose)
                {
                    _challengeCount = 0;
                }
            }
            
            _state.Set(DungeonState.RewardCompleted);
            
            CorgiCombatLog.Log(CombatLogCategory.Dungeon, "OnChallengeCompleted. stageuid[{0}]", stageUid);
        }

        public void OnAutoHuntingStart(string characterId, ulong stageUid, bool serialBoss, ulong buffEndTimestamp)
        {
            if (serialBoss)
            {
                _challengeCount = 5;
            }

            if ((true == serialBoss) && (true == IsChallenging))
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "Request AutoHunting but IsChallenging and SerialBoss is true");
                return;
            }
            
            // 가속 버프 처리
            if (buffEndTimestamp != 0)
            {
                UpdateAutoHuntingBuff(buffEndTimestamp);
            }
            
            
            // challenge 진행 여부
            if (_challengeCount > 0)
            {
                OnChallengeStarted(characterId, stageUid);
            }
        }


	    public override void OnFinishCombat(CombatLogNode logNode)
	    {
            if (IsChallenging)
            {
                _state.Set(DungeonState.OnChallengeFinish);
            }
            else
            {
                _state.Set(DungeonState.FinishStage);
            }
	    }

        protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
            
            if (IsChallenging == false)
            {
                switch (_state.Value)
                {
                    case DungeonState.Exploration:
                        Bridge.UpdateUnit();//-파티에 가입하거나 떠나는 캐릭터의 정보를 클라이언트에게 알려주는 시점.
                        Exploration(logNode);
                        DoWaiting(GetExploreTime(), DungeonState.EnterStage, GetRequireParams());
                        OnPauseEvent(null, PauseCommandType.PauseCommand_Dungeon, logNode, (int)DungeonState.Exploration);
                        break;
                    default:
                        base.Tick(deltaTick, logNode);
                        logNode.IsChallenging = false;
                        break;
                }
                return;
            }

            switch (_state.Value)
            {
                case DungeonState.OnChallengeStart:
                    // finish combat
                    if (_curStage != null)
                    {
                        var childNode = new StageLogNode(_curStage);
                        childNode.DungeonLogNode = logNode;
                        OnFinishStage(childNode);
                        logNode.SetCombatLog(childNode);
                    }
                    IsFailed = false;
                    _state.Set(DungeonState.StageCompleted);
                    break;
                
				case DungeonState.OnChallengeFinish:
				    FinishStage(logNode);
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
						//TickTimer(deltaTick, logNode);
                        StopTimer.Tick();
					}
					break;
                case DungeonState.RewardCompleted:
                    var configSheet = GameData.GetConfig("config.adventure.stage.final");
                    
                    if (_challengeCount <= 0 || (configSheet != null && configSheet.Uid == _curStage.Uid))
                    {
                        _challengeCount = 0;
                        _challengeCharacterId = null;
                        _challengeStageUid = 0;
                        IsChallenging = false;
                    }

                    _state.Set(DungeonState.StageCompleted);
                    break;
                default:
                    //-.....????????
                    base.Tick(deltaTick, logNode);
                    break;
            }
            logNode.IsChallenging = true;
        }

        public override Stage CreateNewStage(ulong nextStageUid)
        {
            if (_curStageIndex < 0 || _curStageIndex >= _stageList.Count)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"invalid stage index.{0} ", _curStageIndex);
                return null;
            }


            if (nextStageUid != 0)
            {
                var prevIndex = _curStageIndex - 1;
                var nextIndex = _curStageIndex + 1;

                if (CheckStage(prevIndex, nextStageUid))
                {
                    _curStageIndex = prevIndex;
                }
                else if (CheckStage(nextIndex, nextStageUid))
                {
                    _curStageIndex = nextIndex;
                }else if (CheckStage(_curStageIndex, nextStageUid))
                {
                    _curStageIndex = _curStageIndex;
                }
                else
                {
                    throw new CorgiException("invalid stage index");
                }
            }

            var curSpec = _stageList[_curStageIndex];

            if (curSpec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"invalid stage spec. null");
                return null;
            }

            Stage retStage = null;
            if (IsChallenging)
            {
                retStage = new StageAdventureBoss(this);
            }
            else
            {
                retStage = new StageAdventureMinion(this);
            }

            if (retStage.Load(curSpec.Uid) == false)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance. {0}", GameData.GetStrByUid(Uid));
                return null;
            }

            CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Enter New Stage {0}, {1}", (_curStageIndex+1), curSpec.Uid);

            return retStage;
        }
        
	    public override List<RequestParam> GetRequireParams()
        {
            var retList = new List<RequestParam>();
            
            // check roomInfo 
            if (CheckStage(_curStageIndex, _nextStageUid) == false)
            {
                var prevIndex = _curStageIndex - 1;
                var nextIndex = _curStageIndex + 1;

                if (CheckStage(prevIndex, _nextStageUid) == false && CheckStage(nextIndex, _nextStageUid) == false)
                {
                    retList.Add(new RequestParam(RedisRequestType.RoomInfo, Bridge.Id()));
                }
            }
            
            // add character info
            foreach (var curChar in CharList)
            {
                retList.Add(new RequestParam(RedisRequestType.CharaterInfo, curChar.DBId));
            }
            
            retList.Add(new RequestParam(RedisRequestType.RoomDeckInfo, Bridge.Id()));
            
		    return retList;
	    }

        bool CheckStage(int nextIndex, ulong nextStageUid)
        {
            if (nextIndex < 0 || nextIndex >= _stageList.Count)
            {
                return false;
            }

            var spec = _stageList[nextIndex];
            if (spec.Uid == nextStageUid)
            {
                return true;
            }

            return false;
        }

        protected override ulong GetExploreTime()
        {
            if (IsChallenging)
            {
                var challengeExploreTime = base.GetExploreTime();
                if (_challengeCount == 5)
                {
                    challengeExploreTime += 2000;
                }

                return challengeExploreTime;
            }
            var exploreTime = CorgiLogicConst.MinimumStageTime - CurStageTick;
            
            if (CorgiLogicConst.MinimumStageTime < CurStageTick || exploreTime < CorgiLogicConst.ExploreDelay)
            {
                exploreTime = CorgiLogicConst.ExploreDelay;
            }

            if (exploreTime < CorgiLogicConst.ConversationStageTime)
            {
                exploreTime = CorgiLogicConst.ConversationStageTime;
            }

            return exploreTime;
        }

        private bool Test_OutcomeControl()
        {
            return _Test_OutcomeControl();
        }
    }
}