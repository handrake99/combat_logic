using System;
using IdleCs.GameLog;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
	[System.Serializable]
	public class AmplifyDamageSkillFeatureComp : SkillFeatureComp
	{
		private float _factor;

		public AmplifyDamageSkillFeatureComp()
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

				_factor = CorgiJson.ParseFloat(param, "factor");

				if (_factor < 0.0f)
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
			logNode.AmplifyOutput(_factor);

			return true;
		}
	}
}