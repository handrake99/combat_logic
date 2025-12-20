using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
	public abstract class PassiveSkillComp : SkillComp
	{
		/// <summary>
		/// static data
		/// </summary>
		private SkillPassiveInfoSpec _spec;

		public SkillPassiveInfoSpec GetSpec()
		{
			return _spec;
		}

		public override string GetName()
		{
			return _spec.Name;
		}

		private PassiveSkillCompType _passiveType = PassiveSkillCompType.None;

		public SkillEffectInst Parent { get; private set; }
		public Unit Target { get; private set; }


		public PassiveSkillCompType PassiveType
		{
			get { return _passiveType; }
			protected set { _passiveType = value; }
		}

		public uint StackCount
		{
			get { return (Stackable) ? Parent.StackCount : 1; }
		}

		public bool Stackable { get; private set; }

		public long BaseAbsolute { get; private set; }
		public float BasePercent { get; private set; }
		protected float LevelModifier { get; private set; }

		private List<ConditionComp> _skillConditions = new List<ConditionComp>();

		public PassiveSkillComp()
		{
			EventManager = new EventManager(CombatEventCategory.Effect);

			EventManager.Register(CombatEventType.OnSkillEffectEnter, this.OnEnter);
		}

		public override bool SetDefault(Unit owner, ISkillActor skillActor)
		{
			if (owner == null || skillActor == null)
			{
				return false;
			}

			Parent = skillActor as SkillEffectInst;
			if (Parent == null)
			{
				return false;
			}
			
			if (base.SetDefault(owner, skillActor) == false)
			{
				return false;
			}

			return true;
		}

		protected override bool LoadInternal(ulong uid)
		{
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}

			var spec = Owner.Dungeon.GameData.GetData<SkillPassiveInfoSpec>(uid);
			if (spec == null)
			{
				return false;
			}

			var constValue = Owner.Dungeon.GameData.GetConfigNumber("config.combat.factor.skill.passive", 500);

			_spec = spec;

			Cooltime = spec.CoolTime;
			Stackable = spec.Stackable;

			LevelModifier = spec.LevelModifier;
			BaseAbsolute = CalcBaseAmount(spec.BaseAbsolute, LevelModifier, Parent.Level, constValue);
			BasePercent = spec.BasePercent;


			// if (_passiveType == PassiveSkillCompType.BarrierDamage)
			// {
			// 	// Exception handling for barrier.
			// 	// "absorbFactor" in params is affected by skill slot mastery.
			// 	// Handle in BarrierDamageByFactorPassiveComp.
			// 	BasePercent = spec.BasePercent;
			// }
			// else
			// {
			// 	var increasePercent =
			// 		Owner.Dungeon.GameData.GetConfigFloat("config.skill_slot.mastery.increase_percent", 0.01f);
			// 	var masteryPerLevel = Owner.Dungeon.GameData.GetConfigNumber("config.skill_slot.mastery.level", 100);
			// 	BasePercent = CalcBasePercent(_spec.BasePercent, Mastery, ParentActor.Level, increasePercent, masteryPerLevel);
			// }

			if (spec.IsPassiveLevel)
			{
				BasePercent *= Parent.Level;
			}

			if (_spec.ConditionUid != 0)
			{
				_skillConditions = SkillConditionCompFactory.Create(_spec.ConditionUid, Owner);
			}

			return true;
		}

		bool OnEnter(EventParam eventParam, CombatLogNode logNode)
		{
			var newLog = CreateLogNode(Parent.Target);

			logNode.AddChild(newLog);

			logNode.AddDetailLog($"OnPassive SkillComp : {Owner.Dungeon.GameData.GetStrByUid(Uid)}");
			return false;
		}

		public override SkillCompLogNode CreateLogNode(Unit target)
		{
			return new PassiveSkillCompLogNode(Parent.Caster, target, this);
		}

		protected int CheckActive(SkillActionLogNode logNode)
		{
			if (_skillConditions.Count == 0)
			{
				return 1;
			}
			return ConditionComp.CheckActive(_skillConditions, logNode);
		}
		
		protected int CheckActive(SkillCompLogNode logNode)
		{
			if (_skillConditions.Count == 0)
			{
				return 1;
			}
			return ConditionComp.CheckActive(_skillConditions, logNode);
		}
		
		public virtual bool GetEnhanceValue(EnhanceType enhanceType, ref float absValue, ref float percentPlus,
			ref float percentMinus, SkillActionLogNode logNode)
		{
			return false;
		}

		public virtual bool GetEnhanceValue(EnhanceType enhanceType, ref float absValue, ref float percentPlus,
			ref float percentMinus, SkillCompLogNode logNode)
		{
			return false;
		}

		public virtual bool GetAvailableTarget(Skill skill, Unit caster, ref List<Unit> targetList)
		{
			return false;
		}

		public virtual bool CheckImmune(ActiveSkillComp skillComp)
		{
			return false;
		}

		public virtual bool CheckImmune(SkillEffectInst skillInst)
		{
			return false;
		}

		public virtual bool CheckImmune(ImmuneType immuneType)
		{
			return false;
		}
		
		public static StatType GetStatFactorType(SkillPassiveInfoSpec spec)
		{
			try
			{
				var parameter = JObject.Parse(spec.Params);
				// stat, enhance
				var statTypeStr = CorgiJson.ParseString(parameter, "statType");

				if (string.IsNullOrEmpty(statTypeStr) == false)
				{
					return CorgiEnum.ParseEnum<StatType>(statTypeStr);
				}

				// barrier
				statTypeStr = CorgiJson.ParseString(parameter, "absorbFactorType");

				if (string.IsNullOrEmpty(statTypeStr) == false)
				{
					return CorgiEnum.ParseEnum<StatType>(statTypeStr);
				}

			}
			catch (Exception e)
			{
				CorgiCombatLog.LogFatal(CombatLogCategory.Skill, "invalid parameter {0}", spec.Name);
				return StatType.StNone;
			}
			return StatType.StNone;


		}

	}

}
