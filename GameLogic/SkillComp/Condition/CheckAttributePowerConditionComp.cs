using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class CheckAttributePowerConditionComp : ConditionComp
    {
	    private SelectCriteriaType _selectType = SelectCriteriaType.None;
	    private SkillAttributeType _attributeType = SkillAttributeType.SatNone;
		    
	    public CheckAttributePowerConditionComp()
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

                _attributeType = ParseParamPascal<SkillAttributeType>(param, "attributeType");
                _selectType = ParseParam<SelectCriteriaType>(param, "selectType");
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
			if (_attributeType == SkillAttributeType.SatNone)
			{
				return 1;
			}
			
            var dungeon = Owner.Dungeon;
            var target = dungeon.GetUnit(logNode.TargetId);
            var unit = GetUnit(Owner, target, logNode);

            var fireValue = unit.GetAttributePower(SkillAttributeType.SatFire);
            var iceValue = unit.GetAttributePower(SkillAttributeType.SatIce);
            var earthValue = unit.GetAttributePower(SkillAttributeType.SatEarth);
            var lightningValue = unit.GetAttributePower(SkillAttributeType.SatLightning);
            var holyValue = unit.GetAttributePower(SkillAttributeType.SatHoly);
            var shadowValue = unit.GetAttributePower(SkillAttributeType.SatShadow);

            uint attributeValue = 0;
            
            if (_attributeType == SkillAttributeType.SatFire)
            {
	            attributeValue = fireValue;
            }else if (_attributeType == SkillAttributeType.SatIce)
            {
	            attributeValue = iceValue;
            }else if (_attributeType == SkillAttributeType.SatEarth)
            {
	            attributeValue = earthValue;
            }else if (_attributeType == SkillAttributeType.SatLightning)
            {
	            attributeValue = lightningValue;
            }else if (_attributeType == SkillAttributeType.SatHoly)
            {
	            attributeValue = holyValue;
            }else if (_attributeType == SkillAttributeType.SatShadow)
            {
	            attributeValue = shadowValue;
            }


            if (_selectType == SelectCriteriaType.Highest)
            {
	            if (attributeValue >= fireValue
	                && attributeValue >= iceValue
	                && attributeValue >= earthValue
	                && attributeValue >= lightningValue
	                && attributeValue >= holyValue
	                && attributeValue >= shadowValue)
	            {
		            return 1;
	            }

            }else if (_selectType == SelectCriteriaType.Lowest)
            {
	            if (attributeValue <= fireValue
	                && attributeValue <= iceValue
	                && attributeValue <= earthValue
	                && attributeValue <= lightningValue
	                && attributeValue <= holyValue
	                && attributeValue <= shadowValue)
	            {
		            return 1;
	            }
	            
            }
			return 0;
		}
        
    }
}