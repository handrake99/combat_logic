using System;
using System.Collections.Generic;

//using IdleCs.GameData;
using IdleCs.Library;
using IdleCs.Utils;
using IdleCs.GameLogic;
//using ProtoBuf;

namespace IdleCs.Managers
{
	// Stage 시작과 끝을 관리함
	// IdleCs.UI 에서 Stage대한 Access Point 
    public class TestStageManager: Singleton<TestStageManager>
    {
//	    private TestStage _stage;
//		protected readonly CombatRandom _random = new CombatRandom();
//
//	    public StageState State
//	    {
//		    get { return (_stage != null) ? _stage.State : StageState.None; }
//	    }
//	    
////		public bool StartFromSaveFile { get { return _startFromSaveFile; } set { _startFromSaveFile = value; } }
//
//	    public Unit GetUnit(string objectId)
//	    {
//		    return _stage?.GetUnit(objectId);
//	    }
//
//	    //public RspBattleStart BattleStartBlock;
//		
////		public bool IsGiveUpDungeon { get { return _isGiveUp; } }
////		public bool IsGameOver { get { return _isGameOver; } }
////		public bool IsTestDungeon { get { return _manager is CombatManagerTest; } }
//		public List<Character> CharList { get { return _stage.CharList; } } 
//		public List<Monster> MonsterList { get { return _stage.MonsterList; } }
//		public List<Unit> TurnList { get { return _stage.TurnList; } }
//
//	    public string CurrentActionUnitId
//	    {
//		    get { return _stage.CurrentActionUnitId; }
//	    }
//	    
//	    public int CurrentTurn
//	    {
//		    get { return _stage.TurnIndex + 1; }
//	    }
//
//	    public CombatRandom CombatRandom { get { return _random; } }
//
//        public TestStage GetCurStage()
//        {
//	        return _stage;
//        }
//        
//        // Enter new Stage
//	    public CombatLogNode EnterStage(uint stageUid, List<uint> charList)
//		{
//			if (_stage != null)
//			{
//				CorgiLog.LogError("Invalid State DungeonManager <{0}> in EnterDungeon", stageUid);
//			}
//			_stage = null;
//
//			_stage = Create(stageUid);
//			if (_stage == null)
//			{
//				CorgiLog.LogError("cant create dungeon : {0}", stageUid);
//				return null;
//			}
//			
//			List<CorgiPosition> positions = new List<CorgiPosition> {pos0, pos1, pos2, pos3};
//			
//			return _stage.EnterStage(characterList, positions);
//		}
//	    
//	    public bool StartCombat()
//	    {
//		    // if any initialization is needed...
//		    if (_stage == null)
//		    {
//			    CorgiLog.LogError("Invalid State DungeonManager<0> in StartCombat");
//			    return false;
//		    }
//		    return _stage.StartCombat();
//	    }
//
//	    public CombatLogNode PreAction()
//	    {
//		    if (_stage == null)
//		    {
//			    CorgiLog.LogError("Invalid State DungeonManager<0> in PreAction");
//			    return null;
//		    }
//
//		    try
//		    {
//			    return _stage.PreAction();
//		    }
//		    catch (StackOverflowException se)
//		    {
//			    Console.WriteLine(se);
//			    throw;
//		    }
//		    catch (Exception e)
//		    {
//			    Console.WriteLine(e);
//			    throw;
//		    }
//	    }
//
//	    public bool PassAction()
//	    {
//		    if (_stage == null)
//		    {
//			    CorgiLog.LogError("Invalid State DungeonManager<0> in PassAction");
//			    return false;
//		    }
//
//		    _stage.PassAction();
//		    return true;
//	    }
//
//	    
//
//	    public bool CanDoAction()
//	    {
//		    if (_stage == null)
//		    {
//			    CorgiLog.LogError("Invalid State DungeonManager<0> in CanDoAction");
//			    return false;
//		    }
//
//		    return _stage.CanDoAction();
//	    }
//
//	    /// <summary>
//	    /// Unit 의 행동이 왔을때 실행한다
//	    /// </summary>
//	    /// <param name="actionInput"></param>
//	    /// <returns></returns>
//		public SkillActionLogNode DoAction(ActionInput actionInput)
//		{
//			try
//			{
//				var actionLogNode = _stage.DoAction(actionInput);
//				if (actionLogNode != null)
//				{
//					CorgiLog.LogWarning("*** DoAction() : " + actionLogNode.Skill.Name);
//				}
//
//				return actionLogNode;
//			}
//			catch (System.StackOverflowException se)
//			{
//				Console.WriteLine(se);
//				throw;
//			}
//			catch (Exception e)
//			{
//				Console.WriteLine(e);
//				throw;
//			}
//		}
//
//
//	    /// <summary>
//	    /// 지정 가능한 target 확인
//	    /// UI 에서 가져다 써야한다.!!!
//	    /// </summary>
//	    /// <param name="unitId"></param>
//	    /// <param name="skillId"></param>
//	    /// <returns></returns>
//	    public List<string> GetAvailableTarget(string unitId, string skillId)
//	    {
//		    return _stage.GetAvailableTarget(unitId, skillId);
//	    }
//		
////		public void OnSkill()
//         //		{
//         //            _manager.OnSkill();
//         //		}
//
//		public bool EnterTurn()
//		{
//			if (_stage == null)
//			{
//				CorgiLog.LogError("Invalid State DungeonManager<0> in EnterTurn");
//				return false;
//			}
//			return _stage.EnterTurn();
//		}
//
//		public bool FinishTurn()
//		{
//			if (_stage == null)
//			{
//				CorgiLog.LogError("Invalid State DungeonManager<0> in FinishTurn");
//				return false;
//			}
//			return _stage.FinishTurn();
//		}
//		
//		public bool FinishStage()
//		{
//			if (_stage == null)
//			{
//				CorgiLog.LogError("Invalid State DungeonManager<0> in FinishStage");
//				return false;
//			}
//			return _stage.FinishStage();
//		}
//
//		public bool GameOver()
//		{
//			
//			return _stage.GameOver();
//		}
//	    
//        public void OnDestroy()
//        {
//            if(_stage == null)
//            {
//                return;
//            }
//            _stage.OnDestroy();
//            _stage= null;
//        }
//	    
//	    // Static Factory
//	    //private static readonly Dictionary<StageRoundType, Type> DungeonTypeMap = new Dictionary<StageRoundType, Type>();
//
//	    protected override void Init()
//	    {
//		    SkillFactory.Init();
//		    SkillCompFactory.Init();
//		    SkillConditionCompFactory.Init();
//		    SkillTargetCompFactory.Init();
//		    //SkillFeatureCompFactory.Init();
//	    }
//
//	    private static Dungeon CreateDungeon()
//	    {
//		    //var type = DungeonTypeMap[rspBattleStart.battle_type];
////		    if (type == null)
////		    {
////			    //CorgiLog.LogError("invalid dungeon type : <0>", rspBattleStart.battle_type.ToString());
////			    return null;
////		    }
////		    var newDungeon = Activator.CreateInstance(type) as Dungeon;
////
////		    if (newDungeon == null)
////		    {
////			    //CorgiLog.LogError("Failed Create Instance : <0>", rspBattleStart.battle_type.ToString());
////			    return null;
////		    }
////		    
////		    return newDungeon.LoadObject(rspBattleStart) ? newDungeon : null;
//		    return null;
//	    }
//		    
//		    return newDungeon.LoadObject(rspBattleStart) ? newDungeon : null;
//		    return null;
//	    }

//	    private static TestStage Create(uint stageUid)
//	    {
//			var stage = new TestStage();
//			stage.LoadTest(999);
//		    return stage;
//	    }
//
//	    public void OnDeadCompletely(Unit unit, CombatLogNode logNode)
//	    {
//		    _stage.OnDeadCompletely(unit, logNode);
//	    }
//
//	    public List<Unit> GetAliveUnitList(bool isEnemy)
//	    {
//		    List<Unit> targetList = new List<Unit>();
//		    if (!isEnemy)
//		    {
//			    foreach (var unit in MonsterList)
//			    {
//				    if (unit.IsLive())
//					    targetList.Add(unit);
//			    }
//		    }
//		    else
//		    {
//			    foreach (var unit in CharList)
//			    {
//				    if (unit.IsLive())
//					    targetList.Add(unit);
//			    }
//		    }
//	    }

    }
}
