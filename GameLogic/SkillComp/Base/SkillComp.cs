using System;
using System.Collections.Generic;
using System.Diagnostics;
using Corgi.GameData;
using IdleCs.Library;
using IdleCs.GameLog;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
	public abstract class SkillComp : CorgiCombatObject, ISkillActor
	{
		private Unit _owner;
		private ISkillActor _parentActor;
		private UnitAliveState _targetAliveState = UnitAliveState.Alive;
		
		// component 별 cool 
		private long _coolTime = 0;
		
		// target들에 각각 적용될지 하나로 적용될지 여부
		private bool _isEachTarget = true;
		
		// static 
	    public bool IgnoreEvent;
	    public bool IgnoreCritical;

		protected Unit Owner
		{
			get { return _owner; }
		}

		public ISkillActor ParentActor
		{
			get { return _parentActor; }
		}
		
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
	    
	    public SkillActorType SkillActorType
	    {
		    get { return _parentActor.SkillActorType; }
	    }

	    public SkillType SkillType
	    {
		    get { return _parentActor.SkillType; }
	    }

		public abstract string GetName();

		protected long Cooltime
		{
			set { _coolTime = value; }
		}

		protected bool IsEachTarget
		{
			set { _isEachTarget = value; }
		}

		protected UnitAliveState TargetAliveState
		{
			get { return _targetAliveState; }
			set { _targetAliveState = value; }
		}
		
		public SkillAttributeType AttributeType
		{
			get { return _parentActor.AttributeType; }
		}
		
		public uint StackCount
		{
			get { return 1; }
		}

		public uint Mastery
		{
			get => _parentActor.Mastery;
		}
		
		public SkillComp()
		{
			//EventManager = new EventManager(CombatEventTargetType.SkillComp);
		}

		protected override bool LoadInternal(ulong uid)
		{
			//return base.Load(uid);
			return true;
		}

		public virtual bool SetDefault(Unit owner, ISkillActor skillActor)
		{
			_owner = owner;
			_parentActor = skillActor;
			
			return true;
		}
		
	    protected override void Tick(ulong deltaTime, TickLogNode logNode)
	    {
		    if (_coolTime > 0)
		    {
			    _coolTime = _coolTime - (long)deltaTime;
		    }

		    if (_coolTime < 0)
		    {
			    _coolTime = 0;
		    }
	    }


		public bool IsCooltime() { return _coolTime > 0; }

		public abstract SkillCompLogNode CreateLogNode(Unit target);

		public bool DoApply(List<Unit> targetList, List<ConditionComp> conditions, CombatLogNode logNode)
		{
			if (++Owner.Dungeon.DoApplyCallCount > 10000)
			{
				throw new CorgiException("Too Many CallCount[{0}] in Dungeon[{1}] / CharacterId[{2}] / Uid[{3}] / ParentUid[{4}]", 
					Owner.Dungeon.DoApplyCallCount, Owner.Dungeon.DungeonType, Owner.DBId, Uid, ParentActor.SkillUid);
			}

			var stackTrace = new CorgiStackTrace(Owner.Dungeon);

			if (stackTrace.IsValid() == false)
			{
				CorgiCombatLog.LogFatal(CombatLogCategory.DungeonStackOverFlow, "StackOverflow skillComp {0}", Uid);
				stackTrace.Finish();
				return false;
			}

			//check cooltime
			if (IsCooltime() == true)
			{
				stackTrace.Finish();
				return false;
			}

			// parent 에 target 수를 넣고, 이를 child에서 활용한다.
			logNode.TargetCount = targetList.Count;

			var ret = false;

			SkillCompLogNode curLog = null;
			foreach (var target in targetList)
			{
				bool isInsert = false;
				if (!CheckTargetAliveState(target))
				{
					continue;
				}

				if (curLog == null || _isEachTarget)
				{
					 curLog = CreateLogNode(target);
					 isInsert = true;
				}
				if (curLog == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"failed to CreateLogNode {0}", Uid);
					continue;
				}

				curLog.Parent = logNode;

				var actorLogNode = logNode as SkillActorLogNode;
				if (actorLogNode != null)
				{
					 curLog.AddDetailLog($"SkillComp {curLog.GetName()} to {target.Name} invoked by {actorLogNode.GetName()}");
				}

				if (conditions != null && conditions.Count > 0)
				{
					var activeCount = ConditionComp.CheckActive(conditions, curLog);
					if (activeCount <= 0)
					{
						//curLog.ActiveCount = 0;
						continue;
					}

					curLog.ActiveCount = activeCount;
				}

				for (var i = 0; i < curLog.ActiveCount; i++)
				{
					if (DoApplyInner(target, curLog))
					{
						 ret = true;
						 if (isInsert)
						 {
							 logNode.AddChild(curLog);
						 }
					}
				}
			}

			stackTrace.Finish();
			return ret;
		}

		protected bool CheckTargetAliveState(Unit target)
		{
			if (target != null)
			{
				if (_targetAliveState == UnitAliveState.Alive && target.IsLive()
				    || _targetAliveState == UnitAliveState.Dead && !target.IsLive())
				{
					return true;
				}
			}
			return false;
		}

		protected virtual bool DoApplyInner(Unit target, CombatLogNode logNode) { return false; }
		
		static public long CalcBaseAmount(long baseAmount, float levelMod, uint level, int constValue)
		{
			if (levelMod >= 0)
			{
				var constantFactor = (1d + (double)level / constValue);

				var retValue = baseAmount + (long)Math.Pow((double)(level*levelMod), constantFactor);
				return retValue;

			}
			else
			{
				var plusLevelMod = (-1) * levelMod;
				var constantFactor = (1d + (double)level / constValue);
				
				var retValue = baseAmount + (long)Math.Pow((double)(level*plusLevelMod), constantFactor)*(-1);
				return retValue;
			}
		}

		static public float CalcBasePercent(float basePercent, uint mastery, uint level, float increasePercent, int masteryPerLevel)
		{
			if (mastery == 0)
			{
				return basePercent;
			}
			
			var levelMastery = (uint)(level * masteryPerLevel);
			var appliedMastery = levelMastery >= mastery ? mastery : levelMastery;

			var retValue = (float)(basePercent * (1.0 + appliedMastery * increasePercent / 100));
			
			return retValue;
		}

	}
}