using System;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class SkillActorTypeConditionComp : ConditionComp
    {
        private SkillActorType _actorType;
        
        public SkillActorTypeConditionComp()
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

                _actorType = ParseParamPascal<SkillActorType>(param, "skillActorType");
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
            var actorType = logNode.GetSkillActorType();
            
            if (actorType == SkillActorType.None)
            {
                return 0;
            }

            if (_actorType == actorType)
            {
                return 1;
            }

            return 0;
        }
        
        
    }
}
