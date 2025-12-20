
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.GameLogic;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public abstract class FSMUnitState 
    {
	    private Unit _owner;
	    private UnitState _thisState;

	    private bool _isDelay = false;
	    private ulong _curDelay = 0;

	    public Unit Owner => _owner;

	    public UnitState ThisState
	    {
		    get { return _thisState;}
		    protected set { _thisState = value; }
	    }
	    
	    protected bool IsDelay
	    {
		    get { return _isDelay; }
	    }

	    protected void SetDelay(ulong newDelay)
	    {
		    if (_isDelay)
		    {
                _curDelay = newDelay;
			    return;
		    }

		    _curDelay = newDelay;
		    _isDelay = true;
	    }

	    protected void SetDelay(long newDelay)
	    {
		    SetDelay(((ulong)newDelay));
	    }

	    protected FSMUnitState(Unit unit)
	    {
		    _owner = unit;
	    }
	    
	    public virtual UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }

	    public UnitState OnTick(ulong deltaTime, CombatLogNode logNode)
	    {
		    if (_isDelay == false)
		    {
			    return ThisState;
		    }
		    if (_curDelay < deltaTime)
		    {
			    _isDelay = false;
			    return ThisState;
		    }
		    
		    _curDelay -= deltaTime;
		    
		    return ThisState;
	    }

	    public virtual UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    // Unit base Trigger
		    if (trigger == UnitTrigger.OnExploration)
		    {
			    return UnitState.Exploration;
		    }else if(trigger == UnitTrigger.OnDead)
		    {
			    return UnitState.Dead;
			    
		    }else if (trigger == UnitTrigger.OnMez)
		    {
			    var mezState = Owner.MezState;

			    switch (mezState)
			    {
				    case MezType.Stun:
					    return UnitState.MezStun;
				    case MezType.Fortify:
					    return UnitState.MezFortify;
					case MezType.Exausted:
					    return UnitState.MezExausted;
				    case MezType.Sleep:
					    return UnitState.MezSleep;
				    case MezType.Silence:
					    return UnitState.MezSilence;
				    default:
					    return _thisState;
			    }
		    }else if (trigger == UnitTrigger.OnFinishCombat)
		    {
			    if (Owner.CombatSideType == CombatSideType.Player)
			    {
				    return UnitState.Idle;
			    }
			    else
			    {
					return UnitState.Destroyed;
			    }
		    }
		    
		    return _thisState;
	    }

	    public virtual void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
	    }

	    public virtual void OnExit(DungeonLogNode dungeonLogNode)
	    {
	    }
    }

    public class FSMUnitNone : FSMUnitState
    {
	    public FSMUnitNone(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.None;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnEnterCombat)
		    {
				return UnitState.Idle;
		    }

		    return base.OnTrigger(trigger, logNode);
	    }
	    
    }
    public class FSMUnitIdle : FSMUnitState
    {
	    public FSMUnitIdle(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Idle;
	    }

	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
			Owner.ResetUnitStatus();
	    }

	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnExploration)
		    {
				return UnitState.Exploration;
		    }else if (trigger == UnitTrigger.OnEnterCombat)
		    {
			    return UnitState.Moving;
		    }

		    return base.OnTrigger(trigger, logNode);
	    }
    }
    
    public class FSMUnitExploration : FSMUnitState
    {
	    public FSMUnitExploration(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Exploration;
	    }

	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    // moviing to init position
		    // if closed to target, finish state
		    Dungeon dungeon = Owner.Dungeon;

			var actorPos = Owner.Position;
			var targetPos = Owner.InitialPosition;

			if (dungeon.DungeonType == DungeonType.Arena)
			{
				targetPos.SetPos(targetPos.X, 0);
			}

			actorPos.MoveToPos(targetPos, ((Owner.MoveSpeed * ((float)CorgiLogicConst.CombatFrame/1000))));
			if (Owner.CombatSideType == CombatSideType.Player)
			{
				actorPos.SetDir(0, 1); // character exploration
			}
			else
			{
				actorPos.SetDir(0, -1); // character exploration
			}
			
		    Owner.Position = actorPos;
		    Owner.ArrivalPosition = targetPos;

			var newLogNode = new UnitMovingLogNode(Owner, null);
			logNode.AddChild(newLogNode);
			
            CorgiCombatLog.Log(CombatLogCategory.Unit, "[{4}] ({0}, {1}) / ({2}, {3})", actorPos.X, actorPos.Y, actorPos.DirX, actorPos.DirY, Owner.Name);
		    return ThisState;
	    }
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnEnterCombat)
		    {
				return UnitState.Moving;
		    }

		    return base.OnTrigger(trigger, logNode);
	    }
    }
    
    
    public class FSMUnitDead : FSMUnitState
    {
	    public FSMUnitDead(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Dead;
		    
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
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
		    }

		    return base.OnTrigger(trigger, logNode);
	    }
    }
    
    public class FSMUnitDestroyed : FSMUnitState
    {
	    public FSMUnitDestroyed(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Destroyed;
		    
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
    }
}