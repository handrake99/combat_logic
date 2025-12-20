using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class DrainDamageHPPercentSkillComp : DamageSkillComp
    {
	    private float _drainFactor;
		public DrainDamageHPPercentSkillComp()
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
            
			try
			{ 
				var parameter = JObject.Parse(spec.Params);
				var drainFactor= CorgiJson.ParseFloat(parameter, "drainFactor");

				if (drainFactor > 0)
				{
					_drainFactor = drainFactor;
				}

			 }
			 catch (Exception e)
			 {
				  CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
				  return false;
			 }
            
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
				curLog.Damage = BaseFactor * target.CurHP;
                curLog.AddDetailLog($"Deal :{curLog.Damage}, Factor:{BaseFactor}, CurHP :{target.CurHP}");
			}
			else
			{
				return false;
			}

            if (DoApplyDamage(curLog) == false)
            {
                return false;
            }

            if (curLog.Damage <= 0 || _drainFactor <= 0f)
            {
	            // no drain
	            return true;
            }

            var heal = curLog.Damage * _drainFactor;
            
			var newLogNode = new HealSkillCompLogNode(Owner, Owner, this);

			newLogNode.Heal = heal;
			
			newLogNode.ForceHit = false;
			newLogNode.IgnoreImmune = false;
			newLogNode.IgnoreEvent = true;
			newLogNode.IgnoreEnhance = true;
			newLogNode.IgnoreCritical = true;
			newLogNode.IgnoreBarrier = false;
			newLogNode.IgnoreDefence = true;
			
			HealSkillComp.DoApplyHeal(Owner, null, newLogNode);
			
			logNode.AddChild(newLogNode);
	        
            return true;
		}
    }
}