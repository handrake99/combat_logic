using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace IdleCs.GameLogic
{
    public class RoleTypeConditionComp : ConditionComp
    {
        private RoleType _roleType = RoleType.RtNone;
        
        public RoleTypeConditionComp()
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

                _roleType = ParseParamPascal<RoleType>(param, "roleType");
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
            var dungeon = Owner.Dungeon;
            var target = dungeon.GetUnit(logNode.TargetId);
            var unit = GetUnit(Owner, target, logNode);

            var thisRoleType = unit.RoleType;

            if (_roleType == RoleType.RtNone)
            {
                return 1;
            }

            if (_roleType == thisRoleType)
            {
                return 1;
            }

            return 0;
        }
        
    }
}
