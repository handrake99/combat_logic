using System;
using Corgi.GameData;
using IdleCs.GameLog;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class CheckCriticalConditionComp : ConditionComp
    {
        private SkillResultType _resultType;
        
        protected override bool LoadInternal(ulong uid)
        {
            if (!base.LoadInternal(uid))
            {
                return false;
            }

            var sheet = GetSpec();
            var paramStr = sheet.Params;

            try
            {
                var param = JObject.Parse(paramStr);
                _resultType = ParseParamPascal<SkillResultType>(param, "skillResultType");
            }
            catch (Exception e)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid condition params[{0}] for {1}", paramStr, Owner.Dungeon.GameData.GetStrByUid(sheet.Uid));
                return false;
            }

            return true;
        }

        public override int CheckActive(SkillCompLogNode logNode)
        {
            var skillEventLogNode = logNode.Parent as SkillEventLogNode;

            if (skillEventLogNode == null || skillEventLogNode.EventType != CombatEventType.OnSkill)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid parent logNode in CheckCriticalConditionComp");
                return 0;
            }

            var skillActionLogNode = skillEventLogNode.Parent as SkillActionLogNode;

            if (skillActionLogNode == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid parent logNode in CheckCriticalConditionComp");
                return 0;
            }

            var check = false;
            
            if (_resultType == SkillResultType.SkillResultDamage)
            {
                check = skillActionLogNode.Childs.Exists(child =>
                {
                    if (child is DamageSkillCompLogNode dmgLogNode)
                    {
                        return dmgLogNode.IsCritical;
                    }

                    return false;
                });
            }
            else if (_resultType == SkillResultType.SkillResultHeal)
            {
                check = skillActionLogNode.Childs.Exists(child =>
                {
                    if (child is HealSkillCompLogNode healLogNode)
                    {
                        return healLogNode.IsCritical;
                    }

                    return false;
                });
            }
            else if (_resultType == SkillResultType.SkillResultNone)
            {
                check = skillActionLogNode.Childs.Exists(child =>
                {
                    if (child is DamageSkillCompLogNode dmgLogNode)
                    {
                        return dmgLogNode.IsCritical;
                    }
                    if (child is HealSkillCompLogNode healLogNode)
                    {
                        return healLogNode.IsCritical;
                    }

                    return false;
                });
            }
            else
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid condition params[{0}] for {1}", _resultType.ToString(), Owner.Dungeon.GameData.GetStrByUid(GetSpec().Uid));
            }

            return check ? 1 : 0;
        }
    }
}