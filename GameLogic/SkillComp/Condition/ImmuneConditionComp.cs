using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public class ImmuneConditionComp : ConditionComp
    {
	    private ImmuneType _immuneType = ImmuneType.None;
	    
	    public ImmuneConditionComp()
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

                _immuneType = ParseParam<ImmuneType>(param, "immuneType");
            }
            catch (Exception e)
            {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid condition params {0}", GameDataManager.GetStringByUid(sheet.Uid));
                return false;
            }

            return true;
		}
	    
        public override int CheckActive(SkillCompLogNode logNode)
		{
            var dungeon = Owner.Dungeon;
            var target = dungeon.GetUnit(logNode.TargetId);
            var unit = GetUnit(Owner, target, logNode);

            var isImmune = unit.ApplyImmune(_immuneType);

            if (isImmune)
            {
	            return 1;
            }
            
			return 0;
		}
    }
}
