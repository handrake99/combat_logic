using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class DungeonRift : Dungeon, ICorgiInterface<InstanceDungeonInfoSpec>
    {
	    private InstanceDungeonInfoSpec _spec;

	    private string _characterId;

	    private DungeonCriteriaType _dungeonType;
	    private uint _grade;
	    private uint _level;
	    private float _monsterMod;
	    
	    private long _initMaxHP;
	    private long _initCurHP;

	    private SharedRift _sharedRift;
	    private long _totalDamage;

	    public long CumulativeDamage
	    {
		    get;
		    set;
	    }
	    
	    public bool HaveToSendCumulativeDamage;
	    public readonly Dictionary<string, long> CumulativeDamageMap = new Dictionary<string, long>();

	    private Dictionary<ClassType, ulong> _partyBuffMap;
	    private readonly HashSet<ClassType> _currentPartyBuff = new HashSet<ClassType>(); // 현재 적용되고 있는 파티버프. 직업으로 구분. 
	    private readonly Queue<string> _partyMemberEnterQueue = new Queue<string>(); // 던전에 들어온 순서 기록. 던전마다 따로 관리.

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
		    get { return DungeonCriteriaType.DctRift; }
	    }
	    
	    public uint MonsterLevel
	    {
		    get { return _level; }
	    }

	    public float MonsterMod => _monsterMod;

	    public DungeonRift(ICombatBridge bridge)
			: base(bridge, DungeonType.Rift)
	    {
		    _partyBuffMap = new Dictionary<ClassType, ulong>
		    {
			    {ClassType.CtWarrior, GameDataManager.GetUidByString("skill.rift.warrior.buff.1")},
			    {ClassType.CtRogue, GameDataManager.GetUidByString("skill.rift.rogue.buff.1")},
			    {ClassType.CtMage, GameDataManager.GetUidByString("skill.rift.mage.buff.1")},
			    {ClassType.CtDruid, GameDataManager.GetUidByString("skill.rift.druid.buff.1")},
		    };
	    }

	    protected override bool LoadInternal(CorgiSharedObject sObject)
	    {
		    if (base.LoadInternal(sObject) == false)
		    {
			    return false;
		    }

		    var riftInfo = sObject as SharedRift;

		    if (riftInfo == null)
		    {
			    return false;
		    }
		    
		    
		    
		    
		    _nextStageUid = riftInfo.stageUid;
		    _grade = riftInfo.grade;
		    _level = riftInfo.level;

		    _sharedRift = riftInfo;
		    
		    return LoadInternal(riftInfo.dungeonUid);
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
		    
		    var gradeStr = string.Format("rift_grade.{0}", _grade);
		    var gradeSpec = GameData.GetData<InstanceGradeInfoSpec>(gradeStr);
		    if (gradeSpec == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid rift dungeon grade uid : {0}", gradeStr);
			    return false;
		    }

		    _monsterMod = gradeSpec.MonsterMod;

		    return true;
	    }
	    
        protected override void Tick(ulong deltaTick, DungeonLogNode logNode)
        {
            switch (_state.Value)
            {
	            case DungeonState.InCombat:
		            TickInCombat(deltaTick, logNode);
		            if (_partyMemberEnterQueue != null && _partyMemberEnterQueue.Count != 0)
		            {
			            UpdatePartyBuff(logNode.CombatLogNode);
		            }
		            if (HaveToSendCumulativeDamage)
		            {
			            CreateCumulativeDamageLogNode(logNode.CombatLogNode);
		            }
		            var curDamage = 0L;
		            var curHeal = 0L;
		            AccumulateDamage(ref curDamage, ref curHeal, logNode.CombatLogNode);
		            if (curDamage > 0)
		            {
			            _totalDamage += curDamage;
			            CumulativeDamage += curDamage;
			            _sharedRift.RecordDamage(_characterId, curDamage);
		            }
		            if (curHeal > 0)
		            {
			            _sharedRift.RecordHeal(curHeal);
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
        

        public override Stage CreateNewStage(ulong nextStageUid)
        {
	        var newStage = new StageInstanceRift(this); 
	        
	        newStage.InitBossHP(_initMaxHP, _initCurHP);
	        
	        if (newStage.Load(nextStageUid) == false)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance.");
		        return null;
	        }

	        return newStage;
        }
        
        public void InitBossHP(long maxHp, long curHp)
        {
	        _initMaxHP = maxHp;
	        _initCurHP = curHp;
        }
        
        private void AccumulateDamage(ref long totalDamage, ref long totalHeal, CombatLogNode logNode)
        {
	        if (logNode == null)
	        {
		        return;
	        }
	        
			if (logNode is DamageSkillCompLogNode damageChild)
			{
				Unit caster = GetUnit(damageChild.CasterId);
				Unit target = GetUnit(damageChild.TargetId);
				if (caster?.CombatSideType == CombatSideType.Player && target?.CombatSideType == CombatSideType.Enemy)
				{
					totalDamage += (long)damageChild.FinalDamage;
				}
			}

			else if (logNode is HealSkillCompLogNode healChild)
			{
				Unit target = GetUnit(healChild.TargetId);
				if (target != null && target.CombatSideType == CombatSideType.Enemy)
				{
					totalHeal += healChild.AppliedHeal;
				}
			}

	        foreach (var child in logNode.Childs)
	        {
				AccumulateDamage(ref totalDamage, ref totalHeal, child);
	        }
        }

        public void OnUpdateRiftInfo(long curHp)
        {
	        if (_curStage == null || _curStage.MonsterList.Count == 0)
	        {
		        return;
	        }

	        var monster = _curStage.MonsterList[0];
	        var isUpdated = false;
	        
	        if (monster.CurHP > 0)
	        {
				monster.ResetHP(curHp);
				if (monster.CurHP == 0)
				{
					IsWin = true;
					_state.Set(DungeonState.FinishStageByOther);
					isUpdated = true;
				}
	        }

	        if (isUpdated)
	        {
		        CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Change DungeonState [{0}] to [{1}] in Dungeon[{2}][{3}]", _state.Prev, _state.Value, DungeonType, DungeonKey);
	        }
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
            
			Bridge.RiftFinish(DungeonKey, _sharedRift.dungeonId, _characterId, _totalDamage, IsWin);

			if (IsWin)
			{
				Bridge.RiftDead(_sharedRift.dungeonId, _characterId);
			}
			
			StopTimer.StartTimer(CorgiLogicConst.RewardDelay);
        }

        public void OnRiftDungeonCompleted(ulong stageUid)
        {
            if (State != DungeonState.Win && State != DungeonState.Lose)
            {
                throw new CorgiException("invalid dungeon state({0}) in OnStageCompleted", State);
            }
            
            _nextStageUid = stageUid;

            _state.Set(DungeonState.RewardCompleted);
        }
        
        public void OnRiftDungeonStop()
        {
	        Bridge.RiftFinish(DungeonKey, _sharedRift.dungeonId, _characterId, _totalDamage, IsWin);
	        _state.Set(DungeonState.Destroy);
        }

        public override List<RequestParam> GetRequireParams()
        {
            var retList = new List<RequestParam>();
	        
			retList.Add(new RequestParam(RedisRequestType.CharaterInfo, _characterId));

			return retList;
        }
        
        protected override Deck GetCurrentDeck(Unit unit)
        {
			return Bridge.GetSoloPartyDeck(_characterId, unit);
        }

        public bool SetPartyBuff(List<string> charIdList)
        {
	        var structure = new InvisibleStructure(this);

	        structure.SetLevel(_level);
		    
	        if (structure.Load(0) == false)
	        {
		        return false;
	        }

	        var skillUidList = new List<ulong>();

	        foreach (var charId in charIdList)
	        {
		        var charInfo = Bridge.GetCharInfo(charId);

		        if (charInfo == null)
		        {
			        continue;
		        }

		        var uid = charInfo.uid;
		        
		        var sheet = GameData.GetData<CharacterInfoSpec>(uid);
		        
		        if (sheet == null)
		        {
			        continue;
		        }
		        
		        var classType = sheet.ClassType;

		        if (_currentPartyBuff.Contains(classType))
		        {
			        continue;
		        }

		        var partyBuffUid = _partyBuffMap[classType];
		        
		        skillUidList.Add(partyBuffUid);

		        _currentPartyBuff.Add(classType);
	        }
	        
	        structure.UpdateSkills(skillUidList);
	        
	        StructureList.Add(structure);

	        PartyBuffList.AddRange(skillUidList);

	        return true;
        }

        private void UpdatePartyBuff(CombatLogNode combatLogNode)
        {
	        while (_partyMemberEnterQueue.Count > 0)
	        {
		        var characterId = _partyMemberEnterQueue.Dequeue();

		        var charInfo = Bridge.GetCharInfo(characterId);
		        
		        if (charInfo == null)
		        {
			        continue;
		        }

		        var uid = charInfo.uid;

		        var sheet = GameData.GetData<CharacterInfoSpec>(uid);

		        if (sheet == null)
		        {
			        continue;
		        }

		        var logNode = new PartyMemberDungeonEnterLogNode(charInfo.nickname);
		        combatLogNode.AddChild(logNode);
		        
		        var classType = sheet.ClassType;

		        if (_currentPartyBuff.Contains(classType))
		        {
			        continue;
		        }

		        var partyBuffUid = _partyBuffMap[classType];

		        if (StructureList == null || StructureList.Count == 0)
		        {
			        continue;
		        }

		        if (StructureList[0] is InvisibleStructure structure)
		        {
			        structure.AddSkillAndTriggerEvent(partyBuffUid, logNode, CombatEventType.OnEnterCombat);
			        _currentPartyBuff.Add(classType);
			        PartyBuffList.Add(partyBuffUid);
		        }
	        }
        }

        public void UpdatePartyMemberEnterQueue(Queue<string> queue)
        {
	        foreach (var charId in queue)
	        {
		        if (charId == _characterId)
		        {
			        continue;
		        }
		        
		        _partyMemberEnterQueue.Enqueue(charId);
	        }
        }

        private void CreateCumulativeDamageLogNode(CombatLogNode logNode)
        {
	        foreach (var characterId in CumulativeDamageMap.Keys)
	        {
		        if (characterId == _characterId)
		        {
			        continue;
		        }
		        
		        var damage = CumulativeDamageMap[characterId];
		        var cumulativeDamageLogNode = new CumulativeDamageLogNode(characterId, damage);
		        logNode.AddChild(cumulativeDamageLogNode);
	        }

	        CumulativeDamageMap.Clear();
	        HaveToSendCumulativeDamage = false;
        }
    }
}
