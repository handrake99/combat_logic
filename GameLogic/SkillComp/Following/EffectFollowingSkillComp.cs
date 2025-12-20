//using System.Collections.Generic;
//
//using IdleCs.GameData;
//
//
//namespace IdleCs.GameLogic
//{
//    public class EffectFollowingSkillComp : FollowingSkillComp
//    {
//	    private EffectFollowingType _type;
//	    
//        public EffectFollowingSkillComp()
//        {}
//
//
//        protected override bool Load(GameAsset asset)
//        {
//			var thisAsset = asset as EffectFollowingAsset;
//
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//	        _type = thisAsset.EffectFollowingType;
//
//	        return base.Load(asset);
//        }
//	    
//	    public override bool DoApply(Unit target, CombatLogNode skillCompLogNode, CombatLogNode logNode)
//	    {
//		    var effectLog = skillCompLogNode as EffectSkillCompLogNode;
//
//		    if (effectLog == null)
//		    {
//			    return false;
//		    }
//
//		    if (_type == EffectFollowingType.Success && effectLog.Inst == null)
//		    {
//                DoApplyInner(target, logNode);
//			    return true;
//		    }
//		    else if (_type == EffectFollowingType.Resist && effectLog.IsResist == false)
//		    {
//                DoApplyInner(target, logNode);
//			    return true;
//		    }
//		    
//		    return false;
//	    }
//    }
//}
