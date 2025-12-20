//using System.Collections.Generic;
//
//using IdleCs.GameData;
//
//
//namespace IdleCs.GameLogic
//{
//    public class ActiveFollowingSkillComp : FollowingSkillComp
//    {
//	    private ActiveFollowingType _type;
//	    
//        public ActiveFollowingSkillComp()
//        {}
//
//
//        protected override bool Load(GameAsset asset)
//        {
//			var thisAsset = asset as ActiveFollowingAsset;
//
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//	        _type = thisAsset.ActiveFollowingType;
//
//	        return base.Load(asset);
//        }
//	    
//	    public override bool DoApply(Unit target, CombatLogNode skillCompLogNode, CombatLogNode logNode)
//	    {
//		    var activeLog = skillCompLogNode as ActiveSkillCompLogNode;
//
//		    if (activeLog == null)
//		    {
//			    return false;
//		    }
//
//		    if (_type == ActiveFollowingType.Critical && activeLog.IsCritical)
//		    {
//			    DoApplyInner(target, logNode);
//			    return true;
//		    }
//		    
//		    return false;
//	    }
//	    
//        
//    }
//}