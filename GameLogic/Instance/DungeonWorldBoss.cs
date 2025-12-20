using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class DungeonWorldBoss: Dungeon, ICorgiInterface<InstanceDungeonInfoSpec>
    {
	    private InstanceDungeonInfoSpec _spec;

	    private string _characterId;

	    private DungeonCriteriaType _dungeonType;
	    private uint _grade;
	    private uint _level;
	    private float _monsterMod;

	    private long _maxHP;
	    private long _curHP;	// redis랑 동기화되는 체력, redisTask 도착했을 때만 update

	    private long _totalDamage = 0L;
	    private long _currentDamage = 0L;
	    private bool _isRequestingDamage = false;
	    

	    public InstanceDungeonInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public string DungeonKey
	    {
		    get; set;
	    }

	    public string CharacterId
	    {
		    get { return _characterId; }
		    set { _characterId = value; }
	    }

	    public DungeonCriteriaType DungeonCriteriaType
	    {
		    get { return DungeonCriteriaType.DctWorldBoss; }
	    }

	    public DungeonWorldBoss(ICombatBridge bridge)
			: base(bridge, DungeonType.WorldBoss)
	    {
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = GameData.GetData<InstanceDungeonInfoSpec>(uid);
		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid sub stage uid : {0}", uid);
			    return false;
			    
		    }

		    _spec = sheetData;

		    AreaType = AreaType.AreaWorldBossCave;

		    return true;
	    }
	    
        protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
            switch (_state.Value)
            {
	            case DungeonState.InCombat:
		            TickInCombat(deltaTick, logNode);
		            // check dmaage log node
		            long curDamage = 0L;
		            AccumulateDamage(ref curDamage, logNode.CombatLogNode);
		            _currentDamage += curDamage;
		            _totalDamage += curDamage;
		            if (_isRequestingDamage == false && _currentDamage > 0L)
		            {
			            // should adjust world boss hp
			            Bridge.WorldBossDamage(DungeonKey, _characterId, _currentDamage);
			            _isRequestingDamage = true;
			            _currentDamage = 0L;
		            }
		            break;
				case DungeonState.Win:
				case DungeonState.Lose:
					// 시간이 지나면 강제로 RewardCompleted로 보내버린다.
					if (StopTimer.IsOver())
					{
						//Bridge.DoDestroy(6);
						Bridge.DungeonStateWaitLog(_state.Value.ToString());
					}
					else
					{
						//-애초에 이벤트 방식이 아닌 폴링 방식 구조를 잡았기 때문에. 여기로 계속 호출될 수가 있음.........
						Bridge.DungeonStateWaitLog(_state.Value.ToString());
						StopTimer.Tick();
					}
					break;
	            case DungeonState.FinishStage:
	            case DungeonState.FinishStageByOther:
		            FinishStage(logNode);
		            break;
	            case DungeonState.RewardCompleted:
                    _state.Set(DungeonState.Destroy);
                    break;
                default:
                    base.Tick(deltaTick, logNode);
                    break;
            }
        }

        void AccumulateDamage(ref long totalDamage, CombatLogNode logNode)
        {
	        if (logNode == null)
	        {
		        return;
	        }
	        
			var damageChild = logNode as DamageSkillCompLogNode;
			if (damageChild != null )
			{
				Unit target = GetUnit(damageChild.TargetId);
				if (target?.CombatSideType == CombatSideType.Enemy)
				{
					totalDamage += (long)damageChild.FinalDamage;
				}
			}

	        foreach (var child in logNode.Childs)
	        {
				AccumulateDamage(ref totalDamage, child);
	        }
        }

        public void OnWorldBossDamage(long newHP, long myDamage)
        {
	        if (_state == DungeonState.InCombat && MonsterList != null)
	        {
		        if (MonsterList.Count != 1)
		        {
			        CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "{0} monsters in WorldBoss", MonsterList.Count);
			        return;
		        }

		        bool isUpdated = false;
		        
		        Unit monster = MonsterList[0];
		        
		        var damage = _curHP - newHP;
		        var otherDamage = damage - myDamage;

		        if (otherDamage > 0 && monster.CurHP > 0)
		        {
			        monster.ApplyDamage(otherDamage);
			        if (monster.CurHP == 0)
			        {
				        IsWin = true;
				        _state.Set(DungeonState.FinishStageByOther);
				        isUpdated = true;
			        }
		        }
		        else if (otherDamage < 0)
		        {
			        CorgiCombatLog.Log(CombatLogCategory.System, "invalid damage[{0}] OnWorldBossDamage", otherDamage);
		        }
		        
		        _curHP = newHP;
		        _isRequestingDamage = false;
		        
		        if (isUpdated)
		        {
			        CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Change DungeonState [{0}] to [{1}] in Dungeon[{2}][{3}]", _state.Prev, _state.Value, DungeonType, DungeonKey);
		        }
	        }
        }

        public void InitBossHP(long maxHP, long curHP)
        {
	        _maxHP = maxHP;
	        _curHP = curHP;
        }

        protected override void EnterStage(DungeonLogNode logNode)
        {
            var uidStr = "instance_worldboss.stage.1";
	        var stageUid = GameDataManager.GetUidByString(uidStr);
	        
	        _nextStageUid = stageUid;
	        
	        base.EnterStage(logNode);
        }
	    
        public override Stage CreateNewStage(ulong nextStageUid)
        {
	        var newStage = new StageInstanceWorldBoss(this); 
	        
	        newStage.InitBossHP(_maxHP, _curHP);
		        
	        if (newStage.Load(nextStageUid) == false)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance.");
		        return null;
	        }

	        return newStage;
        }

        protected override void DoUnitAction(Action<Unit> action)
        {
	        base.DoUnitAction(action);
	        
			//foreach (var curUnit in StructureList)
			//{
				//action(curUnit);
			//}
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

	        Bridge.WorldBossFinish(DungeonKey, _characterId, _totalDamage, IsWin);
	        
	        if (IsWin)
	        {
		        Bridge.WorldBossDead(DungeonKey);
	        }
	        
	        StopTimer.StartTimer(CorgiLogicConst.RewardDelay);
        }

        public void OnWorldBossDungeonCompleted(ulong stageUid)
        {
            if (State != DungeonState.Win && State != DungeonState.Lose)
            {
                throw new CorgiException("invalid dungeon state({0}) in OnStageCompleted", State);
            }
            
            _nextStageUid = stageUid;

            _state.Set(DungeonState.RewardCompleted);
        }
        
        public void OnWorldBossDungeonStop()
        {
	        Bridge.WorldBossFinish(DungeonKey, _characterId, _totalDamage, IsWin);
	        _state.Set(DungeonState.Destroy);
        }

        public override List<RequestParam> GetRequireParams()
        {
            var retList = new List<RequestParam>();
	        
			retList.Add(new RequestParam(RedisRequestType.CharaterInfo, _characterId));

			return retList;
        }
        
        public override void SetCharPositions(List<Unit> charList)
        {
	        if (charList == null)
	        {
		        return;
	        }
		    
	        int mageNum = 0;

	        foreach (var unit in charList)
	        {
		        if (unit == null)
		        {
			        continue;
		        }
		        
		        if (unit.ClassType == ClassType.CtMage)
		        {
			        mageNum++;
		        }
	        }

	        int postfix = 1;

	        if (mageNum == 1)
	        {
		        postfix = 1;
	        }
			else if (mageNum == 0)
	        {
		        postfix = 2;
	        }
	        else if (mageNum == 2)
	        {
		        postfix = 3;
	        }
	        else
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid mageNum[{0}]", mageNum);
	        }

	        string uidStr = $"art_formation.worldboss.{postfix}";

	        SetCharPositions(charList, uidStr);
        }
        
	    public override void DoSkillEffectAuraAction(Unit target, Action<SkillEffectInst> action)
	    {
		    base.DoSkillEffectAuraAction(target, action);
		    
		    // do action int only instance dungeon
		    //foreach (var unit in StructureList)
		    //{
			    //if (unit.ObjectId == target.ObjectId)
			    //{
				    //continue;
			    //}
			    //
			    //unit.DoSkillEffectAuraAction(target, action);
		    //}
	    }
        
        protected override Deck GetCurrentDeck(Unit unit)
        {
			return Bridge.GetSoloPartyDeck(_characterId, unit);
        }
    }
}
