using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
//
//
namespace IdleCs.GameLogic
{
    public class DamageDefFactorSkillComp : DamageSkillComp
    {
        protected long Damage;
        protected float AtkFactor;

        public DamageDefFactorSkillComp()
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

            Damage = spec.BaseAmount;
            AtkFactor = spec.ApFactor;
            
            IgnoreDefence = false;
            IgnoreCritical = false;
            IgnoreEvent = false;
            
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

            // do apply damage
            var ownerLevel = Owner.Level;
            var dmgLvMod = 5 * ownerLevel * ownerLevel + 500;
            
            curLog.AddDetailLog($"dmgLv:{dmgLvMod}, ownerLevel:{ownerLevel}");
            
            var appliedDefence = target.ApplyDefence(Owner, curLog);

            // 기본 피해량
            var baseDamage = Owner.Defence * dmgLvMod / (dmgLvMod + appliedDefence);
            curLog.AddDetailLog($"baseDamage:{baseDamage}, ownerAttackPower:{Owner.Defence}, targetDefence:{target.Defence}");
            // 계수 피해량
            var fDamage = baseDamage * AtkFactor + BaseAmount;
            curLog.AddDetailLog($"fDamage:{fDamage}, Factor:{AtkFactor}, Damage:{BaseAmount}");
            
            curLog.Damage = ApplyAttributePower(target, fDamage, curLog);
            
            ApplyDamageEnhance(target, curLog);

            var aDamage = curLog.Damage;

            curLog.Damage = GetDamageVariation(aDamage);
            curLog.AddDetailLog($"Damage_varation:{curLog.Damage}");

            if (DoApplyDamage(curLog) == false)
            {
                return false;
            }
	        
            return true;
        }
        
        float ApplyAttributePower(Unit target, float damage, CombatLogNode logNode)
        {
            var attributeMap = GameLogicSetting.AttributeMap;
            var thisAttrType = ParentActor.AttributeType;
            var advAttrType = attributeMap[(int) thisAttrType, 0];
            var disAttrType = attributeMap[(int) thisAttrType, 1];

            var ownerAttrPower = (float)Owner.GetAttributePower(thisAttrType);
            var targetAdvAttrPower = (float)target.GetAttributePower(advAttrType);
            var targetDisAttrPower = (float)target.GetAttributePower(disAttrType);

            var retValue = damage * (1.0f + (ownerAttrPower / 10) * (targetAdvAttrPower / 10) -
                                     (ownerAttrPower / 10) * (targetDisAttrPower / 10));
            logNode.AddDetailLog($"aDamage:{retValue}, this:{thisAttrType}, adv:{advAttrType}, dis:{disAttrType}");
            logNode.AddDetailLog($"thisPower:{ownerAttrPower}, advPower:{targetAdvAttrPower}, disPower:{targetDisAttrPower}");
            
            return retValue;
        }

    }
}
