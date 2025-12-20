using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Utils;

using IdleCs.GameLog;

namespace IdleCs.GameLogic
{

	/// <summary>
	/// 전투 unit base class
	/// </summary>
	public class Unit : CorgiCombatObject
	{
		/// <summary>
		/// Static Data
		/// </summary>

//		string _name;

//		private uint _level;
//
//		private string _charName;
//		private string _modelName;
//		private string _combatModel;
		
		string _portraitName;
		private ClassType _classType;
		private RoleType _roleType;
		
		//private string _desc;
		
		public SkillAttack Attack { get; protected set; }
		
		private List<SkillActive> _activeSkills;
		public List<SkillActive> ActiveSkills { get {return _activeSkills; }}
		
		// characdter Passive 
		private List<SkillPassive> _passiveSkills;
		public List<SkillPassive> PassiveSkills { get {return _passiveSkills; }}
		

		private Unit _summoner = null;

		public Unit Summoner => _summoner;

		public void SetSummoner(Unit unit)
		{
			_summoner = unit;
		}

		protected void AddSkill(Skill skill)
		{
			if (skill == null)
			{
				return;
			}

			if (skill.SkillType == SkillType.Passive)
			{
				AddPassiveSkill(skill as SkillPassive);
			}else if (skill.SkillType != SkillType.Attack)
			{
				AddActiveSkill(skill as SkillActive);
			}
		}

		protected void AddActiveSkill(SkillActive skill)
		{
			if (skill == null) { return; }
			_activeSkills.Add(skill);
		}
		
		protected void ResetAttributeMap()
		{
			_attributePowerMap[SkillAttributeType.SatFire] = 0;
			_attributePowerMap[SkillAttributeType.SatIce] = 0;
			_attributePowerMap[SkillAttributeType.SatLightning] = 0;
			_attributePowerMap[SkillAttributeType.SatEarth] = 0;
			_attributePowerMap[SkillAttributeType.SatHoly] = 0;
			_attributePowerMap[SkillAttributeType.SatShadow] = 0;
		}

		protected void ResetActiveSkills()
		{
			_activeSkills.Clear();
		}
		
		protected void AddPassiveSkill(SkillPassive skill)
		{
			if (skill == null) { return; }
			_passiveSkills.Add(skill);
		}
		
		protected void RemovePassiveSkill(SkillPassive skill)
		{
			if (skill == null) { return; }
			_passiveSkills.Remove(skill);
		}
		
		
		private readonly Dictionary<StatType, UnitStat> _statMap = new Dictionary<StatType, UnitStat>();

		protected Dictionary<StatType, UnitStat> StatMap
		{
			get { return _statMap; }
		}
		
		private readonly Dictionary<SkillAttributeType, uint> _attributePowerMap = new Dictionary<SkillAttributeType, uint>();

		protected Dictionary<SkillAttributeType, uint> AttributePowerMap => _attributePowerMap;

		public uint Level { get; protected set; }

		public virtual string Name { get { return null; } }
		public virtual string CharName { get { return null; } }
		public virtual string ModelName { get { return null; } }
		public virtual string PortraitName { get { return null; } }
		
		public virtual ClassType ClassType { get; protected set; }
		public virtual RoleType RoleType { get; protected set; }

		private Dungeon _dungeon;

		public Dungeon Dungeon => _dungeon;

		private CombatSideType _combatSideType;
		
		public CombatSideType CombatSideType
		{
			get { return _combatSideType; }
			set { _combatSideType = value; }
		}
		
		public float UnitSize { get; protected set; } 
		public float AttackRange { get; set; } 
		
		// meter / second
		public virtual float MoveSpeed 
		{
			get { return 2.0f; }
		}

		public float GetRange(Unit target)
		{
			return AttackRange + UnitSize + target.UnitSize;
		}

		/// <summary>
		/// Unit State 관리
		/// </summary>
		private CorgiFSM _unitFSM;
		public UnitState UnitState
		{
			get { return _unitFSM.CurState; }
		}

		/// <summary>
		/// Get Stat
		/// </summary>
		public long MaxHP { get { return GetStat(StatType.StMaxHp); } }
		public long AttackSpeed { get { return GetStat(StatType.StAttackSpeed); } }
		public long AttackPower { get { return GetStat(StatType.StAttackPower); } }
		public long Defence { get {return GetStat(StatType.StDefence); }}
		public long Hit { get { return GetStat(StatType.StHit); } }
		public long Evasion { get {return GetStat(StatType.StEvasion); }}
		public long CritRate { get {return GetStat(StatType.StCrit); }}
		public long Resilience { get { return GetStat(StatType.StResilience); } }

		public long CritDmg { get { return GetStat(StatType.StCritDmg); }}
		
		/// <summary>
		/// Dynamic Data
		/// </summary>

		
		// position
		private CorgiPosition _initialPosition;
		private CorgiPosition _position;
		
		public CorgiPosition Position { get{return _position;} set { _position = value; } } 
		public CorgiPosition ArrivalPosition { get; set; } 
		
		
		public CorgiPosition InitialPosition
		{
			get { return _initialPosition;}
			set { _initialPosition = value; }
		}

		public void ResetPosition(CorgiPosition init, CorgiPosition cur, CorgiPosition arrive)
		{
			_initialPosition = init;
			_position = cur;
			ArrivalPosition = arrive;
		}
		
		// HP
		long _curHP = 1;
		public long CurHP { get { return _curHP; } protected set{_curHP = value;}}

		// barrier
		private long _curBarrier;
		public long CurBarrier => _curBarrier;

		public float CurHPRate
		{
			get { return (float) _curHP / MaxHP; }
		}
		
		// doing skill
		private SkillActive _curAction = null;

		public SkillActive CurAction => _curAction;

		// mana
		private int _curMana = 0;
		public int CurMana { get { return _curMana; } }

		protected int _maxMana = 0;
		public int MaxMana => _maxMana;
		private int _maxManaDiff = 0;
		
		private int _restoreNextMana = 0;
		public int NextMana => _restoreNextMana;
		
		// skill cycle
		protected int _curSkillIndex = 0;
		protected SkillActive _curActiveSkill = null;

		public int CurSkillIndex => _curSkillIndex;

		public SkillActive GetCurActiveSkill()
		{
			return _curActiveSkill;
		}
		
		public virtual SkillActive GetNextActiveSkill()
		{
			if (_activeSkills.Count == 0)
			{
				return null;
			}

			var nextIndex = _curSkillIndex + 1;
			
			if (nextIndex >= _activeSkills.Count)
			{
				nextIndex = 0;
			}

			_curSkillIndex = nextIndex;
			return _activeSkills[nextIndex];
		}

		protected virtual SkillActive GetInstantSkill(bool instant = false)
		{
			return null;
		}

		// 버프 디버프
		private List<SkillEffectInst> _continuousList = new List<SkillEffectInst>();
		private List<SkillEffectInst> _auraList = new List<SkillEffectInst>();
		
		private List<SkillEffectInst> _tempList = new List<SkillEffectInst>();
		protected bool _isUpdatedEffect = false;

		private MezType _mezState = MezType.None;

		public MezType MezState => _mezState;

		//private UnitCombatState _unitState = UnitCombatState.Ready;
		
//		public UnitCombatState GetUnitState()
//		{
//			return _unitState;
//		}

		public List<SkillEffectInst> ContinuousList
		{
			get { return _continuousList; }
		}

		//~ Dynamic Data
		
		public bool IsLive()
		{
			return _curHP > 0;
		}

