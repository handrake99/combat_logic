using System;
using System.Collections.Generic;
using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class SkillEffectAuraInst : SkillEffectInst
    {
	    /// <summary>
	    /// Static Data
	    /// </summary>
	    private SkillTargetComp _targetComp = null;
	    private List<ConditionComp> _conditionComps = null;

	    /// <summary>
	    /// Dynamic Data
	    /// </summary>
	    private List<Unit> _targetList = null;
	    
	    
	    public SkillEffectAuraInst(Unit caster, ContinuousSkillComp skillComp, SkillTargetComp skillTargetComp, List<ConditionComp> conditionComps)
            : base(caster, skillComp, caster)
	    {
		    _targetComp = skillTargetComp;
		    _conditionComps = new List<ConditionComp>();
		    if (conditionComps != null)
		    {
				_conditionComps.AddRange(conditionComps);
		    }
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    EffectInstType = SkillEffectInstType.Aura;
		    
		    return base.LoadInternal(uid);

	    }
	    
		public override void DoSkillEffectAction(Unit target, Action<SkillEffectInst> action)
		{
			if (CheckActive(target))
			{
				action(this);
			}
		}
        
		public override void OnDoApply(SkillEffectInst inst, CombatLogNode logNode)
		{
			if (_targetComp == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"no target aura error.");
				return;
			}

			// check debug
			var skillActorLogNode = logNode.Parent as SkillActorLogNode;

			_targetList = _targetComp.GetTargetList(skillActorLogNode);

			if (_targetList == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Get target aura error in OnDoApply.");
				return;
			}
			
			base.OnDoApply(inst, logNode);

//			foreach (var target in _targetList)
//			{
//				// 다른 타겟에 붙여준다.
//				target.ApplyOtherAura(this, logNode);
//			}
		}
		public override void OnDoOver(CombatLogNode logNode)
		{
			if (_targetList == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Get target aura error in OnDoOver.");
				return;
				
			}
			
			base.OnDoOver(logNode);
			
//			foreach (var target in _targetList)
//			{
//				target.CancelOtherAura(this, logNode);
//			}
		}

		public bool CheckActive(Unit target)
		{
			if (target == null)
			{
				return false;
			}

			var isActive = false;
			foreach (var unit in _targetList)
			{
				if (target.ObjectId == unit.ObjectId)
				{
					isActive = true;
					break;
				}
			}

			if (isActive == false)
			{
				return false;
			}

			return true;
		}

//		public override bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
//		{
//		}
    }
}