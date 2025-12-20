
using System;
using System.Collections.Generic;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class SkillTargetComp
    {
	    private Unit _owner;

	    private List<Unit> _targetList = new List<Unit>();

	    public Unit Owner
	    {
		    get { return _owner; }
	    }

	    public bool IsAreaEffect { get; protected set; }

	    public SkillTargetComp()
	    {
		    IsAreaEffect = false;
	    }

	    public bool Init(Unit owner)
	    {
		    _owner = owner; 
		    
		    return true;
	    }
	    
		public List<Unit> GetTargetList(CombatLogNode logNode)
		{
			var actorLogNode = logNode as SkillActorLogNode;

			if (actorLogNode == null)
			{
				return null;
			}
			
			return GetTargetList(actorLogNode);
		}

		public virtual List<Unit> GetTargetList(SkillActorLogNode logNode)
		{
			return null;
		}
	    
	    protected List<Unit> GetAndResetTargetList()
	    {
		    List<Unit> retList = _targetList;

		    _targetList = new List<Unit>(); //reset

		    return retList;
	    }

	    protected void AddTarget(string targetId)
	    {
		    var target = _owner.Dungeon.GetUnit(targetId);
		    AddTarget(target);
	    }

	    protected void AddTarget(Unit target)
	    {
		    if (target == null)
		    {
			    return;
		    }
		    _targetList.Add(target);
		    
	    }
		

		/*public virtual bool GetTargetsList(ISkillComp skillComp,  ActionInput input, List<IUnit> targetList)
		{
			return false;
		}

		protected void AddTarget(ISkillComp skillComp, IUnit unit, List<IUnit> targetList)
		{
			if(skillComp.TargetAliveType == TargetAliveType.Alive && unit.IsAvailable() == true)
			{
				targetList.Add(unit);
			}else if(skillComp.TargetAliveType == TargetAliveType.Dead && unit.IsAvailable() == false)
			{
				targetList.Add(unit);
			}else if(skillComp.TargetAliveType == TargetAliveType.All && !(unit.IsAlive() == false && unit.IsAvailable() == true))
            {
                targetList.Add(unit);
            }
		}*/

        public virtual void OnDestroy()
        {
        }
	}

}