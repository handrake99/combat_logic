using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class SkillFeatureComp : CorgiObject
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private Unit _owner;
	    private SkillFeatureInfoSpec _spec;
	    
		private SkillFeatureType _featureType;

	    public Unit Owner
	    {
		    get { return _owner; }
	    }

	    public SkillFeatureInfoSpec GetSpec()
	    {
		    return _spec;
	    }

	    public SkillFeatureType FeatureType => _featureType;

		public SkillFeatureComp()
		{
		}

		protected override bool LoadInternal(ulong uid)
		{
			var sheet = _owner.Dungeon.GameData.GetData<SkillFeatureInfoSpec>(uid);

			if (sheet == null)
			{
				return false;
			}

			_spec = sheet;

			_featureType = CorgiEnum.ParseEnum<SkillFeatureType>(sheet.FeatureType);
				
			return true;
		}

	    public bool SetDefault(Unit owner)
	    {
		    _owner = owner;

		    return true;
	    }
		
		public virtual bool DoApplyPre(ActiveSkillCompLogNode logNode)
		{
			return false;
		}
		public virtual bool DoApplyPost(ActiveSkillCompLogNode logNode)
		{
			return false;
		}

	}

	public class SkillFeatureCompInfo
	{
		public SkillFeatureComp FeatureComp;
		public List<ConditionComp> ConditionComps; 

		public bool Init(Unit owner, ulong featureUid, ulong conditionUid)
		{
			FeatureComp = SkillFeatureCompFactory.Create(featureUid, owner);

			if (conditionUid != 0)
			{
				ConditionComps = SkillConditionCompFactory.Create(conditionUid, owner);
			}

			if (FeatureComp == null)
			{
				return false;
			}
			
			return true;
		}

		public bool DoApplyPre(ActiveSkillCompLogNode logNode)
		{
			if (ConditionComps != null && ConditionComps.Count > 0)
			{
				var activeCount = ConditionComp.CheckActive(ConditionComps, logNode);

				if (activeCount == 0)
				{
					 return false;
				}
			}

			return FeatureComp.DoApplyPre(logNode);
		}
		public bool DoApplyPost(ActiveSkillCompLogNode logNode)
		{
			if (ConditionComps != null && ConditionComps.Count > 0)
			{
				var activeCount = ConditionComp.CheckActive(ConditionComps, logNode);

				if (activeCount == 0)
				{
					 return false;
				}
			}

			return FeatureComp.DoApplyPost(logNode);
		}
		
		public static SkillFeatureCompInfo CreateOnFeature(Unit owner, ulong featureUid, ulong conUid)
		{
			if (featureUid == 0)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid OnFeature Json");
				return null;
			}
		
			 var skillFeatureInfo = new SkillFeatureCompInfo();

			 if (skillFeatureInfo.Init(owner, featureUid, conUid) == false)
			 {
				 CorgiCombatLog.LogError(CombatLogCategory.Skill,"failed to init skill feature info {0}", featureUid);
				 
				 return null;
			 }
			
			return skillFeatureInfo;
		}
	}

}