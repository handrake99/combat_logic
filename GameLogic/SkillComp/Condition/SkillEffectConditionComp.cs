//using System.Collections.Generic;
//using IdleCs.GameData;
//
//
//namespace IdleCs.GameLogic
//{
//    public class SkillEffectConditionComp : TargetConditionComp
//    {
//	    private SkillEffectConditionType _skillEffectConditionType;
//	    
//	    public SkillEffectConditionComp()
//	    {
//	    }
//
//	    public override bool LoadObject(GameAsset asset)
//	    {
//		    return Load(asset);
//	    }
//	    
//	    
//		protected override bool Load(GameAsset asset)
//		{
//			if (base.Load(asset) == false)
//			{
//				return false;
//			}
//			
//			var thisAsset = asset as SkillEffectConditionAsset;
//
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//			_skillEffectConditionType = thisAsset.EffectConditionType;
//				
//			return true;
//		}
//	    
//		protected override int CheckActiveInner(Unit target)
//		{
//			if (target == null)
//			{
//				return 0;
//			}
//
//			var ret = 0;
//
//			ret += CheckActiveInner(target.BuffList);
//			ret += CheckActiveInner(target.DebuffList);
//			
//			return ret;
//		}
//
//	    private int CheckActiveInner(List<SkillEffectInst> list)
//	    {
//		    var ret = 0;
//		    
//		    foreach (var curInst in list)
//		    {
//			    if (_skillEffectConditionType == SkillEffectConditionType.BuffCount)
//			    {
//				    if (curInst.BenefitType == EffectBenefitType.Buff)
//				    {
//					    ++ret;
//				    }
//			    }else if (_skillEffectConditionType == SkillEffectConditionType.DebuffCount)
//			    {
//				    if (curInst.BenefitType == EffectBenefitType.Debuff)
//				    {
//					    ++ret;
//				    }
//			    }else if (_skillEffectConditionType == SkillEffectConditionType.CustomCount)
//			    {
//				    if (curInst.EffectInstType == SkillEffectInstType.Custom)
//				    {
//					    ++ret;
//				    }
//				    
//			    }else if (_skillEffectConditionType == SkillEffectConditionType.AttachCount)
//			    {
//				    if (curInst.EffectInstType == SkillEffectInstType.Attach)
//				    {
//					    ++ret;
//				    }
//				    
//			    }else if (_skillEffectConditionType == SkillEffectConditionType.AuraCount)
//			    {
//				    if (curInst.EffectInstType == SkillEffectInstType.Aura)
//				    {
//					    ++ret;
//				    }
//			    }else if (_skillEffectConditionType == SkillEffectConditionType.DotCount)
//			    {
//				    if (curInst.EffectInstType == SkillEffectInstType.Dot)
//				    {
//					    ++ret;
//				    }
//			    }
//		    }
//
//		    return ret;
//	    }
//    }
//}
