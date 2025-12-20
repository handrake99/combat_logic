
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
	public class ForceSkillFeatureComp : SkillFeatureComp
	{
		SkillFeatureForceType _forceType;

		public ForceSkillFeatureComp()
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

                var forceTypeStr = CorgiJson.ParseString(param, "forceType");
                if (string.IsNullOrEmpty(forceTypeStr))
                {
                    return false;
                }

                _forceType = CorgiEnum.ParseEnum<SkillFeatureForceType>(forceTypeStr);
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
//			IUnit target = output.Target;
//
//			if(this.CheckActive(target, output, logNode) <= 0)
//			{
//				return false;
//			}

			if (_forceType == SkillFeatureForceType.Critical)
			{
				logNode.ForceCritical = true;
			}else if(_forceType == SkillFeatureForceType.Hit)
			{
				logNode.ForceHit = true;
			}else
			{
				return false;
			}

			return true;
		}
	}
}