using System;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class SkillAreaTypeConditionComp : ConditionComp
    {
        private SkillAreaType _areaType = SkillAreaType.None;
        
        public SkillAreaTypeConditionComp()
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

                _areaType = ParseParamPascal<SkillAreaType>(param, "areaType");
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
            var thisType = logNode.GetSkillAreaType();

            if (_areaType == SkillAreaType.None)
            {
                return 1;
            }

            if (thisType == _areaType)
            {
                return 1;
            }

            return 0;
        }
        
        
    }
}
