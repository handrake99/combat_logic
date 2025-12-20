using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class FSMUnitMoving : FSMUnitState
    {
	    private Unit _target;

	    public FSMUnitMoving(Unit owner)
		    : base(owner)
	    {
		    ThisState = UnitState.Moving;
	    }
	    
	    public override UnitState Tick(ulong deltaTime, CombatLogNode logNode)
	    {
		    // Check NearestEnemy
		    _target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (_target == null)
		    {
			    return UnitState.Idle;
		    }
		    
		    // moviing to target
		    // if closed to target, finish state
		    Unit actor = Owner;
		    Unit target = _target;
		    Dungeon dungeon = actor.Dungeon;
		    
		    if (actor == null || target == null)
			    return ThisState;

		    CorgiPosition actorPos = actor.Position;
		    CorgiPosition targetPos = target.Position;

		    var distance = CorgiPosition.Distance(actorPos, targetPos);

		    if (distance <= actor.GetRange(target))
		    {
			    var curLogNode = new UnitMovingLogNode(actor, target);
			    logNode.AddChild(curLogNode);
			    return UnitState.Action;
		    }


		    var arrivalPos = CheckCollision(actor, target);

		    actorPos.MoveToPos(arrivalPos, ((actor.MoveSpeed * ((float)deltaTime/1000))));

		    actor.Position = actorPos;
		    actor.ArrivalPosition = arrivalPos;
		    
			var newLogNode = new UnitMovingLogNode(actor, target);
			logNode.AddChild(newLogNode);
			
            CorgiCombatLog.Log(CombatLogCategory.Unit, "[{4}] ({0}, {1}) / ({2}, {3})", actorPos.X, actorPos.Y, actorPos.DirX, actorPos.DirY, actor.Name);

			return ThisState;
	    }

	    CorgiPosition CheckCollision(Unit actor, Unit target)
	    {
		    var dungeon = actor.Dungeon;
		    
		    var actorPos = actor.Position.Pos;
		    var targetPos = target.Position.Pos;
		    var arrivalDistance = CorgiPosition.Distance(actorPos, targetPos) - actor.GetRange(target) + 0.1f;
		    
		    // arrival position 
		    var arrivalPos = CorgiPosition.GetArrivalPosition(actorPos, targetPos, arrivalDistance);

		    var friends = dungeon.GetFriends(actor);

		    if (friends == null)
		    {
			    return arrivalPos;
		    }

		    foreach (var curUnit in friends)
		    {
			    if (curUnit == null || curUnit.ObjectId == actor.ObjectId)
			    {
				    continue;
			    }

			    var curArrivalPos = curUnit.ArrivalPosition;
			    
			    // check closer than unit size
			    var diffDistance = CorgiPosition.Distance(curArrivalPos, arrivalPos);
			    if (diffDistance > curUnit.UnitSize + actor.UnitSize)
			    {
				    continue;
			    }
			    
			    var curDistance = CorgiPosition.Distance(curUnit.Position, curArrivalPos);
				// check if closer 
			    if (curDistance > arrivalDistance)
			    {
				    continue;
			    }
			    

			    var diffPos = arrivalPos - curArrivalPos;
			    diffPos.SetDistance(curUnit.UnitSize + actor.UnitSize);

			    arrivalPos = diffPos + curArrivalPos;
			    break;
		    }

		    return arrivalPos;
	    }
	    
	    public override void OnEnter(UnitState prevState, DungeonLogNode dungeonLogNode)
	    {
		    base.OnEnter(prevState, dungeonLogNode);
		    
		    _target = Owner.Dungeon.GetNearestEnemy(Owner);

		    if (_target == null)
		    {
			    return;
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
}