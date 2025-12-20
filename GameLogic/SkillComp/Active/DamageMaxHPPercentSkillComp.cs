
using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public class DamageMaxHPPercentSkillComp : DamageSkillComp
	{
		public DamageMaxHPPercentSkillComp()
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
            ForceHit = true;
            
            return base.LoadInternal(uid);
        }
        
        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            var curLog = new DamageSkillCompLogNode(Owner, target, this);
            
	        curLog.IgnoreDefence = IgnoreDefence;
	        curLog.IgnoreCritical = IgnoreCritical;
	        curLog.IgnoreEvent = IgnoreEvent;
	        curLog.IgnoreEvent = ForceHit;
            
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
				curLog.Damage = BaseFactor * target.MaxHP;
                curLog.AddDetailLog($"Deal :{curLog.Damage}, Factor:{BaseFactor}, MaxHP :{Owner.MaxHP}");
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
