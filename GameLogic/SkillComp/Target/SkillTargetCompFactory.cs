using System;
using System.Collections.Generic;

using IdleCs.Utils;


namespace IdleCs.GameLogic
{
	public static class SkillTargetCompFactory
	{
		public static Dictionary<SkillTargetType, Type> _skillTargetMap = new Dictionary<SkillTargetType, Type>();

		public static void Init()
		{
			_skillTargetMap.Add(SkillTargetType.Self, typeof(SelfSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.Caster, typeof(CasterSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.Target, typeof(TargetSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.TargetNearestSecond, typeof(TargetNearestSecondSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.TargetNearestThird, typeof(TargetNearestThirdSkillTargetComp));
			
			_skillTargetMap.Add(SkillTargetType.FriendAll, typeof(FriendAllSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.FriendTargetNearest, typeof(FriendTargetNearestSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.FriendAllButSelf, typeof(FriendAllButSelfSkillTargetComp));
			
			_skillTargetMap.Add(SkillTargetType.EnemyAll, typeof(EnemyAllSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.EnemyAllButTarget, typeof(EnemyAllButTargetSkillTargetComp));
			
			_skillTargetMap.Add(SkillTargetType.FriendLowestHP, typeof(FriendLowestHPSkillTargetComp));
			_skillTargetMap.Add(SkillTargetType.FriendTwoLowestHP, typeof(FriendTwoLowestHPSkillTargetComp));
			
			_skillTargetMap.Add(SkillTargetType.RandomEnemy, typeof(RandomEnemySkillTargetComp));
			
			_skillTargetMap.Add(SkillTargetType.Summoner, typeof(SummonerSkillTargetComp));
		}

        public static SkillTargetComp Create(Unit owner, SkillTargetType targetType)
        {
	        if (_skillTargetMap.ContainsKey(targetType) == false)
	        {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Target type : {0}\n", targetType.ToString());
			    return null;
	        }
	        
		    var type = _skillTargetMap[targetType];
	        
		    if (type == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Target type : {0}\n", targetType.ToString());
			    return null;
		    }
		    var inst = Activator.CreateInstance(type) as SkillTargetComp;

		    if (inst == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}", targetType.ToString());
			    return null;
		    }

	        if (inst.Init(owner) == false)
	        {
		        return null;
	        }

		    return inst;
        }
	}
}
