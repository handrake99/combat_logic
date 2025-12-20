//using System;
//using System.Collections.Generic;
//
//using IdleCs.GameData;
//
//
//namespace IdleCs.GameLogic
//{
//    public class BarrierDamageByDamagedHPPassiveComp : BarrierPassiveComp
//    {
//		private int _amount = 0;
//		private int _maxAmount = 0;
//	    
//	    private float _absorbRate 			= 0.0f;
//	    
//        public BarrierDamageByDamagedHPPassiveComp()
//        {
//			EventManager.Register(CombatEventType.OnBeingHitAlways, this.OnDamaged);
//        }
//	    
//		protected override bool Load(GameAsset asset)
//		{
//			var thisAsset = asset as BarrierDamageByDamagedHPSkillCompAsset;
//
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//
//			if (thisAsset.DamagedHPRateFactor <= 0 || thisAsset.AbsorbRate <= 0)
//			{
//				return false;
//			}
//			_absorbRate 			= thisAsset.AbsorbRate;
//			
//			_maxAmount = (int)((Owner.MaxHP - Owner.CurHP) * thisAsset.DamagedHPRateFactor);
//
//			if (_maxAmount <= 0)
//			{
//				return false;
//			}
//
//			_amount = _maxAmount;
//
//			return base.Load(asset);
//		}
//
//		bool OnDamaged(EventParam eventParam, CombatLogNode logNode)
//		{
//			var dmgNode = logNode as DamageSkillCompLogNode;
//
//			if (dmgNode == null)
//			{
//				return false;
//			}
//
//			int absorbAmount = 0;
//
//            absorbAmount = dmgNode.Absorb(_amount, _absorbRate);
//
//			if (absorbAmount <= 0)
//			{
//				return false;
//			}
//
//            _amount -= absorbAmount;
//
//            if(_amount <= 0)
//            {
//	            this.Parent.DoOver(logNode);
//            }
//
//            var newNode = new AbsorbDamageLogNode(this.Parent, this, absorbAmount);
//            dmgNode.AddChild(newNode);
//
//			return true;
//		} 
//	    
//    }
//}
