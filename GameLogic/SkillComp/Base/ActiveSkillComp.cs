using System;
using System.Collections.Generic;
using Corgi.GameData;

using IdleCs.GameLog;

using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public abstract class ActiveSkillComp : SkillComp, ICorgiInterface<SkillActiveInfoSpec>
    {
	    private SkillActiveInfoSpec _spec;
	    
	    private List<SkillFeatureCompInfo> _featureList = new List<SkillFeatureCompInfo>();

	    public SkillActiveInfoSpec GetSpec()
	    {
		    return _spec;
	    }

	    public override string GetName()
	    {
		    return _spec.Name;
		}
		
	    protected List<SkillFeatureCompInfo> FeatureList
	    {
		    get { return _featureList; }
	    }

	    private long _baseAmount;
	    private float _baseFactor;

	    public long BaseAmount
	    {
		    get
		    {
			    if (ParentActor == null || Stackable == false)
			    {
				    return _baseAmount;
			    }
			    return _baseAmount * ParentActor.StackCount;
		    }
	    }
	    
	    public float BaseFactor
	    {
		    get
		    {
			    if (ParentActor == null || Stackable == false)
			    {
				    return _baseFactor;
			    }
			    return _baseFactor * ParentActor.StackCount;
		    }
	    }
	    
	    public bool Stackable
	    {
		    get { return _spec.Stackable; }
	    }

	    public float LevelModifier
	    {
		    get { return _spec.LevelModifier; }
	    }

	    public bool CanAreaEffect
	    {
		    get; protected set;
	    }
	    
	    
	    public ActiveSkillComp()
	    {
	    }
	    
		protected override bool LoadInternal(ulong uid)
		{
			var spec = Owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);    
			if (spec == null)
			{
				return false;
			}

			_spec = spec;

			TargetAliveState = UnitAliveState.Alive;

			var constValue = Owner.Dungeon.GameData.GetConfigNumber("config.combat.factor.skill.active", 500);
			_baseAmount = CalcBaseAmount(_spec.BaseAmount, _spec.LevelModifier, ParentActor.Level, constValue);

			var increasePercent =
				Owner.Dungeon.GameData.GetConfigFloat("config.skill_slot.mastery.increase_percent", 0.01f);
			var masteryPerLevel = Owner.Dungeon.GameData.GetConfigNumber("config.skill_slot.mastery.level", 100);
			_baseFactor = CalcBasePercent(_spec.ApFactor, Mastery, ParentActor.Level, increasePercent, masteryPerLevel);

		    try
		    {
			    var onFeatures = _spec.OnFeature;
				
				foreach (var curData in onFeatures)
				{ 
					var featureUid = curData.FeatureCompUid;
					var conditionUid = curData.ConditionCompUid;

					var featureInfo = SkillFeatureCompInfo.CreateOnFeature(Owner, featureUid, conditionUid);

					if (featureInfo != null)
				    {
					    _featureList.Add(featureInfo);
				    }
				}
		    }
		    catch (Exception e)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid onActive {0}", spec.Name);
			    return false;
		    }

			return base.LoadInternal(uid);
		}


		protected void DoApplyPreFeature(ActiveSkillCompLogNode logNode)
		{
			if (logNode.IsFeatured)
			{
				return;
			}
			foreach(var curFeatureInfo in FeatureList)
			{
				if (curFeatureInfo == null)
				{
					continue;
				}

				if (curFeatureInfo.DoApplyPre(logNode) == true)
				{
					logNode.AddDetailLog($"Applied Pre Feature:{curFeatureInfo.FeatureComp.FeatureType}");
					logNode.IsFeatured = true;
				}
			}
		}
		
		protected void DoApplyPostFeature(ActiveSkillCompLogNode logNode)
		{
			foreach(var curFeatureInfo in FeatureList)
			{
				if (curFeatureInfo == null)
				{
					continue;
				}

				if (curFeatureInfo.DoApplyPost(logNode) == true)
				{
					logNode.AddDetailLog($"Applied Post Feature:{curFeatureInfo.FeatureComp.FeatureType}");
					logNode.IsFeatured = true;
				}
			}
		}

	    public virtual bool DoApplyEffect(Unit target, CombatLogNode logNode)
	    {
		    return false;
	    }
	    
    }
}