		public bool IsFriend(Unit caster)
		{
			return CombatSideType == caster.CombatSideType;
		}

		/// <summary>
		/// for Casting State
		/// </summary>
		public bool IsCasting
		{
			get
			{
				var skill = CurAction;
				if (skill == null) return false;
				return skill.IsCasting;
			}
		}
		/// <summary>
		/// for Casting State
		/// </summary>
		public bool IsChanneling 
		{
			get
			{
				var skill = CurAction;
				if (skill == null) return false;
				return skill.IsChanneling;
			}
		}
		

		/// <summary>
		/// Temparary Members
		/// </summary>
		private List<PassiveSkillComp> _cachedPassiveList = new List<PassiveSkillComp>();

		public Unit(Dungeon dungeon)
		{
			EventManager = new EventManager(CombatEventCategory.Unit);
			EventManager.Register(CombatEventType.OnDeadCompletely, OnDeadCompletely);
			
			_unitFSM = new CorgiFSM(this, UnitState.Idle);
			
			_activeSkills = new List<SkillActive>();
			_passiveSkills = new List<SkillPassive>();
			
			//Set Status
			_statMap.Add(StatType.StMaxHp, new UnitStat(StatType.StMaxHp));
			_statMap.Add(StatType.StAttackSpeed, new UnitStat(StatType.StAttackSpeed));
			_statMap.Add(StatType.StAttackPower, new UnitStat(StatType.StAttackPower));
			_statMap.Add(StatType.StDefence, new UnitStat(StatType.StDefence));
			_statMap.Add(StatType.StHit, new UnitStat(StatType.StHit));
			_statMap.Add(StatType.StEvasion, new UnitStat(StatType.StEvasion));
			_statMap.Add(StatType.StCrit, new UnitStat(StatType.StCrit));
			_statMap.Add(StatType.StResilience, new UnitStat(StatType.StResilience));
			
			_statMap.Add(StatType.StCritDmg, new UnitStat(StatType.StCritDmg));
			
			_attributePowerMap.Add(SkillAttributeType.SatFire, 0);
			_attributePowerMap.Add(SkillAttributeType.SatIce, 0);
			_attributePowerMap.Add(SkillAttributeType.SatLightning, 0);
			_attributePowerMap.Add(SkillAttributeType.SatEarth, 0);
			_attributePowerMap.Add(SkillAttributeType.SatHoly, 0);
			_attributePowerMap.Add(SkillAttributeType.SatShadow, 0);
			
	        // set fsm
	        _unitFSM
		        .RegisterState( UnitState.None, new FSMUnitNone(this))
		        .RegisterState( UnitState.Idle, new FSMUnitIdle(this))
		        .RegisterState( UnitState.Exploration, new FSMUnitExploration(this))
		        .RegisterState( UnitState.Moving, new FSMUnitMoving(this))
		        .RegisterState( UnitState.Action, new FSMUnitAction(this))
		        .RegisterState( UnitState.Actioning, new FSMUnitActioning(this))
		        .RegisterState( UnitState.Casting, new FSMUnitCasting(this))
		        .RegisterState( UnitState.Channeling, new FSMUnitChanneling(this))
		        
		        .RegisterState( UnitState.MezStun, new FSMUnitMezStun(this))
		        .RegisterState( UnitState.MezFortify, new FSMUnitMezFortify(this))
		        .RegisterState( UnitState.MezExausted, new FSMUnitMezExausted(this))
		        .RegisterState( UnitState.MezSilence, new FSMUnitMezSilence(this))
		        .RegisterState( UnitState.MezSleep, new FSMUnitMezSleep(this))
		        .RegisterState( UnitState.Dead, new FSMUnitDead(this))
		        .RegisterState( UnitState.Destroyed, new FSMUnitDestroyed(this));

	        // set fsm trigger
	        _unitFSM
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Idle)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Moving)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Action)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Casting)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Channeling)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.Dead)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.MezStun)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.MezFortify)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.MezExausted)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.MezSilence)
		        .RegisterTrigger(UnitTrigger.OnExploration, UnitState.MezSleep)
		        
		        .RegisterTrigger(UnitTrigger.OnEnterCombat, UnitState.Idle)
		        .RegisterTrigger(UnitTrigger.OnEnterCombat, UnitState.Exploration)
		        
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Idle)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Moving)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Action)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Actioning)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Casting)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Channeling)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.Dead)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.MezStun)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.MezFortify)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.MezExausted)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.MezSilence)
		        .RegisterTrigger(UnitTrigger.OnFinishCombat, UnitState.MezSleep)
		        
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.Moving)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.Action)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.Actioning)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.Casting)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.Channeling)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.MezStun)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.MezFortify)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.MezExausted)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.MezSilence)
		        .RegisterTrigger(UnitTrigger.OnDead, UnitState.MezSleep)
		        
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.Actioning)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.Casting)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.Channeling)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.MezStun)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.MezFortify)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.MezExausted)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.MezSilence)
		        .RegisterTrigger(UnitTrigger.OnCancelCasting, UnitState.MezSleep)
		        .RegisterTrigger(UnitTrigger.ClosedToTarget, UnitState.Moving)
		        
		        .RegisterTrigger(UnitTrigger.OnMez, UnitState.Moving)
		        .RegisterTrigger(UnitTrigger.OnMez, UnitState.Action)
		        .RegisterTrigger(UnitTrigger.OnMez, UnitState.Actioning)
		        .RegisterTrigger(UnitTrigger.OnMez, UnitState.Casting)
		        .RegisterTrigger(UnitTrigger.OnMez, UnitState.Channeling)
		        
		        .RegisterTrigger(UnitTrigger.OnMezEnd, UnitState.MezStun)
		        .RegisterTrigger(UnitTrigger.OnMezEnd, UnitState.MezFortify)
		        .RegisterTrigger(UnitTrigger.OnMezEnd, UnitState.MezExausted)
		        .RegisterTrigger(UnitTrigger.OnMezEnd, UnitState.MezSilence)
		        .RegisterTrigger(UnitTrigger.OnMezEnd, UnitState.MezSleep);
	        
