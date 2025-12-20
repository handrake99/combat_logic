using System;

using IdleCs.Utils;
using IdleCs.GameLog;
//
//
namespace IdleCs.GameLogic
{
    public abstract class HealSkillComp : ActiveSkillComp
    {
        public HealSkillComp()
        {
            
        }
        
        protected override bool LoadInternal(ulong uid)
        {
	        IgnoreCritical = false;
	        IgnoreEvent = false;

	        CanAreaEffect = true;
	        
            return base.LoadInternal(uid);
        }

        protected bool DoApplyHeal(HealSkillCompLogNode healNode)
        {
	        return DoApplyHeal(Owner, this, healNode);
        }

        public static bool DoApplyHeal(Unit owner, HealSkillComp skillComp, HealSkillCompLogNode healNode)
        {
	        if (healNode == null)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DoApplyDamage Called");
		        return false;
	        }
	        
			var targetId = healNode.TargetId;
            var casterId = healNode.CasterId;
            var target = owner.Dungeon.GetUnit(targetId);
            var caster = owner.Dungeon.GetUnit(casterId);
	        
	        var healEventParam = new EventParamSkillComp(owner.Dungeon, skillComp, healNode);

			if(target.IsLive() == false)
			{
				//CorgiLog.Assert(false);
				return false;
			}

			if (skillComp != null)
			{
				skillComp.DoApplyPreFeature(healNode);
			}
	        
			// enhance
            if (healNode.IgnoreEnhance == false)
            {
                caster.ApplyEnhance(new EnhanceType[] {EnhanceType.Heal}, healNode);
                target.ApplyEnhance(EnhanceType.IncomingHeal, healNode);
            }
            
	        //todo: critical
	        if (caster.ApplyCritical(healNode))
	        {
	        }

			// check double passive

			if (skillComp != null)
			{
				skillComp.DoApplyPostFeature(healNode);
			}
            
			//CorgiLog.LogLine("final Heal : " + healNode.Heal);

			// check low damage
			//if(logNode.Damage< 1.0f && output.IsZero == false)

            var finalHeal = target.ApplyHeal((int)healNode.Heal);
            healNode.AppliedHeal = finalHeal;
            
            healNode.AddDetailLog($"FinalHeal : {healNode.FinalHeal}");
	        
            if(healNode.IgnoreEvent == false)
            {
                target.OnEvent(CombatEventType.OnHealed, healEventParam, healNode);
                caster.OnEvent(CombatEventType.OnHeal, healEventParam, healNode);

                if(target.CurHP == target.MaxHP)
                {
                    target.OnEvent(CombatEventType.OnHealedMaxHp, healEventParam, healNode);
                }

                if(caster.ObjectId == target.ObjectId)
                {
                    target.OnEvent(CombatEventType.OnHealedSelf, healEventParam, healNode);
                }

	            if (finalHeal > healNode.Heal)
	            {
                    target.OnEvent(CombatEventType.OnHealedOver, healEventParam, healNode);
	            }
            }

	        return true;
        }
    }
}
//{
//    [System.Serializable]
//	public class HealSkillComp : ActiveSkillComp
//	{
//		//protected float  _healAmount;
//		//protected bool _isCritical = false;
//
//		//public float HealAmount {get {return _healAmount; }}
//		//public bool IsCritical { get { return _isCritical; } }
//
//		public HealSkillComp(IUnit owner, IUnit caster, ISkillActor actor)
//			: base(owner, caster, actor)
//		{}
//
//		public override bool InitState()
//		{
//			//_isCritical = false;
//
//			return base.InitState();
//		}
//
//		public override bool DoApply(IUnit target, ActionInput actionInput, CombatLogNode logNode)
//		{
//			SkillOutputActive output = this.CreateSkillOutput(target);
//
//			output.Amount = AmountAbsolute + AttackPower * apFactor + SpellPower * spFactor;
//			
//			output.Amount = output.Amount * _activeCount;
//			output.Amount = output.Amount * StackCount;
//
//			return DoApply(output, actionInput, logNode);
//		}
//
//		protected bool DoApply(SkillOutputActive output, ActionInput actionInput, CombatLogNode logNode)
//		{
//			// todo
//			CorgiLog.LogLine("heal skill comp DoApply called");
//
//			float heal = 0;
//
//			//SkillOutputActive output = this.CreateSkillOutput(target);
//			HealSkillCompLogNode healNode = new HealSkillCompLogNode(DungeonLogType.Heal, output);
//
//			// check feature 
//            ApplyPreFeature(output.Target, output, healNode);
//
//			//output.Amount = Amount + AttackPower * apFactor + SpellPower * spFactor;
//
//			GetVariation(output);
//
//			heal = output.Amount;
//
//			// owner 버프 디버프 적용
//			heal = Caster.ApplyEnhance(EnhanceType.HealAmount, output);
//
//			output.Amount = heal;
//
//			// owner Critical 적용
//			Caster.ApplyCritical(output, output.ForceCritical);
//
//			logNode.AddResult(healNode);
//
//			return this.ApplyHeal(output, actionInput, healNode);
//
//		}
//
//		protected bool ApplyHeal(SkillOutputActive output, ActionInput actionInput, HealSkillCompLogNode healNode)
//		{
//			IUnit target = output.Target;
//
//			if(target.IsAlive() == false)
//			{
//				CorgiLog.Assert(false);
//				return false;
//			}
//
//			float newHeal = output.Amount;
//
//			// check enhance
//			newHeal = target.ApplyEnhance(EnhanceType.DisableIncomingHeal, output);
//
//			if(output.IgnoreEnhance == false)
//			{
//				newHeal = target.ApplyEnhance(EnhanceType.IncomingHeal, output);
//			}
//
//
//			output.Amount = newHeal;
//
//			// check feature 
//            ApplyPostFeature(target, output, healNode);
//
//			int newHealAmount = (int) output.Amount;
//
//			int diff = target.MaxHP - target.CurHP;
//			int aggro = 0;
//
//            if(output.Caster != null)
//            {
//                output.Caster.AddAccHeal(newHealAmount);
//            }
//
//            output.Amount = target.ChangeHP(newHealAmount);
//
//            float aggroRate = 0.25f;
//
//            int tempRate = CorgiStaticData.Instance.GetConfig("healAggroRate");
//            if(tempRate >= 0)
//            {
//                aggroRate = (float)tempRate / 100;
//            }
//
//			aggro = (int) (output.Amount * aggroRate * _spec.aggroRate);
//
//			foreach(IUnit monster in DungeonManager.Instance.MonsterList)
//			{
//				monster.UpdateAggro(GetAggroTarget(actionInput), aggro, output, healNode);
//			}
//
//            if(output.IgnoreEvent == false)
//            {
//                target.OnEvent(CombatEventType.OnHealed, new EventParamSkillOutput(output), healNode);
//
//                if(target.CurHP == target.MaxHP)
//                {
//                    target.OnEvent(CombatEventType.OnHealedMaxHP, new EventParamSkillOutput(output),  healNode);
//                }
//
//                if(_caster.UnitIndex == target.UnitIndex)
//                {
//                    target.OnEvent(CombatEventType.OnHealedSelf, new EventParamSkillOutput(output),  healNode);
//                }
//            }
//
//			healNode.OnUpdate();
//
//			return true;
//
//		}
//
//	}
//
//}
