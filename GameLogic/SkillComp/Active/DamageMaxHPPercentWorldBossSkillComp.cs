
using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public class DamageMaxHPPercentWorldBossSkillComp : DamageSkillComp
	{
		public DamageMaxHPPercentWorldBossSkillComp()
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
            if (caster == null || caster.CombatSideType == CombatSideType.Player)
            {
	            return false;
            }
            
			if (BaseFactor > 0)
			{
				var ownerLevel = Owner.Level;
				var targetLevel = target.Level;

				var levelDiff = 0u;
				if (ownerLevel > targetLevel)
				{
					levelDiff = ownerLevel - targetLevel;
				}
				const uint minDiff = 50;
				var factor = BaseFactor;
				
				if (levelDiff > minDiff)
				{
					factor += (float)(levelDiff - minDiff) * 0.05f;
				}
				
				curLog.Damage = factor * target.MaxHP;
                curLog.AddDetailLog($"Deal :{curLog.Damage}, Factor:{factor}, MaxHP :{Owner.MaxHP}");
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