//				.RegisterTrigger(UnitTrigger.OnEnterCombat, UnitState.None, UnitState.Moving)
//				.RegisterTrigger(UnitTrigger.OnEnterCombat, UnitState.None, UnitState.Moving)
//				.RegisterTrigger(UnitTrigger.OnEnterCombat, UnitState.None, UnitState.Moving);

			_dungeon = dungeon;
		}

		protected virtual void Initialize()
		{
			_activeSkills.Clear();
			_passiveSkills.Clear();
		}
		
		protected override bool LoadInternal(ulong uid)
		{
	        _curHP = MaxHP;
	        
	        //todo load from sheet
	        InitialPosition = new CorgiPosition(0,0, 0, 0);
	        Position = new CorgiPosition(0,0, 0, 0);
	        
			return true;
		}

		public void OnExploration(DungeonLogNode dungeonLogNode)
		{
			var curLog = new ExplorationLogNode();
			curLog.DungeonLogNode = dungeonLogNode;
			
			_unitFSM.Trigger(UnitTrigger.OnExploration, curLog);
		}
		
		// 임시 코드. 추후 sheet load 로 바꿔야한다.
	    public virtual void OnEnterCombat(CombatLogNode logNode)
		{
			OnUpdateStat(logNode);
			
	        _curHP = MaxHP;
	        
	        // initialize first skill
	        _curSkillIndex = _activeSkills.Count;
	        _curActiveSkill = GetNextActiveSkill();
	        if (_curActiveSkill != null)
	        {
				_maxMana = _curActiveSkill.GetManaCost();
	        }

	        _curMana = 0;
	        _curSkillIndex = 0;
			
	        OnEnterCombatForSkill(logNode);
	        
	        // debugging 
	        CorgiCombatLog.Log(CombatLogCategory.Unit, "[Stat] -------- {0} ---------", this.Name);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]MaxHP   : {0}, {1}", StatMap[StatType.StMaxHp].OrigStat, StatMap[StatType.StMaxHp].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Speed   : {0}, {1}", StatMap[StatType.StAttackSpeed].OrigStat, StatMap[StatType.StAttackSpeed].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]AP      : {0}, {1}", StatMap[StatType.StAttackPower].OrigStat, StatMap[StatType.StAttackPower].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Defence : {0}, {1}", StatMap[StatType.StDefence].OrigStat, StatMap[StatType.StDefence].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Hit     : {0}, {1}", StatMap[StatType.StHit].OrigStat, StatMap[StatType.StHit].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Evasion : {0}, {1}", StatMap[StatType.StEvasion].OrigStat, StatMap[StatType.StEvasion].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Crit    : {0}, {1}", StatMap[StatType.StCrit].OrigStat, StatMap[StatType.StCrit].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]Resil   : {0}, {1}", StatMap[StatType.StResilience].OrigStat, StatMap[StatType.StResilience].AddStat);
	        CorgiCombatLog.Log(CombatLogCategory.Unit,"[Stat]---------------------------");

	        _unitFSM.Trigger(UnitTrigger.OnEnterCombat, logNode);
		}

	    protected virtual void OnEnterCombatForSkill(CombatLogNode logNode)
	    {
		    var eventParam = new EventParamUnit(this);

		    foreach (var skill in ActiveSkills)
		    {
				skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }

		    foreach (var skill in PassiveSkills)
		    {
				skill?.OnEvent(CombatEventType.OnEnterCombat, eventParam, logNode);
		    }
	    }

	    public void OnFinishCombat(CombatLogNode logNode)
	    {
		    if (IsCasting || IsChanneling)
		    {
			    OnCancelCasting(logNode);
		    }
	    }

	    public void OnFinishStage(CombatLogNode logNode)
	    {
		    
		    _unitFSM.Trigger(UnitTrigger.OnFinishCombat, logNode);
	    }

	    public void ResetUnitStatus()
	    {
			ResetHP();
			ResetMana();
			_curBarrier = 0;
			
			_continuousList.Clear();
	    }

	    public void InitStartPosition(CorgiPosition pos)
	    {
		    _initialPosition.SetPos(pos.X, pos.Y);
		    _position.SetPos(pos.X, pos.Y);
		    _position.SetDir(pos.DirX, pos.DirY);
	    }

	    protected override void Tick(ulong deltaTime, TickLogNode logNode)
	    {
		    if (_curAction != null)
		    {
				_curAction.TickInCombat(deltaTime, logNode);
		    }
		    
		    _unitFSM.TickInCombat(deltaTime, logNode);
		    
			if (_curAction != null && _curAction.IsActioned)
			{
				_curAction = null;
			}
		    
		    // check it
		    //logNode.AddUnit(this)skill_continuous.dgn.minotaur.skill1.fire.1;
		    var preCount = _continuousList.Count;
		    foreach (var effectInst in _continuousList)
		    {
			    effectInst.TickInCombat(deltaTime, logNode);
				var postCount = _continuousList.Count;
			    if (effectInst.IsLive == false)
			    {
				    _isUpdatedEffect = true;
			    }

			    if (preCount != postCount)
			    {
				    CorgiCombatLog.LogError(CombatLogCategory.Unit, "[Combat] Continuous List was changed.");
				    break;
			    }
		    }
		    
		    OnUpdateEffect(logNode);
	    }
	    

	    public SkillActionLogNode DoAction(Unit target)
	    {
		    
		    // check instant skill
		    var instantSkill = GetInstantSkill(true);
		    if (instantSkill != null)
		    {
			    _curActiveSkill = instantSkill;
			    return DoSkill(instantSkill, target);
		    }
		    
		    var curSkill = GetCurActiveSkill();
			
			if(Attack == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Unit,$"invalid attack instance");
				CorgiLog.Assert(false);
				return null;
			}

			SkillActive thisSkill = null;
			// check manacost
			if (curSkill != null &&_curMana >= _maxMana)
			{
				// do skill
				// check silence
				if (_mezState == MezType.None)
				{
                    var ret = DoSkill(curSkill, target);
                    if (ret != null)
                    {
	                    return ret;
                    }
				}
			}
			
			 // do attack
			 return DoAttack(target);
		}

	    protected virtual SkillActionLogNode DoAttack(Unit target)
	    {
		    var thisSkill = Attack;
		    
		    
		    // check instant skill

			_curAction = thisSkill;
		    var actionLogNode = thisSkill.DoSkill(target);

		    if (actionLogNode == null)
		    {
			    _curAction = null;
			    return null;
		    }
			
			
			return actionLogNode;
	    }
	    
	    protected virtual SkillActionLogNode DoSkill(SkillActive thisSkill, Unit target)
	    {
			_curAction = thisSkill;
			
			var actionLogNode = thisSkill.DoSkill(target);
			
			if (actionLogNode == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Unit,"DoSkill is failed. {0} ", thisSkill.Name);
				_curAction = null;
				return null;
			}

			OnSkillStart(thisSkill, actionLogNode);

			var eventParam = new EventParamAction(Dungeon, thisSkill, actionLogNode);

			if (thisSkill.IsCasting)
			{
				actionLogNode.ApplyCasting(thisSkill.CastingTime);
				//OnCastingStart
                OnEvent(CombatEventType.OnCastingStart, eventParam, actionLogNode);

				return actionLogNode;
			}
			
			if (thisSkill.IsChanneling)
			{
				actionLogNode.ApplyChanneling(thisSkill.CastingTime);
				//OnCastingStart
                OnEvent(CombatEventType.OnChannelingStart, eventParam, actionLogNode);

				return actionLogNode;
			}
			
			//CorgiLog.LogLine("DoSkill {0}:{1}", this.Name, thisSkill.Name);
			
			return actionLogNode;
		    
	    }

	    void OnAction(CombatLogNode logNode)
	    {
			Dungeon.OnAction(logNode);
	    }

	    public void OnSkillStart(SkillActive thisSkill, CombatLogNode logNode)
	    {
			if (IsLive() == false)
			{
				return;
			}
			
			_curMana = 0;
            //_restoreNextMana = 0;
            
			_curActiveSkill = GetNextActiveSkill();

			var curManaCost = _curActiveSkill.GetManaCost();
			if (curManaCost > 0)
			{
				_maxMana = curManaCost + _maxManaDiff;
			}

			if (_maxMana <= 0)
			{
				_maxMana = 0;
			}
			_maxManaDiff = 0;
            
	    }
	    
		public void OnAttack(SkillActive thisSkill, CombatLogNode logNode)
		{
			if (IsLive() == false)
			{
				return;
			}

			_isUpdatedEffect = true;
			
			OnAction(logNode);
		}

		public virtual void OnSkill(SkillActive thisSkill, CombatLogNode logNode)
		{
			if (IsLive() == false)
			{
				return;
			}

            _curMana += _restoreNextMana;
            _restoreNextMana = 0;
            
			_isUpdatedEffect = true;
			
			OnAction(logNode);
		}

		public void AddMana(int addCount, CombatLogNode logNode)
		{
			var curSkill = GetCurActiveSkill();
			if (curSkill == null)
			{
				return;
			}
			
			if (addCount <= 0)
			{
				return;
			}

			_curMana = _curMana + addCount;

			if (_curMana > _maxMana)
			{
				_curMana = _maxMana;
			}
			
			var newLogNode = new AddManaSkillCompLogNode(this, this, null);
			
			logNode.AddChild(newLogNode);
		}

		public int RestoreNextMana(uint cost)
		{
			_restoreNextMana += (int)cost;

			return _restoreNextMana;
		}
		
		public int ChangeManaCost(int diff)
		{
			_maxManaDiff += diff;

			return diff;
		}

		
		public SkillActive GetSkill(string skillId)
		{
			if (skillId == null)
			{
				return null;
			}
			
			foreach (var curSkill in ActiveSkills)
			{
				if (curSkill != null && (skillId == curSkill.ObjectId))
				{
					return curSkill as SkillActive;
				}
			}

			return null;
		}
		
		protected virtual void DoSkillEffectAction(Action<SkillEffectInst> action)
		{
			for (var i = 0; i < _continuousList.Count; i++)
			{
				var inst = _continuousList[i];

				if (inst == null)
				{
					continue;
				}
				
				inst.DoSkillEffectAction(this, action);
			}
			
			Dungeon.DoSkillEffectAuraAction(this, action);
		}

		public virtual void DoSkillEffectAuraAction(Unit target, Action<SkillEffectInst> action)
		{
			for(var i=0; i<_continuousList.Count; i++)
			{
				var inst = _continuousList[i];
				var auraInst = inst as SkillEffectAuraInst;
				if (auraInst == null)
				{
					continue;
				}
				inst.DoSkillEffectAction(target, action);
			}
		}
		

        public override bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
        {
	        base.OnEvent(eventType, eventParam, logNode);
			
			var eventNode = new SkillEventLogNode(eventType, eventParam, this);
			eventNode.Parent = logNode;

			bool ret = false;

			try
			{
				foreach (var skill in _activeSkills)
				{
					if (skill == null)
					{
						continue;
					}

					if (skill.OnEvent(eventType, eventParam, eventNode) )
					{
						ret = true;
					}
				}
				
				DoSkillEffectAction((inst) =>
				{
					if (inst.OnEvent(eventType, eventParam, eventNode))
					{
						ret = true;
					}
				});
			}
			catch (Exception e)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Unit, e);
				throw;
			}

			if (eventNode.ChildCount > 0)
			{
				logNode.AddChild(eventNode);
			}

			return ret;
		}

		public virtual long GetStat(StatType statType)
		{
			if(_statMap.ContainsKey(statType) == true)
			{
				var retStat = _statMap[statType].GetStat();				
				
                return retStat;
			}

			return 0;
		}
		
		public uint GetAttributePower(SkillAttributeType attrType)
		{
			if(_attributePowerMap.ContainsKey(attrType) == true)
			{
                return _attributePowerMap[attrType];
			}

			return 0;
		}

		/// <summary>
		/// apply
		/// tempList에 추가후 한번에 buff/debuff list 에 넣는다.
		/// 스킬 적용시 지속효과가 적용되는것을 막기 위함.
		/// </summary>
		/// <param name="inst"></param>
		public virtual void ApplySkillEffect(SkillEffectInst inst, CombatLogNode logNode)
		{
			if (inst == null)
			{
				return;
			}

			_tempList.Add(inst);
			
			_isUpdatedEffect = true;
		}
		
		public void CancelSkillEffect(SkillEffectInst inst, CombatLogNode logNode)
		{
			if (inst == null)
			{
				return;
			}

			foreach (var curInst in ContinuousList)
			{
				if (curInst == null || curInst.ObjectId != inst.ObjectId)
				{
					continue;
				}

				curInst.DoOver(logNode);
			}
			
			_isUpdatedEffect = true;
		}
		
		/// <summary>
		/// Interrupt Casting
		/// </summary>
		/// <param name="logNode"></param>
		/// <returns></returns>
		public bool InterruptCasting(Unit caster, InterruptSkillCompLogNode logNode)
		{
			if (IsCasting == false && IsChanneling == false)
			{
				return false;
			}
			

			var curSkill = _curAction;
			if (curSkill == null)
			{
				return false;
			}
			
			// check interrupt immune
			if (curSkill.IsImmune)
			{
				logNode.IsImmune = true;
				return false;
			}
			
			// check immune
            if(this.ApplyImmune(ImmuneType.Interrupt) == true)
            {
                logNode.IsImmune = true;
                return false;
            }
            var isHit = ApplyHit(caster, logNode);
            if (isHit == false)
            {
	            return false;
            }

            if (OnCancelCasting(logNode) == false)
            {
	            return false;
            }

            logNode.AddDetailLog($"{caster.Name} Interrupted {curSkill.Name}to {this.Name}");
            logNode.IsSuccess = true;
            
			_unitFSM.Trigger(UnitTrigger.OnCancelCasting, logNode);
			
			return true;
		}

		public bool OnCancelCasting(CombatLogNode logNode)
		{
			var curSkill = _curAction;
			if (curSkill == null)
			{
				return false;
			}
			
			if(curSkill.OnCancelCasting(logNode) == false)
			{
				return false;
			}
			
			OnSkill(curSkill, logNode);
			
			return true;
		}

