using System;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class ChangeContinuousDurationSkillComp : ActiveSkillComp
    {
		//private float _finalDamage = 0f;
		private long _diffTime = 0;

		private ContinuousBenefitType _benefitType = ContinuousBenefitType.None;
		private ContinuousGroupType _groupType = ContinuousGroupType.None;
		private ulong _continuousUid = 0;
		
		public ChangeContinuousDurationSkillComp()
		{
		}
		
        protected override bool LoadInternal(ulong uid)
        {
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }

            var spec = GetSpec();
            if (spec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ChangeContinuousDurationSkillComp uid : {0}", uid);
                return false;
            }

            _diffTime = spec.BaseAmount;
            
	        if (string.IsNullOrEmpty(spec.Params) == false)
	        {
				try
				{
					var parameter = JObject.Parse(spec.Params);
					var continuousTypeStr = CorgiJson.ParseString(parameter, "benefitType");
					var groupTypeStr = CorgiJson.ParseString(parameter, "groupType");
					var uidStr = CorgiJson.ParseString(parameter, "continuousUid");

					if (string.IsNullOrEmpty(continuousTypeStr) == false)
					{
						_benefitType = CorgiEnum.ParseEnum<ContinuousBenefitType>(continuousTypeStr);
					}
					if (string.IsNullOrEmpty(groupTypeStr) == false)
					{
						_groupType = CorgiEnum.ParseEnum<ContinuousGroupType>(groupTypeStr);
					}
					if (string.IsNullOrEmpty(groupTypeStr) == false)
					{
						_continuousUid = GameDataManager.GetUidByString(uidStr);
					}
					
				}
				catch (Exception e)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
					return false;
				}
	        }

            return true;
        }
		
		public override SkillCompLogNode CreateLogNode(Unit target)
		{
            var curLog = new ChangeContinuousDurationLogNode(this.Owner, target, this);

            curLog.ChangedDuration = _diffTime;
            
	        return curLog;
		}

        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
	        var thisLogNode = logNode as ChangeContinuousDurationLogNode;
	        if (thisLogNode == null )
	        {
		        return false;
	        }

	        if (target == null || target.ContinuousList == null)
	        {
		        return false;
	        }

	        foreach (var curSkillEffect in target.ContinuousList)
	        {
		        if (curSkillEffect == null)
		        {
			        continue;
		        }

		        if (_benefitType != ContinuousBenefitType.None && _benefitType != curSkillEffect.BenefitType)
		        {
			        // buff or debuff check
			        continue;
		        }

		        if (_groupType != ContinuousGroupType.None && _groupType != curSkillEffect.GroupType)
		        {
			        // group type
			        continue;
		        }

		        if (_continuousUid != 0 && _continuousUid != curSkillEffect.Uid)
		        {
			        // continue;
			        continue;
		        }

		        var preDuration = curSkillEffect.CurDuration;
		        curSkillEffect.ChangeDuration(_diffTime);
		        var postDuration = curSkillEffect.CurDuration;
		        
				thisLogNode.AddDetailLog($"ChangeContinuosDuration:HP({preDuration}) Change To ({postDuration}) by ({_diffTime}) in ({curSkillEffect.Name})");
		        
		        thisLogNode.ChangedContinuousIds.Add(curSkillEffect.ObjectId);
	        }


	        return true;

        }
        
    }
}
