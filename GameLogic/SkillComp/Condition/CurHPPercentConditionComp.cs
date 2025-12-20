using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public class CurHPPercentConditionComp : ConditionComp
    {
	    private float _HPPercent = 0f;
	    private CompareCriteriaType _compareType = CompareCriteriaType.None;
	    
	    public CurHPPercentConditionComp()
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

                _HPPercent = CorgiJson.ParseFloat(param, "percent");
                _compareType = ParseParam<CompareCriteriaType>(param, "compareType");
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
            
			var curPercent = ((float) unit.CurHP / unit.MaxHP);
			if (_compareType == CompareCriteriaType.Under && curPercent <= _HPPercent)
			{
				return 1;
			}

			if (_compareType == CompareCriteriaType.Over && curPercent >= _HPPercent)
			{
				return 1;
			}

			if (_compareType == CompareCriteriaType.None && curPercent == _HPPercent)
			{
				return 1;
			}
			
			return 0;
		}
    }
}
