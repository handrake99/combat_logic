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
    public class DungeonInstance : Dungeon, ICorgiInterface<InstanceDungeonInfoSpec>
    {
	    private InstanceDungeonInfoSpec _spec;
	    private ulong _curStageUid;

	    private string _characterId;

	    private DungeonCriteriaType _dungeonType;
	    private uint _grade;
	    private uint _level;
	    private float _monsterMod;
	    

	    public InstanceDungeonInfoSpec GetSpec()
	    {
		    return _spec;
	    }

	    public string CharacterId
	    {
		    get { return _characterId; }
		    set { _characterId = value; }
	    }

	    public ulong CurStageUid => _curStageUid;

	    public uint Grade => _grade;
	    public uint Level => _level;
	    public float MonsterMod => _monsterMod;
	    
	    public DungeonCriteriaType DungeonCriteriaType
	    {
		    get { return _dungeonType; }
	    }

	    public DungeonInstance(ICombatBridge bridge)
			: base(bridge, DungeonType.Instance)
	    {
	    }

	    protected override bool LoadInternal(JObject json)
	    {
			if( CorgiJson.IsValidString(json, "characterId") == false
				|| CorgiJson.IsValidString(json, "dungeonId") == false
				|| CorgiJson.IsValidLong(json, "dungeonUid") == false
				|| CorgiJson.IsValidLong(json, "stageUid") == false
				|| CorgiJson.IsValidInt(json, "dungeonType") == false
				|| CorgiJson.IsValidInt(json, "grade") == false
				|| CorgiJson.IsValidInt(json, "level") == false
				|| CorgiJson.IsValid(json, "affix") == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"invalid commnad parameter for InstanceDungeon Load\n");
				return false;
			}
            
			var characterId = CorgiJson.ParseString(json, "characterId");
			var dungeonId = CorgiJson.ParseString(json, "dungeonId");
			var dungeonUid = (ulong)CorgiJson.ParseLong(json, "dungeonUid");
			var stageUid = (ulong) CorgiJson.ParseLong(json, "stageUid");
			var dungeonTypeInt = CorgiJson.ParseInt(json, "dungeonType");
			var grade = (uint)CorgiJson.ParseInt(json, "grade");
			var level = (uint) CorgiJson.ParseInt(json, "level");
			var affixList = CorgiJson.ParseArrayLong(json, "affix");

			DBId = dungeonId;
			_characterId = characterId;
			_curStageUid = stageUid;

			_dungeonType = (DungeonCriteriaType) dungeonTypeInt;
			_grade = grade;
			if (level <= 0)
			{
				_level = 1;
			}
			else
			{
				_level = level;
			}

			if (affixList != null && affixList.Count > 0)
			{
				foreach (var affixUid in affixList)
				{
					AffixList.Add((ulong)affixUid);
				}
			}
			
		    return Load(dungeonUid);
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

		    var gradeStr = string.Format("instance_grade.{0}", _grade);
		    var gradeSpec = GameData.GetData<InstanceGradeInfoSpec>(gradeStr);
		    if (gradeSpec == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid grade uid : {0}", gradeStr);
			    return false;
		    }

		    _monsterMod = gradeSpec.MonsterMod;
		    
		    var structure = new InvisibleStructure(this);

		    structure.SetLevel(_level);
		    
		    if (structure.Load(0) == false)
		    {
			    return false;
		    }
		    
		    structure.UpdateSkills(AffixList);

		    StructureList.Add(structure);

		    return true;
	    }
	    
        protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
            switch (_state.Value)
            {
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
                case DungeonState.RewardCompleted:
                    _state.Set(DungeonState.Destroy);
                    break;
                default:
                    base.Tick(deltaTick, logNode);
                    break;
            }
        }
	    
        public override Stage CreateNewStage(ulong nextStageUid)
        {
	        Stage newStage = null;
	        if (_dungeonType == DungeonCriteriaType.DctLabyrinth)
	        {
		        newStage = new StageInstanceLabyrinth(this);
	        }else if (_dungeonType == DungeonCriteriaType.DctBastion)
	        {
		        newStage = new StageInstanceBastion(this);
		        
	        }else if (_dungeonType == DungeonCriteriaType.DctChapter)
	        {
		        newStage = new StageInstanceChapter(this);
	        }else if (_dungeonType == DungeonCriteriaType.DctMine)
	        {
		        newStage = new StageInstanceMine(this);
	        }

	        if (newStage == null)
	        {
		        return null;
	        }

	        if (newStage.Load(_curStageUid) == false)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance.");
		        return null;
	        }

	        CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Create Stage[{0}] / {1}", _curStageIndex, _curStageUid);
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
	        }else
	        {
				_state.Set(DungeonState.Lose);
	        }
            
			Bridge.InstanceDungeonFinish(_characterId,DungeonCriteriaType, DBId,Uid, _curStage.Uid, IsWin);
			
            StopTimer.StartTimer(CorgiLogicConst.RewardDelay);
        }

        public void OnInstanceDungeonCompleted(ulong stageUid)
        {
            if (State != DungeonState.Win && State != DungeonState.Lose)
            {
                throw new CorgiException("invalid dungeon state({0}) in OnStageCompleted", State);
            }
            
            _nextStageUid = stageUid;

            _state.Set(DungeonState.RewardCompleted);
            
        }
        
        public void OnInstanceDungeonStop()
        {
	        _state.Set(DungeonState.Destroy);
        }

        public override List<RequestParam> GetRequireParams()
        {
            var retList = new List<RequestParam>();
	        
			 retList.Add(new RequestParam(RedisRequestType.CharaterInfo, _characterId));

			return retList;
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