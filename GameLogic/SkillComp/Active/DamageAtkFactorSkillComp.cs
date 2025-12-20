using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
//
//
namespace IdleCs.GameLogic
{
    public class DamageAtkFactorSkillComp : DamageSkillComp
    {
        //protected long Damage;
        //protected float AtkFactor;

        public DamageAtkFactorSkillComp()
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

            //Damage = spec.BaseAmount;
            //AtkFactor = spec.ApFactor;
            
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
            var baseDamage = Owner.AttackPower * dmgLvMod / (dmgLvMod + appliedDefence);
            
            curLog.AddDetailLog($"baseDamage:{baseDamage}, ownerAttackPower:{Owner.AttackPower}, targetDefence:{target.Defence}");
            // 계수 피해량
            var fDamage = baseDamage * BaseFactor + BaseAmount;
            curLog.AddDetailLog($"fDamage:{fDamage}, Factor:{BaseFactor}, Damage:{BaseAmount}, StackCount:{ParentActor.StackCount}");
            
            
            // 속성치 적용
            curLog.Damage = ApplyAttributePower(target, fDamage, curLog);

            ApplyDamageEnhance(target, curLog);
            
            // 레벨 차이 적용
            var aDamage = curLog.Damage;
            if (ownerLevel != 0 && target.Level != 0)
            {
                var levelDiff = Math.Abs((long)ownerLevel - target.Level);
                if ((int)ownerLevel - target.Level < -10)
                {
                    levelDiff = 10;
                }
                var levelPow = (float)Math.Pow(levelDiff, 1.95f) / 100;

                if (ownerLevel > target.Level)
                {
                    aDamage = aDamage * (1.0f + levelPow);
                }else if (ownerLevel < target.Level)
                {
                    aDamage = aDamage * (1.0f - levelPow);
                }
            }
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
            var thisAttrType = ParentActor.AttributeType;

            var retValue = damage;

            var attrFactor = 1.0f;

            if (thisAttrType == SkillAttributeType.SatNone) // attack
            {
                for(var i=1; i <= 6; i++)
                {
                    thisAttrType = (SkillAttributeType) i;
                    attrFactor += GetAttributeFactor(thisAttrType, target, logNode);
                }
            }
            else // skill
            {
                attrFactor += GetAttributeFactor(thisAttrType, target, logNode);
            }

            if (attrFactor < 0.2f)
            {
                attrFactor = 0.2f;
            }

            retValue = damage * attrFactor;
            
            logNode.AddDetailLog($"finalFactor:{attrFactor}, aDamage:{damage}, RetDamage :{retValue}");

            return retValue;
        }

        float GetAttributeFactor(SkillAttributeType attributeType, Unit target, CombatLogNode logNode)
        {
            var attributeMap = GameLogicSetting.AttributeMap;
            
            var advAttrType = attributeMap[(int) attributeType, 0];
            var disAttrType = attributeMap[(int) attributeType, 1];

            var ownerAttrPower = (float)Owner.GetAttributePower(attributeType);
            var targetAdvAttrPower = (float)target.GetAttributePower(advAttrType);
            var targetDisAttrPower = (float)target.GetAttributePower(disAttrType);

            var attrMod = 0f;
            if (targetAdvAttrPower > 0)
            {
                attrMod += (ownerAttrPower / 200);
            }
            if (targetDisAttrPower > 0)
            {
                attrMod -= (targetDisAttrPower / 200);
            }
            
            var finalAttrMod = Owner.ApplyEnhance(EnhanceType.Synastry, attrMod, logNode);
            
            logNode.AddDetailLog($"this:{attributeType}, adv:{advAttrType}, dis:{disAttrType}");
            logNode.AddDetailLog($"thisPower:{ownerAttrPower}, advPower:{targetAdvAttrPower}, disPower:{targetDisAttrPower}");
            logNode.AddDetailLog($"AttrMode :{attrMod}, FinalAttrMod:{finalAttrMod}");
            
            return finalAttrMod;
        }

    }
}
