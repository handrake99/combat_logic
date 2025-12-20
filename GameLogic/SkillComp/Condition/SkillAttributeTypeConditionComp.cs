using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace IdleCs.GameLogic
{
    public class SkillAttributeTypeConditionComp : ConditionComp
    {
        private SkillAttributeType _attributeType = SkillAttributeType.SatNone;
        
        public SkillAttributeTypeConditionComp()
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

                _attributeType = ParseParamPascal<SkillAttributeType>(param, "attributeType");
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
            var thisAttrType = logNode.GetAttributeType();

            if (_attributeType == SkillAttributeType.SatNone)
            {
                return 1;
            }

            if (thisAttrType == _attributeType)
            {
                return 1;
            }

            return 0;
        }
        
        public override int CheckActive(SkillActionLogNode logNode)
        {
            var thisAttrType = logNode.SkillAttribute;

            if (_attributeType == SkillAttributeType.SatNone)
            {
                return 1;
            }

            if (thisAttrType == _attributeType)
            {
                return 1;
            }

            return 0;
        }
        
    }
}