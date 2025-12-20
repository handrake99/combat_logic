
using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public class DamageReducedHPPercentSkillComp : DamageSkillComp
	{
		public DamageReducedHPPercentSkillComp()
		{
		}
		
        protected override bool LoadInternal(ulong uid)
        {
            var spec = Owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);
            if (spec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DamageAtkFactorSkillComp uid : {0}", uid);
                return false;
            }

            IgnoreDefence = true;
            IgnoreCritical = true;
            IgnoreEvent = true;
            
            return base.LoadInternal(uid);
        }
        
        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            var curLog = new DamageSkillCompLogNode(Owner, target, this);
            
	        curLog.IgnoreDefence = IgnoreDefence;
	        curLog.IgnoreCritical = IgnoreCritical;
	        curLog.IgnoreEvent = IgnoreEvent;
            
            return curLog;
        }
		
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
		{
            var curLog = logNode as DamageSkillCompLogNode;
            if (curLog == null)
            {
                return false;
            }
            
            var caster = Owner.Dungeon.GetUnit(curLog.CasterId);
            if (caster == null || caster.CombatSideType == CombatSideType.Player && target.CombatSideType == CombatSideType.Enemy)
            {
	            return false;
            }
            
			if (BaseFactor > 0)
			{
				var reducedHP = target.MaxHP - target.CurHP;
				curLog.Damage = BaseFactor * reducedHP;
                curLog.AddDetailLog($"Deal :{curLog.Damage}, Factor:{BaseFactor}, ReducedHP :{reducedHP}");
			}
			else
			{
				return false;
			}

            if (DoApplyDamage(curLog) == false)
            {
                return false;
            }
	        
            return true;
		}
	}

}
