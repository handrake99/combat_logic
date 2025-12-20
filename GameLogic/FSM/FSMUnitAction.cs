using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class FSMUnitAction : FSMUnitState
    {
	    public FSMUnitAction(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Action;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    if (Owner.Dungeon.State != DungeonState.InCombat)
		    {
			    return UnitState.Idle;
		    }
		    
		    // check target 
		    var target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (target == null)
		    {
			    // target이 안걸리면 이동한다.
			    return UnitState.Moving;
		    }
		    
		    var distance = Owner.Dungeon.GetDistance(Owner, target);
		    if (distance >= Owner.GetRange(target))
		    {
			    // target이 있는데 사거리가 안닿는경우 이동한다.
			    return UnitState.Moving;
		    }
		    
		    // check delay
		    if (IsDelay)
		    {
			    // 어떤 경우이든 Delay가 있는경우 이 상태를 유지한다. 최소 딜레이
			    return ThisState;
		    }

			// 현재 Action을 진행한다.
			var actionLogNode = Owner.DoAction(target);

			if (actionLogNode == null)
			{
				return ThisState;
			}
			
			logNode.AddChild(actionLogNode);

			if (Owner.IsCasting)
			{
				// 캐스팅 스킬이면 캐스팅 UnitState로 넘긴다.
				return UnitState.Casting;
			}else if (Owner.IsChanneling)
			{
				// 채널링 스킬이면 채널링 UnitState로 넘긴다.
				return UnitState.Channeling;
			}
			
			// 한번 Action후 최소 딜레이 세팅
			SetDelay(Owner.AttackSpeed);
			
			return UnitState.Actioning;
	    }
	    
	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
		    base.OnEnter(prevState, dungeonLogNode);
		    
		    Owner.Dungeon.OnPauseEvent(
			    Owner, PauseCommandType.PauseCommand_UnitFSM, dungeonLogNode 
			    , (int)UnitState.Action, (int)UnitStateEvent.OnEnter, (int)SkillActionType.None);
		    
		    var target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (target == null)
		    {
			    return;
		    }
		    
		    
		    // 이전에 스킬 액션에서 넘어 왔을경우는 기본 딜레이를 준다.
		    //if (prevState == UnitState.Actioning || prevState == UnitState.Casting || prevState == UnitState.Channeling)
		    if (prevState == UnitState.Casting || prevState == UnitState.Channeling)
		    {
			    SetDelay(Owner.AttackSpeed);
		    }
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnFinishCombat)
		    {
			    if (Owner.CombatSideType == CombatSideType.Player)
			    {
				    return UnitState.Idle;
			    }
			    else
			    {
				    return UnitState.Destroyed;
			    }
		    }else if (trigger == UnitTrigger.OnDead)
		    {
			    return UnitState.Dead;
		    }
		    return base.OnTrigger(trigger, logNode);
	    }
    }
    
    public class FSMUnitActioning : FSMUnitState
    {
	    private UnitState _nextState;
	    
	    public FSMUnitActioning(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Actioning;
		    _nextState = UnitState.Action;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    var curSkill = Owner.CurAction;

		    if (curSkill == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Unit,"invalid unit state : no skill {0}", Owner.Name);
			    return _nextState;
		    }

		    if (curSkill.IsActioned)
		    {
			    return _nextState;
		    }

			return ThisState;
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnExploration)
		    {
			    _nextState = UnitState.Exploration;
			    return ThisState;
		    }

		    return base.OnTrigger(trigger, logNode);
	    }

	    public override void OnEnter(UnitState prevState, DungeonLogNode logNode)
	    {
		    base.OnEnter(prevState, logNode);

		    var skillActionType = SkillActionType.None;
		    if (Owner.CurAction != null)
		    {
			    skillActionType = Owner.CurAction.GetSkillActionType();
		    }
		    Owner.Dungeon.OnPauseEvent(
			    Owner, PauseCommandType.PauseCommand_UnitFSM, logNode, (int)UnitState.Actioning, (int)UnitStateEvent.OnEnter, (int)skillActionType);
	    }
	    
	    public override void OnExit(DungeonLogNode logNode)
	    {
		    base.OnExit(logNode);
		    
		    var skillActionType = SkillActionType.None;
		    if (Owner.CurAction != null)
		    {
			    skillActionType = Owner.CurAction.GetSkillActionType();
		    }
		    Owner.Dungeon.OnPauseEvent(
			    Owner, PauseCommandType.PauseCommand_UnitFSM, logNode , (int)UnitState.Actioning, (int)UnitStateEvent.OnExit, (int)skillActionType);
	    }
    }
    
    public class FSMUnitCasting : FSMUnitState
    {
	    public FSMUnitCasting(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Casting;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    if (Owner.Dungeon.State != DungeonState.InCombat)
		    {
			    return UnitState.Idle;
		    }

		    if (Owner.IsCasting )
		    {
                return ThisState;
		    }

		    return UnitState.Actioning;
	    }
	    
	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
		    base.OnEnter(prevState, dungeonLogNode);
		    
		    var target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (target == null)
		    {
			    return;
		    }
		    if (prevState == UnitState.Action)
		    {
			    SetDelay((ulong)Owner.AttackSpeed);
		    }
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnFinishCombat)
		    {
			    if (Owner.CombatSideType == CombatSideType.Player)
			    {
				    return UnitState.Idle;
			    }
			    else
			    {
				    return UnitState.Destroyed;
			    }
		    }else if (trigger == UnitTrigger.OnDead)
		    {
			    return UnitState.Dead;
		    }else if(trigger == UnitTrigger.OnMez)
		    {
			    Owner.OnCancelCasting(logNode);
			    
			    return base.OnTrigger(trigger, logNode);

		    }else if (trigger == UnitTrigger.OnCancelCasting)
		    {
			    return UnitState.Action;
		    }
		    return base.OnTrigger(trigger, logNode);
	    }
    }
    
    
    public class FSMUnitChanneling : FSMUnitState
    {
	    public FSMUnitChanneling(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Channeling;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    if (Owner.Dungeon.State != DungeonState.InCombat)
		    {
			    return UnitState.Idle;
		    }


		    if (Owner.IsChanneling == false)
		    {
                return UnitState.Action;
		    }

		    return ThisState;
	    }
	    
	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
		    base.OnEnter(prevState, dungeonLogNode);

		    var target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (target == null)
		    {
			    return;
		    }
		    if (prevState == UnitState.Action)
		    {
			    SetDelay(Owner.AttackSpeed);
		    }
		    
		    var skillActionType = SkillActionType.None;
		    if (Owner.CurAction != null)
		    {
			    skillActionType = Owner.CurAction.GetSkillActionType();
		    }
		    Owner.Dungeon.OnPauseEvent(
			    Owner, PauseCommandType.PauseCommand_UnitFSM, dungeonLogNode, (int)UnitState.Actioning, (int)UnitStateEvent.OnEnter, (int)skillActionType);
	    }
	    
	    public override void OnExit(DungeonLogNode logNode)
	    {
		    base.OnExit(logNode);
		    
		    var skillActionType = SkillActionType.None;
		    if (Owner.CurAction != null)
		    {
			    skillActionType = Owner.CurAction.GetSkillActionType();
		    }
		    Owner.Dungeon.OnPauseEvent(
			    Owner, PauseCommandType.PauseCommand_UnitFSM, logNode , (int)UnitState.Actioning, (int)UnitStateEvent.OnExit, (int)skillActionType);
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnFinishCombat)
		    {
			    if (Owner.CombatSideType == CombatSideType.Player)
			    {
				    return UnitState.Idle;
			    }
			    else
			    {
				    return UnitState.Destroyed;
			    }
		    }else if (trigger == UnitTrigger.OnDead)
		    {
			    return UnitState.Dead;
		    }else if (trigger == UnitTrigger.OnCancelCasting)
		    {
			    return UnitState.Action;
			    
		    }else if(trigger == UnitTrigger.OnMez)
		    {
			    Owner.OnCancelCasting(logNode);
			    
			    return base.OnTrigger(trigger, logNode);
			}
			    
		    return base.OnTrigger(trigger, logNode);
	    }
    }
}