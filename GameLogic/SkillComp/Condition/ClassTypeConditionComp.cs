using System;
using Corgi.GameData;
using IdleCs.GameLog;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class ClassTypeConditionComp : ConditionComp
    {
        private ClassType _classType = ClassType.CtNone;
        
        public ClassTypeConditionComp()
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

                _classType = ParseParamPascal<ClassType>(param, "classType");
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

            var thisClassType = unit.ClassType;

            if (_classType == ClassType.CtNone)
            {
                return 1;
            }

            if (_classType == thisClassType)
            {
                return 1;
            }

            return 0;
        }
        
    }
}
