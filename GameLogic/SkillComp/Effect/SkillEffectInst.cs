using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Utils;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	public class SkillEffectInst : CorgiCombatObject, ISkillActor, ICorgiInterface<SkillContinuousInfoSpec>
	{
		//Effect 를 건 Unit
		Unit 			_caster;
		
		/// <summary>
		/// Effect를 건 객체
		/// Skill or SkillEffectInst
		/// </summary>
		ISkillActor		_parentActor;
		public uint Level
		{
			get { return _parentActor.Level; }
		}

		public ulong SkillBaseUid
		{
			get { return _parentActor.SkillBaseUid; }
		}

		public ulong SkillUid
		{
			get { return _parentActor.SkillUid; }
		}
		public SkillType SkillType
		{
			get { return _parentActor.SkillType; }
		}
		
		public SkillActorType SkillActorType
		{
			get { return _parentActor.SkillActorType; }
		}
		public SkillAttributeType SkillAttributeType
		{
			get { return _parentActor.AttributeType; }
		}
		
		/// <summary>
		/// Owner skillComp
		/// </summary>
		ContinuousSkillComp _skillComp;
		Unit 			_target;
		uint  			_stackCount;  
		int  			_diffStackCount;  
		SkillContinuousState 	_state;

		//bool _isImmune = false;

		/// <summary>
		/// Static Data
		/// </summary>
		private uint _uid = 0;

		private SkillContinuousInfoSpec _spec;
		public SkillEffectInstType EffectInstType;
		
		// 활성화 시점의 Stat
		private readonly Dictionary<StatType, UnitStat> _statMap = new Dictionary<StatType, UnitStat>();
		
		//bool 			_unique = false;
		ContinuousBenefitType _benefitType = ContinuousBenefitType.Buff;
		private ContinuousGroupType _groupType = ContinuousGroupType.None;

		private bool _isDurationExist = true;
		ulong 			_durationMax = 0;
		private ulong   _durationRemain = 0;
		private ulong   _tickRemain = 0;

		public SkillContinuousInfoSpec GetSpec()
		{
			return _spec;
		}
		
		public string Name
		{
			get { return _spec.Name; }
		}
		
	    SkillActorType ISkillActor.SkillActorType
	    {
		    get { return SkillActorType.SkillContinuous; }
	    }
		
		public ContinuousBenefitType BenefitType
		{
			get { return _benefitType; }
		}

		public ContinuousGroupType GroupType
		{
			get { return _groupType; }
		}

		public bool Stackable
		{
			get { return _spec.Stackable; }
		}

//		public SkillEffectInstState State
//		{
//			get { return _state; }
//			set { _state = value; }
//		}

		public bool IsLive
		{
			get { return _state == SkillContinuousState.Alive && _stackCount > 0; }
		}

		public ContinuousSkillComp SkillComp
		{
			get { return _skillComp; }
		}

		public bool IsDurationOver
		{
			get { return _isDurationExist && _durationRemain <= 0; }
		}
		
		public ulong TickDuration
		{
			get { return _spec.Tick; }
		}

		public string IconName
		{
			get { return _spec.IconName; }
		}
		
		public uint MaxStack
		{
			get { return _spec.MaxStack; }
		}

		public bool IsMaxStack
		{
			get { return _stackCount == _spec.MaxStack; }
		}
		

		public uint InitStack
		{
			get { return _spec.InitStack; }
		}

		public uint StackSpend
		{
			get { return _spec.StackSpend; }
		}

		public bool IsVisible
		{
			get { return _spec.Visible; }
		}

		public bool IsDispel
		{
			get { return _spec.IsDispel; }
		}
		
		public bool IsUniqueInUnit
		{
			get { return _spec.Unique; }
		}
		
		/// <summary>
	    /// OnEvent Components
	    /// </summary>
		protected List<OnEventSkillCompInfo> OnEventList = new List<OnEventSkillCompInfo>();
	    
	    /// <summary>
	    /// Passive Components
	    /// </summary>
        protected List<PassiveSkillComp> PassiveList = new List<PassiveSkillComp>();
		
//		Dictionary<StatType, int> _statMap = new Dictionary<StatType, int>();
//		Dictionary<EnhanceType, int> _enhanceAbsoluteMap = new Dictionary<EnhanceType, int>();
//		Dictionary<EnhanceType, int> _enhancePercentMap = new Dictionary<EnhanceType, int>();

		//public ActionType ActionType { get { return ActionType.SkillInst; } }
		/// <summary>
		/// Caster : 버프 디버프를 발생 시킨 유닛
		/// Owner  : 버프 디버프를 최초 발생시킨 계기가 되는 유닛
		/// Target : 버프 디버프를 들고 있는 유닛
		/// </summary>
		public Unit Caster { get { return _caster; }}
		public Unit Owner { get { return _caster; }}
		public Unit Target { get {return _target; }}
		
		// 버프/디버프를 발생시킨 Actor
		public ISkillActor ParentActor { get {return _parentActor; }}
		
		// stat
		public long MaxHP { get { return GetStat(StatType.StMaxHp); } }
		public long AttackSpeed { get { return GetStat(StatType.StAttackSpeed); } }
		public long AttackPower { get { return GetStat(StatType.StAttackPower); } }
		public long Defence { get {return GetStat(StatType.StDefence); }}
		public long Hit { get {return GetStat(StatType.StHit); }}
		public long Evasion { get {return GetStat(StatType.StEvasion); }}
		public long Crit{ get {return GetStat(StatType.StCrit); }}
		public long Resilience { get {return GetStat(StatType.StResilience); }}
		public long CritDmg { get {return GetStat(StatType.StCritDmg); }}

		//public List<PassiveSkillComp> PassiveList { get { return _onPassiveList; } }


//		private ContinuousLogNode _cachedLogNode;
//
//		protected SkillEffectInstLogNode CachedLogNode
//		{
//			get { return _cachedLogNode; }
//			set { _cachedLogNode = value; }
//		}


		public ulong CurDuration
		{
			get { return _durationRemain; }
		}
		
		public ulong MaxDuration
		{
			get { return _durationMax; }
		}
		
		public uint StackCount
		{
			get
			{
				if(Stackable)
				{
					return _stackCount;
				}else
				{
					return 1;
				}
			}
		}

		public uint Mastery
		{
			get => _parentActor.Mastery;
		}
		public uint VisiblePriority => _spec?.VisiblePriority ?? 0U;
		
		public SkillAttributeType AttributeType { get; protected set; }

        public void ResetDurationOver()
        {
//			if(_spec.duration < 0)
//			{
//				return;
//			}
//            _duration = 0;
        }

		public void ResetDuration()
		{
			SetDuration(_spec.Duration);
		}

		public SkillEffectInst(Unit caster, ContinuousSkillComp skillComp, Unit target)
		{
			EventManager = new EventManager(CombatEventCategory.Effect);

			_caster = caster;
			
			_parentActor = skillComp.ParentActor;
			_skillComp = skillComp;
			_target = target;
			//_stackCount = 1;
			//_state = SkillInstState.Ready;
			
			_statMap.Add(StatType.StMaxHp, new UnitStat(StatType.StMaxHp));
			//_statMap.Add(StatType.AttackSpeed, new UnitStat(StatType.AttackSpeed));
			_statMap.Add(StatType.StAttackPower, new UnitStat(StatType.StAttackPower));
			_statMap.Add(StatType.StDefence, new UnitStat(StatType.StDefence));
			_statMap.Add(StatType.StHit, new UnitStat(StatType.StHit));
			_statMap.Add(StatType.StEvasion, new UnitStat(StatType.StEvasion));
			_statMap.Add(StatType.StCrit, new UnitStat(StatType.StCrit));
			_statMap.Add(StatType.StResilience, new UnitStat(StatType.StResilience));

		    _statMap[StatType.StMaxHp].OrigStat = caster.MaxHP;
		    //_statMap[StatType.AttackSpeed].OrigStat = caster.AttackSpeed;
		    _statMap[StatType.StAttackPower].OrigStat = caster.AttackPower;
		    _statMap[StatType.StDefence].OrigStat = caster.Defence;
		    _statMap[StatType.StHit].OrigStat = caster.Hit;
		    _statMap[StatType.StEvasion].OrigStat = caster.Evasion;
		    _statMap[StatType.StCrit].OrigStat = caster.CritRate;
		    _statMap[StatType.StResilience].OrigStat =  caster.Resilience;

		    AttributeType = _parentActor.AttributeType;
		}

		protected override bool LoadInternal(ulong uid)
		{
			var ret = base.LoadInternal(uid);
			if (ret == false)
			{
				return false;
			}

			var spec = Owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(uid);

			if (spec == null) { return false; }

			_spec = spec;
			
			_benefitType = CorgiEnum.ParseEnum<ContinuousBenefitType>(_spec.BenefitType);
			_groupType = CorgiEnum.ParseEnum<ContinuousGroupType>(_spec.GroupType);
			
			_state = SkillContinuousState.Alive;
			_stackCount = _spec.InitStack;
			
		    try
		    {
			    var onEvent = spec.OnEvent;
			    
				foreach (var curData in onEvent)
				{
					var eventTypeStr = curData.EventType;
					var targetStr = curData.TargetType;
					var skillCompUid = curData.SkillCompUid;
					var conUid = curData.ConditionCompUid;
					 
					var skillCompInfo = 
						SkillCompInfo.CreateOnEvent(Owner, this, eventTypeStr, targetStr, skillCompUid, conUid);
					 
					if (skillCompInfo != null)
					{
						OnEventList.Add(skillCompInfo);
					}
				}
				
			    var onPassive = spec.OnPassive;

			    foreach (var curData in onPassive)
			    {
					 var skillComp = SkillCompFactory.CreatePassive(curData, Owner, this);
					 
					 if (skillComp != null)
					 {
						 PassiveList.Add(skillComp);
					 }
			    }
		    }
		    catch (Exception e)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid onEvent {0}", spec.Name);
			    return false;
		    }
			
			return true;
		}

		public virtual void DoSkillEffectAction(Unit target, Action<SkillEffectInst> action)
		{
			throw new CorgiException("Should be Implement");
		}
		
		public void SetDuration(ulong duration)
		{
			_durationMax = duration;
			if (_durationMax > 0)
			{
				_durationRemain = duration;
				_tickRemain = TickDuration;
				if (_tickRemain <= 0)
				{
					_tickRemain = 1000;
				}
			}else if (_durationMax == 0)
			{
				_isDurationExist = false;
			}
		}

		public void SetDurationBigger(ulong newDuration)
		{
			if (newDuration > _durationRemain)
			{
				SetDuration(newDuration);
			}
		}

		public void ChangeDuration(long diffDuration)
		{
			if (_isDurationExist == false)
			{
				return;
			}
			
			var resultRemain = ((long)_durationRemain + diffDuration);
			if (resultRemain <= 0)
			{
				_durationRemain = 0;
			}
			else
			{
				_durationRemain = (ulong)resultRemain;
			}
		}

		public override bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
		{
			var thisLogNode = logNode as SkillEventLogNode;
			var shouldAddLog = false;
			
			var stackTrace = new CorgiStackTrace(Owner.Dungeon);

			if (stackTrace.IsValid() == false)
			{
				CorgiCombatLog.LogFatal(CombatLogCategory.DungeonStackOverFlow, "StackOverflow skilleffect {0}", Uid);
				stackTrace.Finish();
				return false;
			}

			if (thisLogNode == null)
			{
				// eventLogNode로 부터 올라온게 아닐때 : 
                thisLogNode = new SkillEventLogNode(eventType, eventParam, Target);
                thisLogNode.Parent = logNode;
                thisLogNode.AddDetailLog($"OnEvent : {eventType.ToString()}");
                shouldAddLog = true;
			}
				

			bool ret = false;
			
			foreach (var passive in PassiveList)
			{
				if (passive == null)
				{
					continue;
				}

				if (passive.OnEvent(eventType, eventParam, thisLogNode))
				{
					ret = true;
				}
			}

			foreach (var eventInfo in OnEventList)
			{
				if (eventInfo == null || eventType != eventInfo.EventType)
				{
					continue;
				}

				var skillCompInfo = eventInfo.SkillCompInfo;
				if (skillCompInfo == null)
				{
					continue;
				}

				if (skillCompInfo.Invoke(thisLogNode))
				{
					ret = true;
				}
			}

			if (EventManager.OnEvent(eventType, eventParam, logNode))
			{
				ret = true;
			}

			if (shouldAddLog && thisLogNode.ChildCount > 0)
			{
				logNode.AddChild(thisLogNode);
			}
			
			if(ret)
			{
				if (_stackCount >= StackSpend)
				{
					_stackCount -= StackSpend;
				}
				else
				{
					_stackCount = 0u;
				}
			}

			stackTrace.Finish();
			return ret;
		}

		public virtual void OnDoApply(SkillEffectInst inst, CombatLogNode logNode)
		{
			if (logNode == null || inst == null) { return; }
			
			foreach (var eventComp in OnEventList)
			{
				if (eventComp == null)
				{
					continue;
				}
				
				logNode.AddDetailLog($"SkillInst({Name}) OnEvent {eventComp.EventType}:{eventComp.SkillCompInfo.SkillCompType}");
				
			}

			foreach (var passiveComp in PassiveList)
			{
				if (passiveComp == null)
				{
					continue;
				}
				
				logNode.AddDetailLog($"SkillInst({Name}) Passive {passiveComp.PassiveType} abs {passiveComp.BaseAbsolute} per {passiveComp.BasePercent}");
				
			}
		}
		
		public virtual void OnDoOver(CombatLogNode logNode)
		{
		}

		protected override void Tick(ulong deltaTime, TickLogNode logNode)
		{
			// check tick
			if (_tickRemain > deltaTime)
			{
				_tickRemain -= deltaTime;
			}
			else
			{
				_tickRemain = TickDuration - (deltaTime - _tickRemain);
				
				OnEvent(CombatEventType.OnSkillEffectTick, new EventParamEffect(this), logNode);
			}
			
			// check duration
			if (_durationRemain > deltaTime)
			{
				_durationRemain -= deltaTime;
			}
			else
			{
				_durationRemain = 0;
			}

			if (IsDurationOver)
			{
				_state = SkillContinuousState.Dead;
			}
		}
		

		public long GetStat(StatType statType)
		{
			if(_statMap.ContainsKey(statType) == true)
			{
				return _statMap[statType].GetStat();
			}

			return 0;
		}

		// public override void OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
		// {
		// 	if (_cachedLogNode == null)
		// 	{
  //               _cachedLogNode = new SkillEffectInstLogNode(Owner, this, Target);
		// 	}
  //
		// 	//_cachedLogNode.Target = eventParam.GetTarget();
  //
		// 	_cachedLogNode.Parent = logNode;
		// 	
		// 	base.OnEvent(eventType, eventParam, _cachedLogNode);
  //
		// 	foreach (var onEventStruct in OnEventList)
		// 	{
		// 		if (eventType != onEventStruct.EventType)
		// 		{
		// 			continue;
		// 		}
		// 		
  //               var targetComp = onEventStruct.TargetComp;
  //               //var condition = onEventStruct.Condition;
  //               var skillComp = onEventStruct.SkillComp;
		// 		//FollowingSkillComp followingSkillComp = null; //onEventStruct.FollowingSkillComp;
		// 		
		// 		if (targetComp == null || skillComp == null)
		// 		{
		// 			CorgiLog.LogWarning("Invalid OnEventSkillComp Struct ");
		// 			continue;
		// 		}
  //
		// 		var targetList = targetComp.GetTargetList(_cachedLogNode);
  //
		// 		if (targetList == null)
		// 		{
		// 			CorgiLog.LogWarning("Invalid OnEventSkillComp & Target: " );
		// 			continue;
		// 		}
  //
		// 		skillComp.DoApply(targetList, _cachedLogNode);
		// 	}
  //
		// 	foreach (var onPassiveStruct in PassiveList)
		// 	{
  //               var targetComp = onPassiveStruct.TargetComp;
  //               //var condition = onPassiveStruct.Condition;
  //               var skillComp = onPassiveStruct.SkillComp;
		// 		
		// 		if (targetComp == null || skillComp == null)
		// 		{
		// 			CorgiLog.LogWarning("Invalid OnEventSkillComp Struct ");
		// 			continue;
		// 		}
  //
		// 		var targetList = targetComp.GetTargetList(_cachedLogNode);
  //
		// 		if (targetList == null)
		// 		{
		// 			CorgiLog.LogWarning("Invalid OnEventSkillComp & Target: " );
		// 			continue;
		// 		}
		// 		
		// 		//todo: Check Target & Condition
		// 		
		// 		skillComp.OnEvent(eventType, eventParam, _cachedLogNode);
		// 	}
  //
		// 	
		// 	// todo: refactoring - check recursive call
		// 	if (_cachedLogNode != null && _cachedLogNode.ChildCount > 0)
		// 	{
		// 		logNode.AddChild(_cachedLogNode);
		// 		_cachedLogNode = null;
		// 	}
		// }

		/// <summary>
		/// Remove Buff/Debuff 
		/// </summary>
		/// <param name="logNode"></param>
		public void DoOver(CombatLogNode logNode)
		{
			_state = SkillContinuousState.Dead;
			
            var newNode = new RemoveContinuousLogNode(this);
            logNode.AddChild(newNode);

			OnDoOver(logNode);
		}

