using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.Library;

using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
	public static class SkillEffectInstFactory
	{
	    // Static Factory
		public static SkillEffectInst Create(ContinuousSkillComp skillComp, Unit owner, Unit target)
		{
			var uid = skillComp.Uid;
			var spec = owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(uid);
			if (spec == null)
			{
				return null;
			}

			// 그외 전부
			var inst = new SkillEffectStandard(owner, skillComp, target);
			if (inst.Load(uid) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}", uid);
				return null;
				   
			}

			return inst;
		}
		
		public static SkillEffectInst CreateAura(ContinuousSkillComp skillComp, Unit owner, SkillTargetComp targetcomp, List<ConditionComp> conditionComps)
		{
			var uid = skillComp.Uid;
			var spec = owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(uid);
			if (spec == null)
			{
				return null;
			}

			if (skillComp.ParentActor.SkillType == SkillType.Relic)
			{
				var inst = new SkillEffectInstRelic(owner, skillComp, targetcomp, conditionComps);
				if (inst.Load(uid) == false)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}", uid);
					 return null;
					 
				}

				return inst;
			}
			else
			{
				var inst = new SkillEffectAuraInst(owner, skillComp, targetcomp, conditionComps);
				if (inst.Load(uid) == false)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}", uid);
					 return null;
					 
				}

				return inst;
			}
		}
	}
}
