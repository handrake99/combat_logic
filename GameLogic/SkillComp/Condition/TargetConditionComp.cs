//using System;
//using System.Collections.Generic;
//using IdleCs;
//using IdleCs.GameData;
//
//using IdleCs.Library;
//
//namespace IdleCs.GameLogic
//{
//	[System.Serializable]
//	public abstract class TargetConditionComp : ConditionComp
//	{
//		private SkillTargetComp _targetComp;
//		
//		protected TargetConditionComp()
//		{
//		}
//		
//	    public override bool LoadObject(GameAsset asset)
//	    {
//		    return Load(asset);
//	    }
//
//		protected override bool Load(GameAsset asset)
//		{
//			if (base.Load(asset) == false)
//			{
//				return false;
//			}
//			
//			var thisAsset = asset as TargetConditionAsset;
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//			_targetComp = SkillTargetCompFactory.Create(Owner, thisAsset.TargetType);
//			
//			return true;
//		}
//
//		public sealed override int CheckActive(CombatLogNode logNode)
//		{
//			if (logNode == null)
//			{
//				return CheckActiveInner(Owner);
//			}
//			
//			var actorLogNode = logNode as SkillActorLogNode;
//			
//			if (actorLogNode == null)
//			{
//				return 0;
//			}
//
//			var targetList = _targetComp.GetTargetList(actorLogNode);
//
//			var accCount = 0;
//
//			foreach (var target in targetList)
//			{
//				var curCount = CheckActiveInner(target);
//
//				if (curCount > 0)
//				{
//					accCount += curCount;
//				}
//				
//			}
//
//			return accCount;
//		}
//		
//		protected virtual int CheckActiveInner(Unit target)
//		{
//			return 0;
//		}
//	}
//}
//
