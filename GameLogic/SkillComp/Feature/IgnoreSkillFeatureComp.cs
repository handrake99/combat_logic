
using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class IgnoreSkillFeatureComp : SkillFeatureComp
	{
		SkillFeatureIgnoreType _ignoreType;
		private float _ignoreFactor = 1f;

		public IgnoreSkillFeatureComp()
			:base()
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

                var typeStr = CorgiJson.ParseString(param, "ignoreType");
                if (string.IsNullOrEmpty(typeStr))
                {
                    return false;
                }

                _ignoreType = CorgiEnum.ParseEnum<SkillFeatureIgnoreType>(typeStr);
                if (CorgiJson.IsValidFloat(param, "ignoreFactor"))
                {
					_ignoreFactor = CorgiJson.ParseFloat(param, "ignoreFactor");
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
			switch (_ignoreType)
			{
				case SkillFeatureIgnoreType.Critical:
					logNode.IgnoreCritical = true;
					break;
				case SkillFeatureIgnoreType.Event:
					 logNode.IgnoreEvent = true;
					break;
				case SkillFeatureIgnoreType.Barrier:
					 logNode.IgnoreBarrier = true;
					break;
				case SkillFeatureIgnoreType.Immune:
					 logNode.IgnoreImmune = true;
					break;
				case SkillFeatureIgnoreType.Enhance:
					 logNode.IgnoreEnhance = true;
					break;
				case SkillFeatureIgnoreType.Defence:
					logNode.IgnoreDefence = true;
					logNode.IgnoreDefenceFactor = _ignoreFactor;
					break;
				default:
					return false;
			}

            //logNode.AddDetailLog($"Apply Feature Type {_ignoreType}");
			return true;
		}
	}
}
