using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace IdleCs.GameLogic
{
    public class DungeonTimeConditionComp : ConditionComp
    {
        private ulong _dungeonTime = 0u;
        
        public DungeonTimeConditionComp()
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

                _dungeonTime = (ulong)CorgiJson.ParseLong(param, "dungeonTime");
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

            var curTime = dungeon.CurCombatTick;

            if (curTime >= _dungeonTime)
            {
                return 0;
            }

            return 1;
        }
        
    }
}