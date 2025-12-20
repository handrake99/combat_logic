//
//using System;
//using System.Collections.Generic;
//using IdleCs;
//using IdleCs.Spec;
//
//namespace IdleCs.GameLogic
//{
//    [System.Serializable]
//	public class AmountFactorSkillFeatureComp : SkillFeatureComp
//	{
//		//SkillFeatureIgnoreType _ignoreType;
//
//		public AmountFactorSkillFeatureComp(ActiveSkillComp skillComp)
//			:base(skillComp)
//		{
//		}
//		public override bool Load(long code)
//		{
//			bool ret = base.Load(code);
//			if(ret == false)
//			{
//				return false;
//			}
//
//			return true;
//		}
//
//		public override bool DoApplyPre(SkillOutputActive output, CombatLogNode logNode)
//		{
//			IUnit target = output.Target;
//
//			if(this.CheckActive(target, output, logNode) <= 0)
//			{
//				return false;
//			}
//
//			ActiveSkillComp activeComp = null;
//			if(output.SkillComp is ActiveSkillComp)
//			{
//				activeComp = (ActiveSkillComp)output.SkillComp;
//			}
//
//			if(activeComp == null)
//			{
//				return false;
//			}
//
//			if( activeComp.AttackPower > activeComp.SpellPower )
//			{
//				activeComp.SpellPower = 0;
//			}else
//			{
//				activeComp.AttackPower = 0;
//			}
//
//			return true;
//		}
//	}
//}