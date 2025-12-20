using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace IdleCs.GameLogic
{
    public class DungeonTypeConditionComp : ConditionComp
    {
        private DungeonType _dungeonType = DungeonType.None;
        
        public DungeonTypeConditionComp()
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

                _dungeonType = ParseParamPascal<DungeonType>(param, "dungeonType");
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

            var thisType = dungeon.DungeonType;

            if (_dungeonType == DungeonType.None)
            {
                return 1;
            }

            if (_dungeonType == thisType)
            {
                return 1;
            }

            return 0;
        }
        
    }
}
