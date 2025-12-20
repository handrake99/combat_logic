using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public static class SkillFeatureCompFactory
	{
        // Static Factory
        static Dictionary<SkillFeatureType, Type> _featureMap
            = new Dictionary<SkillFeatureType, Type>();
	    
		public static void Init()
		{
			_featureMap.Add(SkillFeatureType.AmplifyingOutput, typeof(AmplifyDamageSkillFeatureComp));
			_featureMap.Add(SkillFeatureType.Enhance, typeof(EnhanceSkillFeatureComp));
			
			_featureMap.Add(SkillFeatureType.Ignore, typeof(IgnoreSkillFeatureComp));
			_featureMap.Add(SkillFeatureType.Force, typeof(ForceSkillFeatureComp));
			
			_featureMap.Add(SkillFeatureType.Drain, typeof(DrainSkillFeatureComp));
		}

        public static SkillFeatureComp Create(ulong featureUid, Unit owner)
        {
	        var uid = featureUid;
			var sheet = owner.Dungeon.GameData.GetData<SkillFeatureInfoSpec>(uid);
			if (sheet == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Feature uid : {0}", uid);
				return null;
			}
			
			var featureType = CorgiEnum.ParseEnum<SkillFeatureType>(sheet.FeatureType);

			if (_featureMap.ContainsKey(featureType) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid FeatureType {0}:{1}/{2}"
					,  featureUid, featureType.ToString(), sheet.FeatureType);
				return null;
			}
		    
			var type = _featureMap[featureType];
			if (type == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Feature SkillComp type : {0}", featureType.ToString());
				return null;
			}
			var inst = Activator.CreateInstance(type) as SkillFeatureComp;

			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}", featureType.ToString());
				return null;
			}

			inst.SetDefault(owner);

			if (inst.Load(uid) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Load Feature : {0}", featureUid);
				return null;
			}
			return inst;
	        
        }
	}
}