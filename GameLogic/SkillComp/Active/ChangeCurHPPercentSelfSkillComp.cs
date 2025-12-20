using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class ChangeCurHPPercentSelfSkillComp : ActiveSkillComp
    {
		//private float _finalDamage = 0f;
		
		public ChangeCurHPPercentSelfSkillComp()
		{
		}
		
		public override SkillCompLogNode CreateLogNode(Unit target)
		{
            var curLog = new ChangeCurHPPercentSelfLogNode(this.Owner, target, this);
            
	        return curLog;
		}

        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
	        var thisLogNode = logNode as ChangeCurHPPercentSelfLogNode;
	        if (thisLogNode == null )
	        {
		        return false;
	        }

	        // check self
	        if (target.ObjectId != Owner.ObjectId)
	        {
		        return false;
	        }

	        thisLogNode.PreHP = target.CurHP;
	        
	        // apply
	        var resultHP = (int)(target.MaxHP * this.BaseFactor);
	        target.ResetHP(resultHP);
	        thisLogNode.CurHP = target.CurHP;

            thisLogNode.AddDetailLog($"ChangeHPPercent:HP({thisLogNode.PreHP}) Change To ({thisLogNode.CurHP}) rate by ({BaseFactor})");

	        return true;

        }
        
    }
}