//		public float ApplyEnhance(EnhanceType enhanceType, SkillOutputActive output)
//		{
//			return Caster.ApplyEnhance(enhanceType, output);
//		}

//		public bool IsDurationOver()
//		{
//			if((_spec.duration > 0 && Duration <= 0) )
//			{
//				return true;
//			}else
//			{
//				return false;
//			}
//		}

//		public bool IsLive()
//		{
//			if((_spec.duration > 0 && Duration <= 0) || (_stackCount <= 0) || (State == SkillInstState.Dead))
//			{
//				return false;
//			}else
//			{
//				return true;
//			}
//		}

//		public void OnEvent(CombatEventType eventType, IEventParam eventParam, CombatLogNode logNode)
//		{
//			if(eventType != CombatEventType.OnDeadCompletely && _target.IsAvailable() == false)
//			{
//				return;
//			}
//
//            if(_state == SkillInstState.Ready)
//            {
//                if(Target.UnitIndex != Caster.UnitIndex || eventType == CombatEventType.OnContinuousLive)
//                {
//                    _state = SkillInstState.Live;
//                }
//            }else if(_state == SkillInstState.Live && eventType == CombatEventType.OnContinuousLive) // 한번만 실행되도록.
//            {
//                return;
//            }
//
//            if(_state == SkillInstState.Ready && eventType != CombatEventType.OnSkillEffectEnter && eventType != CombatEventType.OnMaxStack)
//            {
//                return;
//            }
//
//			if(_eventManager.OnEvent(eventType, eventParam, logNode) == true && Stackable && eventType != CombatEventType.OnSkillEffectExit)
//			{
//				_stackCount -= _spec.stackSpend;
//				if(_stackCount < 0)
//				{
//					_stackCount = 0;
//				}
//			}
//		}

		public bool OnEnter(EventParam eventParam, CombatLogNode logNode) 
		{
			//if(System.Object.ReferenceEquals(Caster, Target) == false)
			// 자기 자신한테 거는 것 아니면 바로 Live로 활성화.
//			if(Caster.UnitIndex != Target.UnitIndex)
//			{
//				_state = SkillInstState.Live;
//			}
//
//            Caster.RegisterRef(this);
//			ContinuousSkillComp comp = _skillComp as ContinuousSkillComp;
//			if(comp != null)
//			{
//				comp.RegisterRef(this);
//			}
//                        
//            int passiveListCount = _onPassiveList.Count;
//            
//            for (int i = 0; i < passiveListCount; i++)
//            {
//                PassiveSkillComp skillComp = _onPassiveList[i];
//
//                skillComp.OnEnterPub(eventParam, logNode);
//            }            
//			
//			CorgiLog.LogLine("skill inst onEnter : " + this.Name);

			return false;
		}

		public bool OnExit(EventParam eventParam, CombatLogNode logNode)
		{
//			ContinuousSkillComp comp = _skillComp as ContinuousSkillComp;
//			if(comp != null)
//			{
//				comp.UnRegisterRef(this);
//			}
//            Caster.UnRegisterRef(this);
//                        
//            int passiveListCount = _onPassiveList.Count;
//            
//            for (int i = 0; i < passiveListCount; i++)
//            {
//                PassiveSkillComp skillComp = _onPassiveList[i];
//
//                skillComp.OnExitPub(eventParam, logNode);
//            }            

            return false;
		}

		
		public bool GetPassiveComp(PassiveSkillCompType passiveType, List<PassiveSkillComp> passiveList)
		{
            bool bRet = false;

			foreach (var skillComp in PassiveList)
			{
				if (skillComp == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid EffectSkillComp Struct");
					continue;
				}

				if (passiveType != skillComp.PassiveType)
				{
					continue;
				}

				passiveList.Add(skillComp);
				bRet = true;
			}
			
			
			return bRet;
		}
		
		public bool GetEnhanceValue(EnhanceType enhanceType
                                    , ref float absValue, ref float percentPlus, ref float percentMinus, SkillActionLogNode logNode)
		{
	        foreach(var passiveComp in PassiveList)
	        {
		        var curAbsValue = 0.0f;
                var curPercentPlus = 0.0f;
                var curPercentMinus = 1.0f;

		        if (passiveComp == null)
		        {
			        continue;
		        }

		        var count = 1;

		        // if (condition != null)
		        // {
			       //  count = condition.CheckActive(logNode);
		        // }

		        // if (count <= 0)
		        // {
			       //  continue;
		        // }
		        
		        
		        if (passiveComp.GetEnhanceValue(enhanceType, ref curAbsValue, ref curPercentPlus, ref curPercentMinus, logNode))
		        {
                    logNode.AddDetailLog($"Enhance Type:{enhanceType}name:{Name}, abs:{absValue}, plus:{curPercentPlus}, minus:{curPercentMinus}");
		        }
		        

		        for (var i = 0; i < count; i++)
		        {
			        absValue += curAbsValue;
                    percentPlus += curPercentPlus;
                    percentMinus *= curPercentMinus;
		        }
            }

			return true;
		}
		
		public bool GetEnhanceValue(EnhanceType enhanceType
                                    , ref float absValue, ref float percentPlus, ref float percentMinus, SkillCompLogNode logNode)
		{
	        foreach(var passiveComp in PassiveList)
	        {
		        var curAbsValue = 0.0f;
                var curPercentPlus = 0.0f;
                var curPercentMinus = 1.0f;

		        if (passiveComp == null)
		        {
			        continue;
		        }

		        var count = 1;

		        // if (condition != null)
		        // {
			       //  count = condition.CheckActive(logNode);
		        // }

		        // if (count <= 0)
		        // {
			       //  continue;
		        // }
		        
		        
		        if (passiveComp.GetEnhanceValue(enhanceType, ref curAbsValue, ref curPercentPlus, ref curPercentMinus, logNode))
		        {
                    logNode.AddDetailLog($"Enhance Type:{enhanceType}name:{Name}, abs:{absValue}, plus:{curPercentPlus}, minus:{curPercentMinus}");
		        }

		        for (var i = 0; i < count; i++)
		        {
			        absValue += curAbsValue;
                    percentPlus += curPercentPlus;
                    percentMinus *= curPercentMinus;
		        }
            }

			return true;
		}


		public bool GetAvailableTarget(Skill skill,  Unit caster, ref List<Unit> targetList)
		{
			var ret = false;
			
            foreach(var passiveComp in PassiveList)
            {
	            if (passiveComp == null)
	            {
		            continue;
	            }

                // if (condition != null && condition.CheckActive(null) > 0)
                // {
	               //  if (passiveComp.GetAvailableTarget(skill, caster, ref targetList))
	               //  {
		              //   ret = true;
	               //  }
                // }
            }

			return ret;
		}
		
		public bool GetForceRelation(ActiveSkillCompLogNode logNode)
		{
			if (logNode == null)
			{
				return false;
			}
			
//            foreach(var passiveCompStruct in PassiveList)
//            {
//		        var condition = passiveCompStruct.Condition;
//		        var passiveComp = passiveCompStruct.SkillComp as ForceElementalRelationPassiveComp;
//		        
//		        if (passiveComp == null)
//		        {
//			        continue;
//		        }
//
//		        var count = 1;
//
//		        if (condition != null)
//		        {
//			        count = condition.CheckActive(logNode);
//		        }
//
//		        if (count <= 0)
//		        {
//			        continue;
//		        }
//
//	            logNode.ForceElementalRelation = passiveComp.ForceRelation;
//
//	            return true;
//            }

			return false;
		}
		
		
		
		public void DoStack(int stackCount)
		{
			_diffStackCount = stackCount;
		}

		public void OnUpdateStack(CombatLogNode logNode)
		{
			if (_diffStackCount == 0)
			{
				return;
			}

			var diffStack = _diffStackCount;

			//_stackCount = (uint)((int)_stackCount + _stackCount);

			if(_stackCount + diffStack > MaxStack)
			{
				diffStack = (int)(MaxStack - _stackCount);
				_stackCount = MaxStack;
			}else if (_stackCount + _diffStackCount <= 0)
			{
				diffStack = (int)_stackCount * (-1);
				_stackCount = 0;
			}
			else
			{
				_stackCount = (uint)(_stackCount + _diffStackCount);
			}

			if (diffStack > 0 && _stackCount == MaxStack)
			{
				var eventParam = new EventParamEffect(this);
				OnEvent(CombatEventType.OnSkillEffectMaxStack, eventParam, logNode);
			}

			_diffStackCount = 0;
		}

		public uint DoOver(uint stackCount)
		{
			if (_stackCount > stackCount)
			{
				_stackCount -= stackCount;
				return stackCount;
			}
			else
			{
				var retCount = _stackCount;
				_stackCount = 0;
				_state = SkillContinuousState.Dead;
				return retCount;
			}
		}

		public SkillEffectInst Clone(Unit target, uint stackCount)
		{
			var skillComp = _skillComp;

			if(skillComp == null)
			{
				return null;
			}

			//skillComp.DoApply(target, null, logNode);
			
			var clonedInst = SkillEffectInstFactory.Create(skillComp, Owner, target);
                
			clonedInst._stackCount = stackCount;

			ResetDuration();

			return clonedInst;
		}
	}
}
