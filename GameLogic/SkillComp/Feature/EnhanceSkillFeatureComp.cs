using System;
using System.Collections.Generic;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class EnhanceSkillFeatureComp : SkillFeatureComp
	{
		private EnhanceType _enhanceType;
		private int _baseAbsolute;
		private float _basePercent;

		public EnhanceSkillFeatureComp()
			: base()
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

                var typeStr = CorgiJson.ParseString(param, "enhanceType");
                if (string.IsNullOrEmpty(typeStr))
                {
                    return false;
                }

                _enhanceType = CorgiEnum.ParseEnum<EnhanceType>(typeStr);
                _baseAbsolute = CorgiJson.ParseInt(param, "baseAbsolute");
                _basePercent = CorgiJson.ParseFloat(param, "basePercent");

                if (_enhanceType == EnhanceType.None || _baseAbsolute == -1 || _basePercent < 0.0f)
                {
	                return false;
                }
            }
            catch (Exception e)
            {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid feature params {0}", sheet.Uid);
                return false;
            }

			return true;
		}

		public override bool DoApplyPre(ActiveSkillCompLogNode logNode)
		{
			logNode.AddDetailLog($"ApplyFeatureEnhance {_enhanceType} : abs:{_baseAbsolute}, percent {_basePercent}");
			
			if (_basePercent > 0)
			{
				logNode.AddEnhanceValue(_enhanceType, _baseAbsolute, _basePercent, 1f);
			}
			else
			{
				logNode.AddEnhanceValue(_enhanceType, _baseAbsolute, 1f, _basePercent);
			}
			

			return true;
		}
	}
}