//		/// <summary>
//		/// Apply Owned Aura Inst
//		/// - Tick
//		/// - Not Passive
//		/// </summary>
//		/// <param name="inst"></param>
//		public void ApplyOwnAura(SkillEffectAuraInst inst)
//		{
//			if (inst.Owner.ObjectId != this.ObjectId)
//			{
//				return;
//			}
//
//			_auraOwnList.Add(inst);
//		}
//
//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="inst"></param>
//		public void ApplyOtherAura(SkillEffectAuraInst inst, CombatLogNode logNode)
//		{
//			if (inst == null)
//			{
//				return;
//			}
//
//			if (inst.BenefitType == EffectBenefitType.Buff && _buffList.Contains(inst))
//			{
//				return;
//			}
//			else if(inst.BenefitType == EffectBenefitType.Debuff && _debuffList.Contains(inst))
//			{
//				return;
//			}
//			_tempList.Add(inst);
//		}
//
//		public void CancelOtherAura(SkillEffectAuraInst inst, CombatLogNode logNode)
//		{
//			if (inst == null)
//			{
//				return;
//			}
////			if (inst.BenefitType == EffectBenefitType.Buff && _buffList.Contains(inst))
////			{
////				inst.DoOver(logNode);
////			}
////			else if(inst.BenefitType == EffectBenefitType.Debuff && _debuffList.Contains(inst))
////			{
////				inst.DoOver(logNode);
////			}
//		}

		public bool ApplyMez(MezType mezType)
		{
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.Mez, _cachedPassiveList);

			foreach (var passiveComp in _cachedPassiveList)
			{
				var thisPassive = passiveComp as MezPassiveComp;
				if (thisPassive == null)
				{
					continue;
				}

				var checkImmune = (mezType == MezType.All || mezType == thisPassive.MezType);

				if (checkImmune)
				{
					return true;
				}
			}

			return false;
		}

		public bool ApplyBarrier()
		{
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.BarrierDamage, _cachedPassiveList);

			var ret = false;
			
			foreach (var passiveComp in _cachedPassiveList)
			{
				if (passiveComp is BarrierPassiveComp barrierPassiveComp)
				{
					ret = true;
					break;
				}
			}

			return ret;
		}
		
		/// <summary>
		/// Hit 체크 
		/// </summary>
		/// <param name="owner"></param>
		/// <returns> true -> hit </returns>
		
        public bool ApplyHit(Unit caster, ActiveSkillCompLogNode logNode)
		{
			if (logNode.ForceHit)
			{
				// ignore hit
				return true;
			}
			
			var target = this;
			
            var hitRate = caster.Hit;
            var evasionRate = target.Evasion;

            //var statLvMod = 9.2931f * caster.Level * caster.Level - 296.62f * target.Level + 3614.7f;
            //var hitConst = 0.000033;

            //var hitProb = hitRate / (statLvMod + hitRate);
            //var hitProb = (float)((hitConst * hitRate) / (Math.Pow(caster.Level, 1.05)));
            var hitProb = UnitStatHelper.GetHitProb(caster.Level, hitRate);
            var hitFinalProb = caster.ApplyEnhance(EnhanceType.HitProb, hitProb, logNode);

            var evasionProb = UnitStatHelper.GetEvasionProb(target.Level, evasionRate);
            var evasionFinalProb = target.ApplyEnhance(EnhanceType.EvasionProb, evasionProb, logNode);
            
            var resultProb = 0.05f + hitFinalProb - evasionFinalProb;

            var calcHitProb = Dungeon.Random.Prob();

            bool result = true;
            
            if (resultProb < calcHitProb)
            {
	            result = false;
	            logNode.IsMiss = true;
            }
            
            logNode.AddDetailLog($"casterLevel:{caster.Level}, hitRate:{hitRate}, targetLevel:{target.Level},evasionRate:{evasionRate}");
            logNode.AddDetailLog($"hitFinalProb:{hitFinalProb}, evasionFinalProb:{evasionFinalProb}");
            logNode.AddDetailLog($"Hit:{result}, resultProb:{resultProb}, calcProb:{calcHitProb}");

            return result;
        }
		
		
		/// <summary>
		/// Immune 체크 / 미구현
		/// </summary>
		/// <param name="owner"></param>
		/// <returns> true -> immune </returns>
		public bool ApplyImmune(ActiveSkillComp skillComp, ActiveSkillCompLogNode logNode)
		{
			if (logNode.IgnoreImmune)
			{
                logNode.AddDetailLog($"Ignored Immune by Feature");
				return false;
			}
			
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.Immune, _cachedPassiveList);

			foreach (var passiveComp in _cachedPassiveList)
			{
				var thisPassive = passiveComp as ImmunePassiveComp;
				if (thisPassive == null)
				{
					continue;
				}

				var checkImmune = thisPassive.CheckImmune(skillComp);

				if (checkImmune)
				{
					logNode.IsImmune = true;
					return true;
				}
			}
			
			return false;
		}
		
		public bool ApplyImmune(SkillEffectInst skillInst, AddContinuousLogNode logNode)
		{
			// check cur skill
			var curSkill = _curAction;
			if (curSkill != null && curSkill.IsImmune)
			{
	            var passiveList = new List<PassiveSkillComp>();

	            if (skillInst.GetPassiveComp(PassiveSkillCompType.Mez, passiveList))
	            {
		            logNode.IsImmune = true;
		            return true;
	            }
			}
			
			// check buff/debuff
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.Immune, _cachedPassiveList);

			foreach (var passiveComp in _cachedPassiveList)
			{
				var thisPassive = passiveComp as ImmunePassiveComp;
				if (thisPassive == null)
				{
					continue;
				}

				var checkImmune = thisPassive.CheckImmune(skillInst);

				if (checkImmune)
				{
					logNode.IsImmune = true;
					return true;
				}
			}
			
			return false;
		}

		public bool ApplyImmune(ImmuneType immuneType)
		{
			// check cur skill
			var curSkill = GetCurActiveSkill();
			if (curSkill != null && curSkill.IsImmune)
			{
				if (immuneType == ImmuneType.Mez || immuneType == ImmuneType.Interrupt)
				{
					return true;
				}
			}
			
			// check buff/debuff
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.Immune, _cachedPassiveList);

			foreach (var passiveComp in _cachedPassiveList)
			{
				var thisPassive = passiveComp as ImmunePassiveComp;
				if (thisPassive == null)
				{
					continue;
				}

				var checkImmune = thisPassive.CheckImmune(immuneType);

				if (checkImmune)
				{
					return true;
				}
			}

			return false;

		}

		public long ApplyDefence(Unit caster, DamageSkillCompLogNode dmgNode)
		{
			var defence = Defence;

			if (caster != null && dmgNode.IgnoreEnhance == false)
			{
				defence = (int)caster.ApplyEnhance(new EnhanceType[]{EnhanceType.DefencePanetration}, (float)defence, dmgNode);
			}

			if (dmgNode.IgnoreDefence)
			{
				defence = (long)(defence * dmgNode.IgnoreDefenceFactor);
			}

			return defence;
		}
		
		public bool ApplyCritical(Unit target, DamageSkillCompLogNode dmgNode)
		{
			if (dmgNode == null || dmgNode.IgnoreCritical == true)
			{
                dmgNode.AddDetailLog($"Ignored Critical by Feature");
				return false;
			}
			

            var isCritical = false;

            if (dmgNode.ForceCritical == false)
            {
                var caster = this;
                var critRate = this.CritRate;
                var resilienceRate = target.Resilience;

                var levelDiff = (int)caster.Level - target.Level;
                if (caster.Level == 0 || target.Level == 0)
                {
	                levelDiff = 0;
                }

                var statLvMod = (float)levelDiff * levelDiff / 200;
                if (caster.Level < target.Level)
                {
	                statLvMod *= -1;
                }

                var critProb = UnitStatHelper.GetCritProb(caster.Level, critRate);
                
                if (dmgNode.IgnoreEnhance == false)
                {
                    critProb = this.ApplyEnhance(EnhanceType.CritProb, critProb, dmgNode);
                }
                
                var resilienceProb = UnitStatHelper.GetResilienceProb(target.Level, resilienceRate);

                if (dmgNode.IgnoreEnhance == false)
                {
                    resilienceProb = target.ApplyEnhance(EnhanceType.ResilienceProb, resilienceProb, dmgNode);
                }
                
                var resultProb = critProb - resilienceProb + statLvMod;

                var calcProb = Dungeon.Random.Prob();
                
                dmgNode.AddDetailLog($"casterLevel:{caster.Level}, critRate:{critRate}, targetLevel:{target.Level},resilienceRate:{resilienceRate}");
                dmgNode.AddDetailLog($"lvlMode:{statLvMod}, critFinalProb:{critProb}, resilienceFinalProb:{resilienceProb}");

                if (resultProb >= calcProb)
                {
	                isCritical = true;
                    dmgNode.AddDetailLog($"Crit:true, resultProb:{resultProb}, calcProb:{calcProb}");
                }
                else
                {
                    dmgNode.AddDetailLog($"Crit:false, resultProb:{resultProb}, calcProb:{calcProb}");
                }
            }
            else
            {
                dmgNode.AddDetailLog($"Force Critical");
                isCritical = true;
            }
	            
	            
            
			if (isCritical)
			{
				float newDamage = 0f;

				var critDamage = this.CritDmg;
				
				var critDmgPercent = UnitStatHelper.GetCritDmgPercent(this.Level, critDamage);
				
				if (dmgNode.IgnoreEnhance == false)
				{
                    critDmgPercent = this.ApplyEnhance(EnhanceType.CritDamage, critDmgPercent, dmgNode);
				}
				
				if (dmgNode.IgnoreEnhance == false)
				{
                    critDmgPercent = target.ApplyEnhance(EnhanceType.IncomingCritDamage, critDmgPercent, dmgNode);
				}

				dmgNode.AddDetailLog($"Crit Damage : {critDamage}");

				newDamage = dmgNode.Damage * (1.0f + critDmgPercent);

                dmgNode.AddDetailLog($"Damage:{dmgNode.Damage}, FinalDamage:{newDamage}");
                
				dmgNode.Damage = newDamage;
				dmgNode.IsCritical = true;
				
                //CorgiLog.LogLine("Critical True / new Damage : " + this.CritRate + " + " + dmgNode.ElementalCritRate + " / " +  dmgNode.Damage);
			}
			else
			{
                //CorgiLog.LogLine("Critical false / old Damage : " + this.CritRate + " + " + dmgNode.ElementalCritRate + " / " + dmgNode.Damage);
			}
			

			return true;
		}
		
		public bool ApplyCritical(HealSkillCompLogNode healNode)
		{
			if (healNode == null || healNode.IgnoreCritical == true)
			{
                healNode.AddDetailLog($"Ignored Critical by Feature");
				return false;
			}

			var isCritical = false;

			if (healNode.ForceCritical == false)
			{
                var caster = this;
                var critRate = (float) this.CritRate;
                
                var statLvMod = 9.2931f * caster.Level * caster.Level + 3614.7f;

                var critProb = critRate / (statLvMod + critRate) + 0.05f;
                var critFinalProb = this.ApplyEnhance(EnhanceType.CritProb, critProb, healNode);
                
                var resultProb = critFinalProb;

                var calcProb = Dungeon.Random.Prob();
                
                healNode.AddDetailLog($"casterLevel:{caster.Level}, critRate:{critRate}");
                healNode.AddDetailLog($"critFinalProb:{critFinalProb}");

                if (resultProb >= calcProb)
                {
	                isCritical = true;
                    healNode.AddDetailLog($"Crit:true, resultProb:{resultProb}, calcProb:{calcProb}");
                }
                else
                {
                    healNode.AddDetailLog($"Crit:false, resultProb:{resultProb}, calcProb:{calcProb}");
                }
			}
			else
			{
                healNode.AddDetailLog($"Force Critical");
				isCritical = true;
			}
			
            

			if (isCritical)
			{
				float newValue= 0f;

				var critDamage = this.CritDmg;
				
				var critDmgPercent = UnitStatHelper.GetCritDmgPercent(this.Level, critDamage);

				newValue = healNode.Heal * (1.0f + critDmgPercent);
				
                healNode.AddDetailLog($"CritDmg : {CritDmg}");
                healNode.AddDetailLog($"Heal:{healNode.Heal}, FinalHeal:{newValue}");

				healNode.Heal = newValue;
				healNode.IsCritical = true;
				
                //CorgiLog.LogLine("Critical True / new Damage : " + this.CritRate + " + " + dmgNode.ElementalCritRate + " / " +  dmgNode.Damage);
			}

			return true;
		}

		
		/// <summary>
		/// Apply damage by Advantage Type 
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="dmgNode"></param>
		/// <returns></returns>
