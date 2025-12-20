using System.Collections.Generic;
using Corgi.GameData;

using IdleCs.Managers;
using IdleCs.Utils;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
    public class ContinuousSkillComp : SkillComp, ICorgiInterface<SkillContinuousInfoSpec>
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private SkillContinuousInfoSpec _spec;
	    
	    public ContinuousBenefitType BenefitType { get; private set; }

	    public SkillContinuousInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public override string GetName()
	    {
		    return _spec.Name;
		}
	    
	    public uint InitStack
	    {
		    get { return _spec.InitStack; }
	    }
	    public uint MaxStack
	    {
		    get { return _spec.MaxStack; }
	    }
	    public uint Duration
	    {
		    get { return _spec.Duration; }
	    }

	    public uint CoolTime
	    {
		    get { return _spec.CoolTime; }
	    }

	    public ContinuousSkillComp()
	    {
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
			var spec = Owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(uid);

			if(spec == null)
			{
				//Debug.LogError("invalid skill continuous code : " + code);
				CorgiLog.Assert(false);
				return false;
			}

			_spec = spec;

			BenefitType = CorgiEnum.ParseEnum<ContinuousBenefitType>(_spec.BenefitType);
			
		    return base.LoadInternal(uid);
	    }

	    public override SkillCompLogNode CreateLogNode(Unit target)
	    {
            return new AddContinuousLogNode(Owner, target, this);
	    }
	    
	    /// <summary>
	    /// Skill 이나 SkillEffectInst 에서 불려질수 있다(ISkillActor 상속자들
	    /// </summary>
	    /// <param name="target"></param>
	    /// <param name="logNode"></param>
		protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
	    {
			var curLog = logNode as AddContinuousLogNode;
			
			var isInsert = true;
			//var isEnter = false;
			
//            // Check Immune
//            if(target.ApplyImmune(this, curLog))
//            {
//                //inst.OnEvent(CombatEventType.OnImmune, new EventParamSkillOutput(inst), newNode);
//
//                return true;
//            }


			SkillEffectInst resultInst = null;
			List<SkillEffectInst> curList = null;
			
			curList = target.ContinuousList;

			// Check Apply & Stack
            foreach (var curInst in curList)
            {
                if (curInst.IsUniqueInUnit==false && curInst.Uid == Uid)
                {
	                if (target.ApplyImmune(curInst, curLog))
	                {
                        //target.OnEvent(CombatEventType.OnImmune, new EventParamAction(inst), (inst), newNode);
                        return true;
	                }
	                
					if (curInst.Stackable)
					{
						curInst.DoStack((int)InitStack);
						target.IsUpdatedEffect();
					}

					curInst.SetDurationBigger(Duration);

					isInsert = false;

					resultInst = curInst;
					
					curLog.AddDetailLog($"Stack Existed Continuous {curInst.StackCount+InitStack}/{curInst.MaxStack}:{Duration}");
					
					break;
                }
            }

            // Apply Insert
			if (isInsert)
            {
                resultInst = SkillEffectInstFactory.Create(this, Owner, target);

	            if (resultInst == null)
	            {
		            CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid uid for skilleffect. {0}", Uid);
		            return false;
	            }

	            if (target.ApplyImmune(resultInst, curLog))
	            {
		            return true;
	            }
                    
                resultInst.SetDuration(Duration);
                
				curLog.AddDetailLog($"Insert Continuous {resultInst.StackCount}/{resultInst.MaxStack}:{Duration}");
				
                target.ApplySkillEffect(resultInst, curLog);
                
                // run only first
				resultInst.OnDoApply(resultInst, curLog);
            }


			var eventParam = new EventParamEffect(resultInst);
			resultInst.OnEvent(CombatEventType.OnSkillEffectApply, eventParam, curLog);

			return true;
		}

		public bool DoApplyAura(Unit owner, SkillTargetComp targetComp, List<ConditionComp> conditionComps, CombatLogNode logNode)
	    {
			var curLog = CreateLogNode(owner) as AddContinuousLogNode;
			curLog.DungeonLogNode = logNode.DungeonLogNode;
			
			var isInsert = true;

			SkillEffectInst resultInst = null;
			List<SkillEffectInst> curList = null;
			
            // Apply Insert
			resultInst = SkillEffectInstFactory.CreateAura(this, Owner, targetComp, conditionComps);

			if (resultInst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid uid for skilleffect. {0}", Uid);
				return false;
			}

			resultInst.SetDuration(0);
			
			owner.ApplySkillEffect(resultInst, curLog);
			curLog.AddDetailLog($"Insert Continuous {resultInst.StackCount}/{resultInst.MaxStack}:{Duration}");
			
			logNode.AddChild(curLog);

			resultInst.OnDoApply(resultInst, curLog);


			return true;
		}
    }
}
