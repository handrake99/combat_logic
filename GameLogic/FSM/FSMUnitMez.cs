using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public abstract class FSMUnitMez : FSMUnitAction
    {
	    public MezType MezType { get; protected set; }
	    
	    public FSMUnitMez(Unit owner)
		    : base(owner)
	    {
		    //ThisState = UnitState.Mez;
	    }
	    
	    
	    public override UnitState OnTrigger(UnitTrigger trigger, CombatLogNode logNode)
	    {
		    if (trigger == UnitTrigger.OnMez)
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
				    case MezType.Silence:
					    return UnitState.MezSilence;
				    case MezType.Sleep:
					    return UnitState.MezSleep;
				    default:
					    return ThisState;
			    }
		    }else if (trigger == UnitTrigger.OnMezEnd)
		    {
			    var mezState = Owner.MezState;
			    if (mezState == MezType.None)
			    {
				    return UnitState.Action;
			    }
			    
		    }

		    return base.OnTrigger(trigger, logNode);

	    }
	    
	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
		    base.OnEnter(prevState, dungeonLogNode);
		    
		    var target = Owner;

		    if (target == null)
		    {
			    return;
		    }
		    if (prevState == UnitState.Action)
		    {
			    SetDelay((ulong)Owner.AttackSpeed);
		    }
	    }
    }
    
    public class FSMUnitMezStun : FSMUnitMez
    {
	    public FSMUnitMezStun(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.MezStun;
		    MezType = MezType.Stun;
	    }

	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
    }
    public class FSMUnitMezSilence : FSMUnitMez
    {
	    public FSMUnitMezSilence(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.MezSilence;
		    MezType = MezType.Silence;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return base.Tick(deltaTime, logNode);
	    }
    }
    public class FSMUnitMezSleep : FSMUnitMez
    {
	    public FSMUnitMezSleep(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.MezSleep;
		    MezType = MezType.Sleep;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
    }
    
    public class FSMUnitMezFortify : FSMUnitMez
    {
	    public FSMUnitMezFortify(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.MezFortify;
		    MezType = MezType.Fortify;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
    }
    
    public class FSMUnitMezExausted : FSMUnitMez
    {
	    public FSMUnitMezExausted(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.MezExausted;
		    MezType = MezType.Exausted;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    return ThisState;
	    }
    }
}