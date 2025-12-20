using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class DispelBuffSkillComp : ActiveSkillComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            return base.LoadInternal(uid);
        }

        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            return new DispelSkillCompLogNode(Owner, target, this);
        }
        
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var curLog = logNode as DispelSkillCompLogNode;
            if (curLog == null)
            {
	            return false;
            }
            
            // try interrupt
	        foreach(var curInst in target.ContinuousList)
			{
				if (curInst.BenefitType != ContinuousBenefitType.Buff)
				{
					continue;
				}

				curLog.DoDispel(curInst);
			}

	        if (curLog.Result > 0)
	        {
				var eventParam = new EventParamSkillComp(Owner.Dungeon, this, curLog);
				
				Owner.OnEvent(CombatEventType.OnDispelBuff, eventParam, curLog);
	        }
	        
			return true;
        }
        
    }
}