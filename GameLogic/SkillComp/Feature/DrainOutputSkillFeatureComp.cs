//using System.Collections.Generic;
//
//using IdleCs.Library;
//
//namespace IdleCs.GameLogic
//{
//    [System.Serializable]
//	public class DrainOutputSkillFeatureComp : SkillFeatureComp
//	{
//		public float _outputFactor;
//		public int _minDrainValue = 0;
//		public int _maxDrainValue = 0;
//
//		private HealSkillOutputSkillComp _healComp = null;
//
//		public DrainOutputSkillFeatureComp()
//		{
//		}
//		
//		protected override bool Load(GameAsset asset)
//		{
//			var thisAsset = asset as DrainOutputFeatureAsset;
//
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//			_outputFactor = thisAsset.OutputFactor;
//			_minDrainValue = thisAsset.MinDrainValue;
//			_maxDrainValue = thisAsset.MaxDrainValue;
//			
//			_healComp = new HealSkillOutputSkillComp();
//			
//			if (_healComp == null)
//			{
//				return false;
//			}
//			_healComp.SetDefault(Owner, SkillActor);
//			
//			// NEVET LOAD
//			
//			return true;
//		}
//
//		public override bool DoApplyPost(int activeCount, ActiveSkillCompLogNode logNode)
//		{
//			if(activeCount <= 0)
//			{
//				return false;
//			}
//
//			var mod = _outputFactor * activeCount;
//
//			var drainHP = logNode.DrainOutput(mod, _minDrainValue, _maxDrainValue);
//			
//			var newTargetList = new List<Unit>();
//			newTargetList.Add(Owner);
//			
//			_healComp.DoApplyPre(drainHP, logNode);
//
//			if (_healComp.DoApply(newTargetList, null, null, logNode) == false)
//			{
//				CorgiLog.LogError("can't transfer damage.");
//				return false;
//			}
//			
//			return true;
//		}
//	}
//}