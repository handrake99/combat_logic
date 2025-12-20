using System;
using Newtonsoft.Json.Linq;
using IdleCs.GameLog;
using IdleCs.Utils;
using IdleCs.Managers;

namespace IdleCs.GameLogic
{
    public class DrainSkillFeatureComp : SkillFeatureComp
    {
        private float _factor;

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

                _factor = CorgiJson.ParseFloat(param, "factor");

                if (_factor <= 0.0f)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid DrainSkillFeatureComp params {0}", GameDataManager.GetStringByUid(sheet.Uid));
                return false;
            }

            return true;
        }

        public override bool DoApplyPost(ActiveSkillCompLogNode logNode)
        {
            if (logNode is DamageSkillCompLogNode dmgNode)
            {
                var healNode = new HealSkillCompLogNode(Owner, Owner, null);

                healNode.Heal = dmgNode.Damage * _factor;

                HealSkillComp.DoApplyHeal(Owner, null, healNode);
                
                dmgNode.AddChild(healNode);

                return true;
            }
            
            return false;
        }
    }
}