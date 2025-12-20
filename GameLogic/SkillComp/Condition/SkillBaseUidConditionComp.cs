using System;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class SkillBaseUidConditionComp : ConditionComp
    {
        private ulong _baseUid;
        
        public SkillBaseUidConditionComp()
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

                var baseUidStr = CorgiJson.ParseString(param, "skillBaseUid");
                _baseUid = GameDataManager.GetUidByString(baseUidStr);
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
            var baseUid = logNode.GetSkillBaseUid();

            if (_baseUid == 0)
            {
                return 0;
            }

            if (_baseUid == baseUid)
            {
                return 1;
            }

            return 0;
        }
        
        
    }
}
