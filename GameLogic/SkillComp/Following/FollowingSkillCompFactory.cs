//using System;
//using System.Collections.Generic;
//
//using IdleCs;
//using IdleCs.GameData;
//using IdleCs.Library;
//
//
//namespace IdleCs.GameLogic
//{
//    public static class FollowingSkillCompFactory
//    {
//        public static FollowingSkillComp Create(FollowingAsset asset, Unit owner, ISkillActor actor)
//        {
//            if (asset == null)
//            {
//                return null;
//            }
//
//            if (asset.SkillCompType == SkillCompType.Active)
//            {
//                return CreateActive(asset as ActiveFollowingAsset, owner, actor);
//            }else if (asset.SkillCompType == SkillCompType.Effect)
//            {
//                return CreateEffect(asset as EffectFollowingAsset, owner, actor);
//            }
//
//            return null;
//        }
//        
//        public static ActiveFollowingSkillComp CreateActive(ActiveFollowingAsset asset, Unit owner, ISkillActor actor)
//        {
//            var retFollowing = new ActiveFollowingSkillComp();
//
//            retFollowing.Init(owner, actor);
//
//            if (retFollowing.LoadObject(asset) == false)
//            {
//                return null;
//            }
//
//            return retFollowing;
//        }
//        
//        public static EffectFollowingSkillComp CreateEffect(EffectFollowingAsset asset, Unit owner, ISkillActor actor)
//        {
//            var retFollowing = new EffectFollowingSkillComp();
//
//            retFollowing.Init(owner, actor);
//
//            if (retFollowing.LoadObject(asset) == false)
//            {
//                return null;
//            }
//
//            return retFollowing;
//        }
//    }
//}
