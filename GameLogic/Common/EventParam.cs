
using System;
using System.Collections.Generic;
using System.ComponentModel;
using IdleCs;

using IdleCs.GameLog;
using IdleCs.Library;


namespace IdleCs.GameLogic
{

	/// <summary>
	/// OnEvent를 위한 parameter정리
	/// todo : category 관련 정리 필요.
	/// </summary>
	public abstract class EventParam
	{
		private CombatEventCategory _eventCategory;

		public EventParam(CombatEventCategory category)
		{
			_eventCategory = category;
		}

		public CombatEventCategory EventCategory
		{
			get { return _eventCategory; }
		}

		public abstract Unit GetOwner();
		public abstract Unit GetCaster();
		public abstract Unit GetTarget();
		public abstract ISkillActor GetSkillActor();

	}

//    [System.Serializable]
//	public class EventParamNone : IEventParam
//	{
//		static EventParamNone _default = new EventParamNone();
//
//		public ISkillOutput Output { get { return null; } }
//
//		EventParamNone()
//		{
//		}
//
//		public static EventParamNone Instance
//		{
//			get
//			{
//				return _default;
//			}
//		}
//	}
//

	public class EventParamRule : EventParam
	{
		public EventParamRule()
			: base(CombatEventCategory.Rule)
		{
		}
		
		public override Unit GetOwner()
		{
			return null;
		}

		public override Unit GetCaster()
		{
			return null;
		}

		public override Unit GetTarget()
		{
			return null;
		}

		public override ISkillActor GetSkillActor()
		{
			return null;
		}
	}

	public class EventParamUnit : EventParam
	{
		Unit _unit = null;

		public Unit Unit
		{
			get { return _unit; }
		}

		public EventParamUnit(Unit unit)
			: base(CombatEventCategory.Unit)
		{
			_unit = unit;
		}

		public override Unit GetOwner()
		{
			return _unit;
		}

		public override Unit GetCaster()
		{
			return _unit;
		}

		public override Unit GetTarget()
		{
			return _unit;
		}
		public override ISkillActor GetSkillActor()
		{
			return null;
		}
	}

	public class EventParamAction : EventParam
	{
		private Dungeon _dungeon;
		private Skill _skill;
		private SkillActionLogNode _actionLogNode;

		//public Skill Skill { get { return _actionLogNode.Skill; } }

		public EventParamAction(Dungeon dungeon, Skill skill, SkillActionLogNode logNode)
			: base(CombatEventCategory.Action)
		{
			//_output = new SkillOutputAction(action, input);
			_dungeon = dungeon;
			_skill = skill;
			_actionLogNode = logNode;
		}

		public override Unit GetOwner()
		{
			var unitId = _actionLogNode.OwnerId;
			return _dungeon.GetUnit(unitId);
		}

		public override Unit GetCaster()
		{
			var unitId = _actionLogNode.CasterId;
			return _dungeon.GetUnit(unitId);
		}

		public override Unit GetTarget()
		{
			var unitId = _actionLogNode.TargetId;
			return _dungeon.GetUnit(unitId);
		}
		
		public override ISkillActor GetSkillActor()
		{
			return _skill;
		}
	}


	public class EventParamEffect : EventParam
	{
		SkillEffectInst _effectInst;

		public SkillEffectInst EffectInst
		{
			get { return _effectInst; }
		}

		public EventParamEffect(SkillEffectInst inst)
			: base(CombatEventCategory.Effect)
		{
			_effectInst = inst;
		}

		public override Unit GetOwner()
		{
			return _effectInst.Owner;
		}

		public override Unit GetCaster()
		{
			return _effectInst.Caster;
		}

		public override Unit GetTarget()
		{
			return _effectInst.Target;
		}
		public override ISkillActor GetSkillActor()
		{
			return _effectInst;
		}
	}

	public class EventParamSkillComp : EventParam
	{
		private Dungeon _dungeon;
		private SkillComp _skillComp;
		private SkillCompLogNode _skillLogNode;

		public SkillCompLogNode SkillLogNode
		{
			get { return _skillLogNode; }
		}

		public EventParamSkillComp(Dungeon dungeon, SkillComp skillComp, SkillCompLogNode skillCompLogNode)
			: base(CombatEventCategory.Action)
		{
			_dungeon = dungeon;
			_skillComp = skillComp;
			_skillLogNode = skillCompLogNode;
		}

		public override Unit GetOwner()
		{
			var unitId = _skillLogNode.OwnerId;
			return _dungeon.GetUnit(unitId);
		}

		public override Unit GetCaster()
		{
			var unitId = _skillLogNode.CasterId;
			return _dungeon.GetUnit(unitId);
		}

		public override Unit GetTarget()
		{
			var unitId = _skillLogNode.TargetId;
			return _dungeon.GetUnit(unitId);
		}
		
		public override ISkillActor GetSkillActor()
		{
			return _skillComp;
		}
	}
}


