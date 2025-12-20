using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace IdleCs.GameLogic
{
    public class SkillTypeConditionComp : ConditionComp
    {
        private SkillType _skillType = SkillType.None;
        
        public SkillTypeConditionComp()
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

                _skillType = ParseParamPascal<SkillType>(param, "skillType");
            }
            catch (Exception e)
            {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid condition params {0}", sheet.Uid);
                return false;
            }

            return true;
        }
        
        public override int CheckActive(SkillCompLogNode logNode)
        {
            var thisType = logNode.GetSkillType();

            if (_skillType == SkillType.None)
            {
                return 1;
            }

            if (thisType == _skillType)
            {
                return 1;
            }

            if (_skillType == SkillType.Active)
            {
                if (thisType == SkillType.Casting || thisType == SkillType.Channeling)
                {
                    return 1;
                }
            }

            return 0;
        }

        public override int CheckActive(SkillActionLogNode logNode)
        {
            var thisType = logNode.SkillType;
            
            if (_skillType == SkillType.None)
            {
                return 1;
            }

            if (thisType == _skillType)
            {
                return 1;
            }

            if (_skillType == SkillType.Active)
            {
                if (thisType == SkillType.Casting || thisType == SkillType.Channeling)
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
