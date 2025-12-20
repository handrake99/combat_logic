using System.Collections.Generic;

using IdleCs.Utils;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    [System.Serializable]
    public class FollowingSkillComp : CorgiObject
    {
	    private Unit _owner;
	    private ISkillActor _skillActor;
	    //private SkillCompType _skillCompType;
	    
	    /// <summary>
	    /// Active Components
	    /// </summary>
        private List<ActiveSkillCompInfo> _activeList = new List<ActiveSkillCompInfo>();
        private List<EffectSkillCompInfo> _effectList = new List<EffectSkillCompInfo>();
		    
        public FollowingSkillComp()
        {
        }
	    
	    public bool Init(Unit owner, ISkillActor actor)
	    {
		    _owner = owner;
		    _skillActor = actor;

		    return true;
	    }
        
	    
		protected override bool LoadInternal(ulong uid)
		{
//			var thisAsset = asset as FollowingAsset;
//			if (thisAsset == null)
//			{
//				return false;
//			}
//
//			_skillCompType = thisAsset.SkillCompType;
//			
//            foreach (var assetStruct in thisAsset.ActiveList)
//            {
//	            var skillCompStruct = new ActiveSkillCompStruct();
//	            if (skillCompStruct.Init(_owner, _skillActor, assetStruct) == false)
//	            {
//		            continue;
//	            }
//	            
//                _activeList.Add(skillCompStruct);
//            }
//            
//            foreach (var assetStruct in thisAsset.EffectList)
//            {
//	            var skillCompStruct = new EffectSkillCompStruct();
//	            if (skillCompStruct.Init(_owner, _skillActor, assetStruct) == false)
//	            {
//		            continue;
//	            }
//	            
//                _effectList.Add(skillCompStruct);
//            }
				
			return base.LoadInternal(uid);
		}

	    public virtual bool DoApply(Unit target, CombatLogNode skillCompLogNode, CombatLogNode logNode)
	    {
		    return false;
	    }

	    protected bool DoApplyInner(Unit target, CombatLogNode logNode)
	    {
			foreach (var activeCompStruct in _activeList)
			{
				var targetComp = activeCompStruct.TargetComp;
				//var condition = activeCompStruct.Condition;
				var activeComp = activeCompStruct.SkillComp;
				//FollowingSkillComp followingComp = null; //activeCompStruct.FollowingSkillComp;
				
				if (targetComp == null || activeComp == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ActiveSkillComp Struct ");
					continue;
				}

				var targetList = targetComp.GetTargetList(logNode);

				if (targetList == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ActiveSkillComp & Target: " );
					continue;
				}

				activeComp.DoApply(targetList, null, logNode);
				
			}
		    
			foreach (var effectCompStruct in _effectList)
			{
				var targetComp = effectCompStruct.TargetComp;
				//var condition = effectCompStruct.Condition;
				var skillComp = effectCompStruct.SkillComp;
				//FollowingSkillComp followingComp = null; //effectCompStruct.FollowingSkillComp;

				if (targetComp == null || skillComp == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid EffectSkillComp Struct");
					continue;
				}

				var targetList = targetComp.GetTargetList(logNode);

				if (targetList == null)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ActiveSkillComp & Target: " );
					continue;
				}
				
				skillComp.DoApply(targetList, null, logNode);
			}

		    return true;
	    }
        
    }
}