//		public bool ApplyAdvantageType(Unit caster, DamageSkillCompLogNode dmgNode)
//		{
//			// todo: Calculate
//
//			return true;
//		}
		
		/// <summary>
		/// Apply Damage
		/// </summary>
		/// <param name="damage"> + Damage</param>
		/// <returns></returns>
        public long ApplyDamage(long damage)
        {
            if(damage == 0)
            {
                //CorgiLog.Assert(false);
	            // absorb all damage
                return damage;
            }

            if(_curHP <= 0)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid cur hp for damage");
                return damage;
            }

            var remainHp = _curHP - damage;
	       	//CorgiLog.LogWarning("ApplyDamage() " + this + " / " + _curHP + " - " + damage + " = " + remainHp);
            if(remainHp <= 0)
            {
                _curHP = 0;
            }else
            {
                _curHP = remainHp;
            }

            return damage;
        }

		/// <summary>
		/// Apply Heal amount
		/// </summary>
		/// <param name="heal"></param>
		/// <returns></returns>
        public long ApplyHeal(long heal)
        {
            if(heal <= 0)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid cur hp for heal");
                return heal;
            }

	        if(_curHP <= 0)
	        {
		        return _curHP;
	        }

	        _curHP += heal;

	        if(_curHP > MaxHP)
	        {
		        // overheal � �외 ��
		        var newDiff = heal - (_curHP - MaxHP);
		        _curHP = MaxHP;

		        return newDiff;
	        }

	        return heal; 
        }

		public long ConsumeHP(float hpRate, DamageSkillCompLogNode logNode)
		{
			if (hpRate <= 0f)
			{
				return 0;
			}
			
			if (_curHP <= 0)
			{
				return 0;
			}


			var consumeHP = (long) (_curHP * hpRate);
			
            var remainHp = _curHP - consumeHP;
			
            if(remainHp <= 0)
            {
	            consumeHP = _curHP - 1;
                _curHP = 1;
            }else
            {
                _curHP = remainHp;
            }

			logNode.Damage = consumeHP;

            return consumeHP;
		}

		public long ResetHP(long hp = -1)
		{
			_curHP = hp;

			if (hp < 0 || hp > MaxHP)
			{
				_curHP = MaxHP;
			}

			return _curHP;
		}

		public int ApplyMana(int mana)
		{
            if(mana == 0)
            {
                //CorgiLog.Assert(false);
	            // absorb all damage
                return 0;
            }

            var curSkill = GetCurActiveSkill();
            if (curSkill == null)
            {
	            return 0;
            }
            
            var curMana = _curMana;
            
            if (curMana + mana > _maxMana)
            {
	            _curMana = _maxMana;
            }else if (curMana + mana < 0)
            {
	            _curMana = 0;
            }
            else
            {
	            _curMana = curMana + mana;
            }
            var diff = _curMana - curMana;


            CorgiCombatLog.Log(CombatLogCategory.Unit, "ApplyMana() {0}/{1}/{2}", _curMana, mana, diff);

            return diff;
		}
		public long ResetMana(int resetMana = 0)
		{
			_curMana = resetMana;

			return _curMana;
		}
		
        public bool IsNearDeath(float percent)
        {
			if (((float)_curHP / (float)MaxHP) <= percent)
            {
                return true;
            }

            return false;
        }

        public int IsDamageHPPercent(float preHPRate, float percent)
        {
	        var preHPCount = (int)((1.0f - preHPRate) / percent) + 1;
	        var curHPCount = (int)((1.0f - (float)_curHP / MaxHP) / percent) + 1;

	        var diff = curHPCount - preHPCount;

	        return diff > 0 ? diff : 0;
        }

        /// <summary>
        /// Apply Enhance Passive
        /// </summary>
        /// <param name="enhanceType"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// 
		
		/// <summary>
		/// Apply Enhance Passive
		/// </summary>
		/// <param name="enhanceType"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public void ApplyEnhance(EnhanceType[] enhanceTypes, ActiveSkillCompLogNode logNode)
		{
			var resultValue = 0.0f;
			var resultPercentPlusValue = 1f;
			var resultPercentMinusValue = 1f;

//            if(this.CheckDisableEnhance(enhanceType, output) == true)
//            {
//                output.SetZero();
//                return 0.0f;
//            }

			var skillLogNode = logNode as SkillCompLogNode;

			foreach (var enhanceType in enhanceTypes)
			{
				if (skillLogNode != null)
				{
					skillLogNode.GetEnhanceValue(enhanceType, ref resultValue, ref resultPercentPlusValue, ref resultPercentMinusValue);
				}
				
                this.GetEnhanceValue(enhanceType, ref resultValue, ref resultPercentPlusValue, ref resultPercentMinusValue, logNode);
			}
			
			logNode.ApplyEnhance(resultValue, resultPercentPlusValue, resultPercentMinusValue);
		}

		public void ApplyEnhance(EnhanceType enhanceType, ActiveSkillCompLogNode logNode)
		{
			ApplyEnhance(new EnhanceType[]{enhanceType}, logNode);
		}
		
        public float ApplyEnhance(EnhanceType enhanceType, float applyValue, CombatLogNode logNode)
        {
	        if (logNode is SkillCompLogNode)
	        {
		        return ApplyEnhance(enhanceType, applyValue, logNode as SkillCompLogNode);
	        }else if (logNode is SkillActionLogNode)
	        {
		        return ApplyEnhance(enhanceType, applyValue, logNode as SkillActionLogNode);
	        }

	        // default
	        return applyValue;
        }
		
		public float ApplyEnhance(EnhanceType[] enhanceTypes, float applyValue, SkillActionLogNode logNode)
		{
			float absValue = 0;
			float percentPlusValue = 1.0f;
			float percentMinusValue = 1.0f;
			
			float preAbsValue = 0.0f;
			float prePercentPlusValue = 1.0f;
			float prePercentMinusValue = 1.0f;

			var retValue = applyValue;

			foreach (var enhanceType in enhanceTypes)
			{
                this.GetEnhanceValue(enhanceType, ref absValue, ref percentPlusValue, ref percentMinusValue, logNode);

			}
		    retValue = retValue * prePercentPlusValue * prePercentMinusValue + preAbsValue;
			retValue = retValue * percentPlusValue * percentMinusValue + absValue;

			return retValue;
		}
		
		// apply enhance for 
		public float ApplyEnhance(EnhanceType enhanceType, float applyValue, SkillActionLogNode logNode)
		{
			return ApplyEnhance(new EnhanceType[]{enhanceType}, applyValue, logNode);
		}
		
		public float ApplyEnhance(EnhanceType[] enhanceTypes, float applyValue, SkillCompLogNode logNode)
		{
			float absValue = 0;
			float percentPlusValue = 1.0f;
			float percentMinusValue = 1.0f;
			
			float preAbsValue = 0.0f;
			float prePercentPlusValue = 1.0f;
			float prePercentMinusValue = 1.0f;

			var retValue = applyValue;

//            if(this.CheckDisableEnhance(enhanceType, output) == true)
//            {
//                output.SetZero();
//                return 0.0f;
//            }

			foreach (var enhanceType in enhanceTypes)
			{
				if (logNode != null)
				{
					logNode.GetEnhanceValue(enhanceType, ref preAbsValue, ref prePercentPlusValue, ref prePercentMinusValue);
				}
				
                this.GetEnhanceValue(enhanceType, ref absValue, ref percentPlusValue, ref percentMinusValue, logNode);

			}
		    retValue = retValue * prePercentPlusValue * prePercentMinusValue + preAbsValue;
			retValue = retValue * percentPlusValue * percentMinusValue + absValue;

			return retValue;
		}
		
		// apply enhance for 
		public float ApplyEnhance(EnhanceType enhanceType, float applyValue, SkillCompLogNode logNode)
		{
			return ApplyEnhance(new EnhanceType[]{enhanceType}, applyValue, logNode);
		}
		
		/// <summary>
		/// Get Enhance Value
		///  - add unit variable for return value (Delegate)
		/// </summary>
		/// <param name="enhanceType"></param>
		/// <param name="logNode"></param>
		/// <param name="absValue"></param>
		/// <param name="percentValue"></param>
		/// <returns></returns>
		/// 
        float retAbs = 0;

        float retPercentPlus = 0f;
        float retPercentMinus = 1.0f;
        
		public bool GetEnhanceValue
			(EnhanceType enhanceType, ref float absValue, ref float plusValue, ref float minusValue, SkillActionLogNode logNode)
		{
            retAbs = 0;
            retPercentPlus = 0f;
            retPercentMinus = 1.0f;
			
			DoSkillEffectAction((inst) =>
			{
                if (inst != null)
                {
                    inst.GetEnhanceValue(enhanceType, ref retAbs, ref retPercentPlus, ref retPercentMinus, logNode);
                }
			});

			absValue += retAbs;
			plusValue += retPercentPlus;
			minusValue *= retPercentMinus;
			

			//CorgiLog.LogLine("enhanceType : " + enhanceType + ", abs : " + retAbs + ", percent : " + retPercent);
			logNode.AddDetailLog($"ApplyEnhance {enhanceType} : abs:{absValue}, plus:{plusValue}, minus:{minusValue}");

			return true;
		}
		
		public bool GetEnhanceValue
			(EnhanceType enhanceType, ref float absValue, ref float plusValue, ref float minusValue, SkillCompLogNode logNode)
		{
            retAbs = 0;
            retPercentPlus = 0f;
            retPercentMinus = 1.0f;
			
			DoSkillEffectAction((inst) =>
			{
                if (inst != null)
                {
                    inst.GetEnhanceValue(enhanceType, ref retAbs, ref retPercentPlus, ref retPercentMinus, logNode);
                }
			});

			absValue += retAbs;
			plusValue += retPercentPlus;
			minusValue *= retPercentMinus;
			

			//CorgiLog.LogLine("enhanceType : " + enhanceType + ", abs : " + retAbs + ", percent : " + retPercent);
			logNode.AddDetailLog($"ApplyEnhance {enhanceType} : abs:{absValue}, plus:{plusValue}, minus:{minusValue}");

			return true;
		}
		
		void AddTarget(ActivateTargetState targetState, Unit unit, ref List<Unit> targetList)
		{
			if (targetState == ActivateTargetState.Alive && unit.IsLive())
			{
				targetList.Add(unit);
			}else if (targetState == ActivateTargetState.Dead && unit.IsLive() == false)
			{
				targetList.Add(unit);
			}
		}

		public List<Unit> GetAvailableTarget(string skillId)
		{
			if (_dungeon == null) return null;
			
			var skill = GetSkill(skillId) as SkillActive;
			if(skill == null)
			{
				return null;
			}
			
			var targetList = new List<Unit>();

			return targetList;
		}
		
		/// <summary>
		/// Update Stat
		/// </summary>
		public void OnUpdateStat(CombatLogNode logNode)
		{
            var enumerator = _statMap.GetEnumerator();
			
			_cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None, PassiveSkillCompType.Stat, _cachedPassiveList);

			// 사라지는 경우 처리 bug fix.
			// if (_cachedPassiveList.Count <= 0)
			// {
			// 	return;
			// }
			
            while (enumerator.MoveNext())
            {
                var unitStat = enumerator.Current.Value;
	            
	            var prevStatValue = unitStat.Stat;
	            var statType = unitStat.StatType;

                unitStat.OnUpdateStat(_cachedPassiveList, logNode);
                
	            if (statType == StatType.StMaxHp)
	            {
		            var diff = unitStat.Stat - prevStatValue;

		            // if diff > 0, should apply cur hp 
		            if (diff > 0)
		            {
			            ApplyHeal(diff);
		            }

		            if (diff < 0 && CurHP > MaxHP)
		            {
			            ResetHP(MaxHP);
		            }
	            }
	            else if (statType == StatType.StAttackSpeed)
	            {
		            var diff = unitStat.Stat - prevStatValue;
		            if (diff != 0)
		            {
						Attack?.ResetSkillActionTime(diff);
		            }
	            }
            }                        
		}

		public void IsUpdatedEffect()
		{
			_isUpdatedEffect = true;
		}
		
		public void OnUpdateEffect(CombatLogNode logNode)
		{
			if (_isUpdatedEffect == false)
			{
				// flag check.
				return;
			}

			int maxCount = 10;
			int curCount = 0;
			
			// apply templist
			while (_isUpdatedEffect && curCount++ < maxCount)
			{
				_isUpdatedEffect = false;
				
				if (_tempList.Count > 0)
				{
					var tempList = new List<SkillEffectInst>();
					tempList.AddRange(_tempList);
					_tempList.Clear();
					
					foreach (var inst in tempList)
					{
						 if (inst == null)
						 {
							  continue;
							  
						 }
						 var eventParam = new EventParamEffect(inst);
						 
						 inst.OnEvent(CombatEventType.OnSkillEffectEnter, eventParam, logNode);
						 
						 if (inst != null && inst.IsMaxStack)
						 {
							   inst.OnEvent(CombatEventType.OnSkillEffectMaxStack, eventParam, logNode);
						 }
						 
						 if (inst.BenefitType == ContinuousBenefitType.Buff)
						 {
							   OnEvent(CombatEventType.OnSkillEffectBuff, eventParam, logNode);
						 }

						 if (inst.BenefitType == ContinuousBenefitType.Debuff)
						 {
							   OnEvent(CombatEventType.OnSkillEffectDebuff, eventParam, logNode);
						 }
						 
						 _continuousList.Add(inst);
					}
				}

				foreach (var inst in _continuousList)
				{
					if (inst == null)
					{
						continue;
					}
	                // buff & debuff alive check.            
	                if (inst.IsLive == false)
	                {
		                var eventParam = new EventParamEffect(inst);
		                
	                    if (inst.IsDurationOver == true)
	                    {
	                        inst.OnEvent(CombatEventType.OnSkillEffectDurationOver, eventParam, logNode);
	                    }
	                    else
	                    {
	                        inst.OnEvent(CombatEventType.OnSkillEffectBreak, eventParam, logNode);
	                    }
	                    inst.OnEvent(CombatEventType.OnSkillEffectExit, eventParam, logNode);
	                }

	                if (inst.IsLive)
	                {
		                inst.OnUpdateStack(logNode);
	                }
				}
				
				// remove dead buff/debuff
				for(int i=_continuousList.Count - 1; i>=0; i--)
				{
					SkillEffectInst inst = _continuousList[i];
					if(inst != null && inst.IsLive == false)
					{
						var curLogNode = new RemoveContinuousLogNode(inst);
						_continuousList.RemoveAt(i);
						logNode.AddChild(curLogNode);
					}
				}

				OnUpdateStat(logNode);
				OnUpdateBarrier(logNode);

				OnUpdateMez(logNode); // check something.
			}

			
			
#if UNITY_EDITOR
			//UnityEngine.Profiler.EndSample();
#endif
		}

		public void OnUpdateMez(CombatLogNode logNode)
		{
			if (!IsLive())
				return;

			var prevMezState = MezState;
			
            _cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None , PassiveSkillCompType.Mez, _cachedPassiveList);
            
			//CorgiLog.LogLine("passive check count : " + passiveList.Count);

            int passiveListCount = _cachedPassiveList.Count;
            _mezState = MezType.None;
			
            for (int i = 0; i < passiveListCount; i++)            
			{                
                MezPassiveComp comp = _cachedPassiveList[i] as MezPassiveComp;
				if(comp == null)
				{
					CorgiLog.Assert(false);
					continue;
				}
				
				if (_mezState < comp.MezType)
				{
                    _mezState = comp.MezType;
				}
			}

            if (prevMezState == MezType.None && _mezState != MezType.None)
            {
	    //         if (_curAction != null && _curAction.OnCancelCasting(logNode))
	    //         {
					// OnSkill(_curAction, logNode);
					//
					// _curAction = null;
	    //         }

	            OnEvent(CombatEventType.OnMez, new EventParamUnit(this), logNode);
	            
	            _unitFSM.Trigger(UnitTrigger.OnMez, logNode);
            }

            if (prevMezState != MezType.None && _mezState == MezType.None)
            {
	            _unitFSM.Trigger(UnitTrigger.OnMezEnd, logNode);
            }
		}

		public void OnUpdateBarrier(CombatLogNode logNode)
		{
			if (!IsLive())
				return;

			var prevMezState = MezState;
			
            _cachedPassiveList.Clear();
			GetPassiveComp(ContinuousBenefitType.None , PassiveSkillCompType.BarrierDamage, _cachedPassiveList);
            
			//CorgiLog.LogLine("passive check count : " + passiveList.Count);

			_curBarrier = 0;
            int passiveListCount = _cachedPassiveList.Count;
			
            for (int i = 0; i < passiveListCount; i++)            
			{                
                var comp = _cachedPassiveList[i] as BarrierPassiveComp;
				if(comp == null)
				{
					CorgiLog.Assert(false);
					continue;
				}

				_curBarrier += comp.GetBarrierAmount();
			}
		}
		
		protected virtual bool OnDeadCompletely(EventParam eventParam, CombatLogNode logNode)
		{
//#if UNITY_EDITOR || UNITY_STANDALONE_WIN
//            if(IsTest && this.IsTestActiveDead == false && IsTestPlayMode == false)
//            {
//                _curHP = 1;
//            }
//#endif
			
			if(IsLive() == true)
			{
				//this.OnEvent(CombatEventType.OnDeadAlmost, new EventParamUnit(this), logNode);
				return false; // passive 같� 걸로 �아 �다.
			}
			else
			{
				//this.OnEvent(CombatEventType.OnDeadCompletely, new EventParamUnit(this), logNode);
			}
            
            _cachedPassiveList.Clear();

//            if (GetPassiveComp(SkillContinuousType.All, PassiveEffectType.Salvation, _cachedPassiveList) == false)
//            {
				//SetUnitState(true, UnitCombatState.Dead);
            //}

            foreach (var inst in _continuousList)
			{
				if (inst != null)
				{
					inst.DoOver(logNode);
				}
			}

            //_auraList.Clear();

			DieLogNode dieNode = new DieLogNode(this);
			logNode.AddChild(dieNode);

			_dungeon.OnDeadCompletely(this, dieNode);
			
			_unitFSM.Trigger(UnitTrigger.OnDead, dieNode);

			return true;
		}

		private void GetPassiveComp(ContinuousBenefitType continuousType, PassiveSkillCompType passiveType, List<PassiveSkillComp> list)
		{
			DoSkillEffectAction((inst) =>
			{
				if (continuousType == ContinuousBenefitType.None || continuousType == inst.BenefitType)
				{
					inst.GetPassiveComp(passiveType, list);
				}
			});
		}

//		private void SetUnitState(bool set, UnitCombatState state)
//		{
//			if (set)
//			{
//				_unitState |= state;
//			}
//			else
//			{
//				_unitState &= ~state;
//			}
//		}

//		public bool CheckState(UnitCombatState state)
//		{
//			if (state != UnitCombatState.Ready)
//				return (int)(_unitState & state) != 0;
//			
//			return _unitState == UnitCombatState.Ready;
//		}
	}
}
