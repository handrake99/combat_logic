using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class CheckContinuousConditionComp : ConditionComp
    {
        private ContinuousBenefitType _benefitType = ContinuousBenefitType.None;
        private ContinuousGroupType _groupType = ContinuousGroupType.None;
        private SkillAttributeType _attributeType = SkillAttributeType.SatNone;
        private ContinuousDispelType _dispelType = ContinuousDispelType.None;
        private ulong _continuousUid = 0;
        private ContinuousDispelType _isOwner = ContinuousDispelType.None;
        
        public CheckContinuousConditionComp()
        {
            
        }

        protected override bool LoadInternal(ulong uid)
        {
            
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }
            
            var sheet = GetSpec();

            var paramStr = sheet.Params;

            try
            {
                var param = JObject.Parse(paramStr);

                if (param == null)
                {
                    return false;
                }
                
                _benefitType = ParseParam<ContinuousBenefitType>(param, "benefitType");
                _groupType = ParseParam<ContinuousGroupType>(param, "groupType");
                _attributeType = ParseParamPascal<SkillAttributeType>(param, "attributeType");
                _dispelType = ParseParam<ContinuousDispelType>(param, "isDispel");
                _isOwner = ParseParam<ContinuousDispelType>(param, "isOwner");
                var uidStr = CorgiJson.ParseString(param, "continuousUid");
                if (string.IsNullOrEmpty(uidStr) == false)
                {
                    _continuousUid = GameDataManager.GetUidByString(uidStr);
                }
            }
            catch (Exception e)
            {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid condition params {0}", GameDataManager.GetStringByUid(sheet.Uid));
                return false;
            }

            return true;
            
        }

        public override int CheckActive(SkillCompLogNode logNode)
        {
            var dungeon = Owner.Dungeon;
            var target = dungeon.GetUnit(logNode.TargetId);
            var unit = GetUnit(Owner, target, logNode);

            int count = 0;

            if (unit == null)
            {
                return 0;
            }

            foreach (var inst in unit.ContinuousList)
            {
                if (inst == null)
                {
                    continue;
                }

                bool isCorrect = true;

                if (_benefitType != ContinuousBenefitType.None && inst.BenefitType != _benefitType)
                {
                    isCorrect = false;
                }

                if (_groupType != ContinuousGroupType.None && inst.GroupType != _groupType)
                {
                    isCorrect = false;
                }
                
                if (_attributeType != SkillAttributeType.SatNone && inst.AttributeType != _attributeType)
                {
                    isCorrect = false;
                }

                if (_continuousUid != 0 && inst.Uid != _continuousUid)
                {
                    isCorrect = false;
                }

                if (_dispelType != ContinuousDispelType.None)
                {
                    ContinuousDispelType isDispel;
                    if (inst.IsDispel == false)
                    {
                        isDispel = ContinuousDispelType.False;
                    }
                    else
                    {
                        isDispel = ContinuousDispelType.True;
                    }

                    if (isDispel != _dispelType)
                    {
                        isCorrect = false;
                    }
                }

                if (_isOwner == ContinuousDispelType.True)
                {
                    if (inst.Caster.ObjectId != Owner.ObjectId)
                    {
                        isCorrect = false;
                    }
                }

                if (isCorrect)
                {
                    count += (int)inst.StackCount;
                }
            }
            
            return GetRetValue(count);
        }
    }
}