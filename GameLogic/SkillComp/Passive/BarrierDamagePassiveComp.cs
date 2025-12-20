
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public abstract class BarrierDamagePassiveComp : BarrierPassiveComp
    {
		private int _amount = 0;
		protected int EnhancedAmount = 0;
		protected int OrigAmount = 0;
		
		protected int Amount
		{
			get { return _amount;} set { _amount = value; }
		}
	    
        public BarrierDamagePassiveComp() : base()
        {
	        PassiveType = PassiveSkillCompType.BarrierDamage;
	        
			//EventManager.Register(CombatEventType.OnBeingHit, OnHit);
        }

        protected override bool LoadInternal(ulong uid)
        {
	        if (base.LoadInternal(uid) == false)
	        {
		        return false;
	        }

	        return true;
        }
        
	    public override int GetBarrierAmount()
	    {
		    return _amount;
	    }
	    
	    public override int GetOrigBarrierAmount()
	    {
		    return OrigAmount;
	    }
	    
	    public override int GetEnhancedBarrierAmount()
	    {
		    return EnhancedAmount;
	    }
	    

        protected override bool OnEnter(EventParam eventParam, CombatLogNode logNode)
        {
	        OrigAmount = _amount;
	        
	        var amount = _amount;

	        var parentLog = logNode.Parent;

	        amount = (int)Owner.ApplyEnhance(EnhanceType.Barrier, (float)amount, parentLog);

	        EnhancedAmount = amount - _amount;
	        
	        _amount = amount;

			Owner.OnEvent(CombatEventType.OnBarrier, eventParam, logNode);

	        return true;
        }

		protected override bool OnHit(EventParam eventParam, CombatLogNode logNode)
		{
			var dmgNode = logNode.Parent as DamageSkillCompLogNode;
			if (dmgNode == null)
			{
				return false;
			}

			if (dmgNode.IgnoreBarrier)
			{
                dmgNode.AddDetailLog($"Ignored Barrier by Feature");
				return false;
			}
			
			var absorbAmount = 0;

			//Parent.Target.ApplyEnhance(EnhanceType.DamageToBarrier, dmgNode);

            absorbAmount = dmgNode.Absorb(_amount, AbsorbRate);

			if (absorbAmount <= 0)
			{
				return false;
			}

            _amount -= absorbAmount;
            if (_amount <= 0)
            {
	            this.Parent.DoOver(logNode);
	            
				Parent.OnEvent(CombatEventType.OnSkillEffectBarrierBreak, new EventParamEffect(Parent), logNode);
            }

            AbsorbDamageLogNode newNode = new AbsorbDamageLogNode(this.Parent, this, absorbAmount);
            dmgNode.AddChild(newNode);

			return true;
		} 
    }
}
