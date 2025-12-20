using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class DungeonArena: Dungeon, ICorgiInterface<InstanceDungeonInfoSpec>
    {
	    private InstanceDungeonInfoSpec _spec;

	    private string _characterId;
	    private ulong _exploreTime = CorgiLogicConst.ExploreDelayArena;
	    
	    private readonly List<Unit> _enemyList = new List<Unit>();
	    public List<Unit> EnemyList => _enemyList;

	    public Dictionary<string, SharedMemberInfo> SharedEnemyList = new Dictionary<string, SharedMemberInfo>();
	    
	    public string CharacterId
	    {
		    get { return _characterId; }
		    set { _characterId = value; }
	    }
	    
	    public string DungeonKey { get; set; }
	    
	    public string TargetId { get; set; }
	    
	    public uint SuddenDeathCount { get; set; }

	    public InstanceDungeonInfoSpec GetSpec()
	    {
		    return _spec;
	    }

	    public DungeonArena(ICombatBridge bridge)
			: base(bridge, DungeonType.Arena)
	    {
		    AreaType = AreaType.AreaArena;
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
		    // var sheetData = GameData.GetData<InstanceDungeonInfoSpec>(uid);
		    // if (sheetData == null)
		    // {
			   //  CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid sub stage uid : {0}", uid);
			   //  return false;
			   //  
		    // }
		    //
		    // _spec = sheetData;

		    return true;
	    }

	    public DungeonLogNode EnterArena(List<Unit> charList, List<Unit> enemyList)
	    {
		    if (charList == null || charList.Count == 0)
		    {
			    return null;
		    }
		    
		    if (enemyList == null || enemyList.Count == 0)
		    {
			    return null;
		    }
		    
	        var retLogNode = new DungeonLogNode(_state);
            
		    //_gameData = GameDataManager.Instance; // set current gamedata
		    
		    UpdateCharacters(charList);
		    UpdateEnemies(enemyList);

	        OnEnterArena();
	        
			EnterStage(retLogNode);
	        
            var dungeonInfo = new SharedArena();
            dungeonInfo.Init(this);
            retLogNode.SharedInstance= dungeonInfo;
	        
            _state.Set(DungeonState.Exploration);
            
            return retLogNode;

	    }
	    
	    protected void UpdateEnemies(List<Unit> unitList)
	    {
            _enemyList.Clear();
			var newCharList = FillNpc(unitList);
			 
	        _enemyList.AddRange(newCharList);

	        foreach (var enemy in _enemyList)
	        {
		        var npc = enemy as Npc;
		        if (npc != null)
		        {
					npc.SetCombatSide(CombatSideType.Enemy);
		        }
	        }
	        
	        UpdateEnemies(false);
	    }
	    
        public void UpdateEnemies(bool isDataUpdated = false)
        {
	        uint minLevel = 0;
	        
            foreach (var unit in _enemyList)
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
	        foreach (var unit in _enemyList)
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
		        
                // var deck = GetCurrentDeck(unit);
                // if (character.LoadCombatSetting(deck) == false)
                // {
	               //  //CorgiLog.LogError("invalid character deck info");
	               //  CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "LoadCombatSetting error by deckinfo from UpdateCharacters. characteruid[{0}]", character.Uid);
                //     continue;
                // }
                //
                if (skillUids.Count == 0 && unit.ActiveSkills.Count == 4)
                {
                    skillUids.Add(unit.ActiveSkills[0].Uid);
                    skillUids.Add(unit.ActiveSkills[1].Uid);
                    skillUids.Add(unit.ActiveSkills[2].Uid);
                    skillUids.Add(unit.ActiveSkills[3].Uid);
                }
	        }
	        
            foreach (var unit in _enemyList)
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
	        
	        _enemyList.Sort(delegate(Unit x, Unit y)
	        {
		        var a = (int)x.ClassType;
		        var b = (int)y.ClassType;
		        if (a == 0 && b == 0) return 0;
		        else if (a < b) return -1;
		        else if (a > b) return 1;

		        return 0;
	        });
	        
	        // // set character positions
	        SetCharPositions(_enemyList);
        }
	    
        void OnEnterArena()
        {
	        OnEnterDungeon();
	        
	        // sorting
	        _enemyList.Sort(delegate(Unit x, Unit y)
	        {
		        var a = (int)x.ClassType;
		        var b = (int)y.ClassType;
		        if (a == 0 && b == 0) return 0;
		        else if (a < b) return -1;
		        else if (a > b) return 1;

		        return 0;
	        });
	        
	        // // set enemies positions
	        SetCharPositions(_enemyList);

			CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Enter Dungeon : {0}", ObjectId);
        }

        protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
	        switch (_state.Value)
	        {
		        case DungeonState.Exploration:
			        Exploration(logNode);
			        _state.Set(DungeonState.Exploring);
			        // DoWaiting(GetExploreTime(), DungeonState.Exploring, GetRequireParams());
			        break;
				case DungeonState.Exploring:
					TickInExplore(deltaTick, logNode);
					break;
				case DungeonState.EnterCombat:
					OnEnterCombat(logNode);
					_state.Set(DungeonState.InCombat);
					break;
				case DungeonState.InCombat:
					TickInCombat(deltaTick, logNode);
					break;
				case DungeonState.FinishStage:
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
						StopTimer.Tick();
					}
					
					break;
				case DungeonState.RewardCompleted:
					_state.Set(DungeonState.Destroy);
					break;
		        case DungeonState.None:
				default:
					CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"An Unsupported StageState : {0} -> {1}\n",_state.Prev, _state.Value);
					break;
			}
        }

        protected override void EnterStage(DungeonLogNode logNode)
        {
         //    var uidStr = "instance_worldboss.stage.1";
	        // var stageUid = GameDataManager.GetUidByString(uidStr);
	        //
	        // _nextStageUid = stageUid;
	        _curStage = CreateNewStage(0);
	        
	        if (_curStage == null)
	        {
		        throw new CorgiException("Critical!, Invalid stage uid[{0}] when Enter Stage.", GameData.GetStrByUid(_nextStageUid));
	        }
	        
            OnEnterStage(logNode);
        }
        
        protected override void Exploration(DungeonLogNode logNode)
        {
			foreach (var curUnit in EnemyList)
			{
				curUnit.OnExploration(logNode);
			}
			
			base.Exploration(logNode);
        }
        
	    public void TickInExplore(ulong deltaTime, LogNode logNode)
	    {
		    var dungeonNode = (DungeonLogNode) logNode;

		    if (dungeonNode == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid dungeon log node");
			    return;
		    }
		    
		    var childNode = new TickLogNode(deltaTime);
		    childNode.DungeonLogNode = dungeonNode;
		    
		    DoUnitAction((unit) =>
		    {
				 unit.TickInCombat(deltaTime, childNode);
		    });

		    dungeonNode.SetCombatLog(childNode);
		    
		    if (_exploreTime > deltaTime)
		    {
			    _exploreTime -= deltaTime;
		    }
		    else
		    {
			    _exploreTime = 0;
		    }
		    
		    if (_exploreTime == 0)
		    {// end Waiting
			    _exploreTime = 0;
			    _state.Set(DungeonState.EnterCombat);

			    CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Waiting End");
		    }
	    }
        
		protected override SharedInstance.SharedInstance GetSharedInstance()
		{
			var sharedDungeon = new SharedArena();
			sharedDungeon.Init(this);

			return sharedDungeon;
		}
	    
        public override Stage CreateNewStage(ulong nextStageUid)
        {
	        var newStage = new StageInstanceArena(this); 
	        
	        if (newStage.Load(nextStageUid) == false)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance.");
		        return null;
	        }

	        foreach (var enemy in EnemyList)
	        {
		        if (enemy == null)
		        {
			        continue;
		        }

		        newStage.AddEnemy(enemy);
	        }

	        return newStage;
        }

        protected override void OnFinishStage(CombatLogNode logNode)
        {
            base.OnFinishStage(logNode);

            //var isWin = IsStageWin();
            
            if (IsWin)
	        {
				_state.Set(DungeonState.Win);
	        }
	        else
	        {
				_state.Set(DungeonState.Lose);
	        }

            var winnerId = IsWin ? _characterId : TargetId;
	        Bridge.ArenaFinish(DungeonKey, _characterId, TargetId, winnerId);
	        
	        StopTimer.StartTimer(CorgiLogicConst.RewardDelay);
        }

        public override List<RequestParam> GetRequireParams()
        {
            var retList = new List<RequestParam>();
	        
			retList.Add(new RequestParam(RedisRequestType.CharaterInfo, _characterId));

			return retList;
        }
        
        public override void SetCharPositions(List<Unit> charList)
        {
	        if (charList == null || charList.Count == 0)
	        {
		        return;
	        }

	        var firstChar = charList[0];

	        string uidStr;
	        if (firstChar.CombatSideType == CombatSideType.Player)
	        {
				uidStr = $"art_formation.pvp.1";
	        }
	        else
	        {
				uidStr = $"art_formation.pvp.2";
		        
	        }
			SetCharPositions(charList, uidStr);
        }
        
        protected override Deck GetCurrentDeck(Unit unit)
        {
			return Bridge.GetSoloOffenseDeck(_characterId, unit);
        }
        
	    protected override ulong GetExploreTime()
	    {
		    return CorgiLogicConst.ExploreDelayArena;
	    }

	    public override void ApplyDungeonFeature(DamageSkillCompLogNode dmgNode)
	    {
		    if (CurCombatTick < 30000)
		    {
			    return;
		    }
		    
		    var exponent = (CurCombatTick - 30000) / 10000 + 1;
		    SuddenDeathCount = (uint)exponent;
		    
		    var preDamage = dmgNode.Damage;
		    var factor = (int)Math.Pow(2, exponent);
		    dmgNode.Damage *= factor;
		    dmgNode.AddDetailLog($"Sudden Death: PreDamage[{preDamage}] / Factor[{factor}] / Damage[{dmgNode.Damage}]");
	    }
	    
	    public void OnArenaDungeonCompleted(ulong stageUid)
	    {
		    if (State != DungeonState.Win && State != DungeonState.Lose)
		    {
			    throw new CorgiException("invalid dungeon state({0}) in OnStageCompleted", State);
		    }
            
		    _nextStageUid = stageUid;

		    _state.Set(DungeonState.RewardCompleted);
	    }

	    public void OnArenaDungeonStop()
	    {
		    Bridge.ArenaFinish(DungeonKey, _characterId, TargetId, TargetId);
		    _state.Set(DungeonState.Destroy);
	    }

	    public override Npc CreateNpcInstance()
	    {
		    return new NpcArena(this);
	    }
    }
}