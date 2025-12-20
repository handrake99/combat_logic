using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Network;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class DungeonTutorial : Dungeon, ICorgiInterface<TutorialDungeonInfoSpec>
    {
	    private TutorialDungeonInfoSpec _spec;
	    private Unit _owner;					// user
        private List<TutorialStageInfoSpec> _stageList = new List<TutorialStageInfoSpec>();

        private List<Unit> _unitList = new List<Unit>();

        private PauseManager _pauseManager;
        
	    public TutorialDungeonInfoSpec GetSpec()
	    {
		    return _spec;
	    }

	    public DungeonTutorial(ICombatBridge bridge)
			: base(bridge, DungeonType.Instance)
	    {
	    }

	    // called after new constructor
	    public void SetOwner(Unit owner)
	    {
		    _owner = owner;
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = GameData.GetData<TutorialDungeonInfoSpec>(uid);
		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid sub stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;
		    
		    // setting stage
            string stagePrefix = sheetData.StagePrefix;

            if (stagePrefix == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid stage prefix : {0}\n", uid);
                return false;
            }

            var stageCount = sheetData.StageCount;

            _stageList.Clear();

            for (int i = 1; i <= stageCount; i++)
            {
                string curStageUid = stagePrefix + "." + i;

                var stageSpec = GameData.GetData<TutorialStageInfoSpec>(curStageUid);

                if (stageSpec == null)
                {
                    CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid stage uid : {0}\n", curStageUid);
                    return false;
                }

                _stageList.Add(stageSpec);
            }

            if (_stageList.Count == 0)
            {
	            return false;
            }
            
            // setting pause
            _pauseManager = new PauseManager(this, _owner);
            var pausePrefix = sheetData.PausePrefix;
            var pauseCount = sheetData.PauseCount;
            
            for (int i = 1; i <= pauseCount; i++)
            {
                string curPauseUid = pausePrefix + "." + i;

                var thisSpec = GameData.GetData<GuidePauseInfoSpec>(curPauseUid);

                if (thisSpec == null)
                {
                    CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid pause uid : {0}\n", curPauseUid);
                    return false;
                }

                var pauseCommand = CorgiEnum.ParseEnum<PauseCommandType>(thisSpec.PauseCommand);
                
				var pauseComp = PauseFactory.CreateInstance(pauseCommand, _owner);
				if (pauseComp.Load(thisSpec.Uid) == false)
				{
	                CorgiCombatLog.LogError(CombatLogCategory.Tutorial, "Invalid pause command type ({0})",
		                thisSpec.PauseCommand);
					 continue;
				}

				_pauseManager.AddPause(pauseComp);
            }
            
		    return true;
	    }
        
        public override Stage CreateNewStage(ulong nextStageUid)
        {
            if (_curStageIndex < 0 || _curStageIndex >= _stageList.Count)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"invalid stage index.{0} ", _curStageIndex);
                return null;
            }


            var curSpec = _stageList[_curStageIndex];

            if (curSpec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"invalid stage spec. null");
                return null;
            }

            Stage retStage = new StageTutorial(this);

            if (retStage.Load(curSpec.Uid) == false)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"failed Initializing stage instance. {0}", GameData.GetStrByUid(Uid));
                return null;
            }

            CorgiCombatLog.Log(CombatLogCategory.Tutorial,"Enter New Stage {0}, {1}", (_curStageIndex+1), curSpec.Uid);

            return retStage;
        }

        public override List<Unit> FillNpc(List<Unit> unitList)
        {
	        var thisSpec = GetSpec();
	        
	        // 첫번재의 경우 DungeonInfo의 IsSolo 확인
	        if (_curStage == null && thisSpec != null && thisSpec.IsSolo)
	        {
		        return unitList;
	        }
	        
	        var tutorialStage = _curStage as StageTutorial;
	        
	        // 
	        if (tutorialStage != null)
	        {
		        var spec = tutorialStage.GetSpec();
		        if (spec != null && spec.IsSolo)
		        {
			        // no npc
			        return unitList;
		        }
	        }
	        
	        return base.FillNpc(unitList);
        }
        
	    public override Npc CreateNpcInstance()
	    {
		    return new NpcTutorial(this);
	    }
	    
        public override DungeonLogNode EnterDungeon(List<Unit> unitList, ulong curStageUid)
        {
	        _unitList.Clear();
	        _unitList.AddRange(unitList);
	        var stageUid = _stageList[0].Uid;
	        
	        _pauseManager.OnEnterDungeon();
            
            return base.EnterDungeon(unitList, stageUid);
        }
        
        // tutorial 던전의 Stage초기화는 Exploration 때 한다.
        protected override void Exploration(DungeonLogNode logNode)
        {
	        if (_curStage != null )
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"invalid state. prev stage was not over yet.");
		        return ;
	        }

	        _curStage = CreateNewStage(_nextStageUid);
	        if (_curStage == null)
	        {
		        throw new CorgiException("Critical!, Invalid stage uid[{0}] when Enter Stage.", GameData.GetStrByUid(_nextStageUid));
	        }
	        
	        UpdateCharacters(_unitList);
	        
			foreach (var curUnit in CharList)
			{
				curUnit.OnExploration(logNode);
			}
        }
        
        protected override void EnterStage(DungeonLogNode logNode)
        {
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
        
        protected override void OnFinishStage(CombatLogNode logNode)
        {
            base.OnFinishStage(logNode);
            
			_state.Set(DungeonState.Win);
        }
	    public override void OnFinishCombat(CombatLogNode logNode)
	    {
			_state.Set(DungeonState.FinishStage);
	    }
        
        protected override Deck GetCurrentDeck(Unit unit)
        {
			return Bridge.GetPersonalDeck(unit);
        }
        
	    protected override ulong GetExploreTime()
	    {
		    return CorgiLogicConst.TutorialStageTime;
	    }

	    protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
	    {
		    switch (_state.Value)
		    {
			    case DungeonState.Win:
			    case DungeonState.Lose:
				    _state.Set(DungeonState.RewardCompleted);
				    break;
				case DungeonState.StageCompleted:
					ResetDungeonState();
					_curStage = null;
					_curStageIndex++;
					
				    if (_curStageIndex >= _stageList.Count)
				    {
						 _state.Set(DungeonState.FinishDungeon);
				    }
				    else
				    {
					    // 남았을때
						_nextStageUid = _stageList[_curStageIndex].Uid;
						DoWaiting(CorgiLogicConst.StageDelay, DungeonState.Exploration, null);
				    }
                    // for stage complete 
					break;
			    default:
				    base.Tick(deltaTick, logNode);
				    break;
		    }
	    }

	    public PauseTargetType GetPauseTargetType()
	    {
		    return _pauseManager.GetCurPauseTargetType();
	    }

	    public override void OnPauseEvent(Unit unit, PauseCommandType command, DungeonLogNode logNode, params int[] args)
	    {
		    _pauseManager.OnPauseEvent(unit, command, logNode, args);
	    }
    }
}
