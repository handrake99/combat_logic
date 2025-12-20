using System;

using IdleCs.GameLog;
using IdleCs.Utils;

//
//
namespace IdleCs.GameLogic
{
    public abstract class DamageSkillComp : ActiveSkillComp
    {
	    /// <summary>
	    /// Static
	    /// </summary>
	    ///
	    protected bool ForceHit;
	    protected bool IgnoreDefence;

        public DamageSkillComp()
        {
	        ForceHit = false;
	        IgnoreDefence = false; 
	        IgnoreCritical = false;
	        IgnoreEvent = false;
        }

        protected override bool LoadInternal(ulong uid)
        {
	        CanAreaEffect = true;
            return base.LoadInternal(uid);
        }

        protected bool DoApplyDamage(DamageSkillCompLogNode dmgNode)
        {
	        return DoApplyDamage(Owner, this, dmgNode);
        }

        public static bool DoApplyDamage(Unit owner, DamageSkillComp skillComp, DamageSkillCompLogNode dmgNode)
        {
	        if (dmgNode == null)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DoApplyDamage Called");
		        return false;
	        }
	        
	        var dmgEventParam = new EventParamSkillComp(owner.Dungeon, skillComp, dmgNode);
	        
			var targetId = dmgNode.TargetId;
            var casterId = dmgNode.CasterId;
            var target = owner.Dungeon.GetUnit(targetId);
            var caster = owner.Dungeon.GetUnit(casterId);
            if (target == null || caster == null)
            {
	            return false;
            }

			if(target.IsLive() == false)
			{
				//CorgiLog.Assert(false);
				return false;
			}

			var preHPRate = (float)target.CurHP/(float)target.MaxHP;
			
			// apply pre feature
			if (skillComp != null)
			{
				skillComp.DoApplyPreFeature(dmgNode);
			}
			
            // check hit / evasion
            var isHit = target.ApplyHit(caster, dmgNode);
            if (isHit == false)
            {
	            dmgNode.Damage = 0f;
	            return true;
            }
			
			
			// check Immune todo
			if(target.ApplyImmune(skillComp, dmgNode) )
			{
				dmgNode.IsImmune = true;

				return true;
			}
			
			if(dmgNode.IgnoreEvent == false)
			{
				target.OnEvent(CombatEventType.OnBeingPreHit, dmgEventParam, dmgNode);
			}
			else
			{
                dmgNode.AddDetailLog($"Ignored Event by Feature");
			}
			
            
	        if (caster.ApplyCritical(target, dmgNode))
	        {
	        }
			
			// target enhance
			// if(dmgNode.IgnoreEnhance == false)
			// {
			// 	target.ApplyEnhance(new EnhanceType[]{EnhanceType.IncomingDamage}, dmgNode);
			// }
			

            if (skillComp != null)
            {
				skillComp.DoApplyPostFeature(dmgNode);
            }
	        
			//CorgiLog.LogLine("caculated final damage : " + dmgNode.Damage);


			// check low damage
			//if(logNode.Damage< 1.0f && output.IsZero == false)
	        //if(dmgNode.Damage < 1.0f)
	        //{
		        //dmgNode.Damage = 1.0f;
	        //}
	        
			//CorgiLog.LogLine("[Unit {0}] final damage : {1}" , Owner.CharName, dmgNode.FinalDamage);

            target.OnEvent(CombatEventType.OnBeingHitAlways, dmgEventParam, dmgNode);
			if(dmgNode.IgnoreEvent == false)
			{
                target.OnEvent(CombatEventType.OnBeingHit , dmgEventParam, dmgNode);
                caster.OnEvent(CombatEventType.OnActiveHit, dmgEventParam, dmgNode);

				if(dmgNode.IsCritical)
				{
                    caster.OnEvent(CombatEventType.OnCriticalHit, dmgEventParam, dmgNode);
				}
			}

			owner.Dungeon.ApplyDungeonFeature(dmgNode);
			
			dmgNode.PreHP = target.CurHP;
            target.ApplyDamage((long)dmgNode.FinalDamage);
			dmgNode.CurHP = target.CurHP;
            dmgNode.AddDetailLog($"FinalDamage : {dmgNode.FinalDamage}");
            dmgNode.AddDetailLog($"PreHP: {dmgNode.PreHP}, CurHP : {dmgNode.CurHP}");
            
			if(dmgNode.IgnoreEvent == false)
			{
                caster.OnEvent(CombatEventType.OnActivePostHit, dmgEventParam, dmgNode);
                target.OnEvent(CombatEventType.OnBeingPostHit , dmgEventParam, dmgNode);
			}
	        
            if (target.IsLive() == false)
            {
                var unitEventParam = new EventParamUnit(target);
                target.OnEvent(CombatEventType.OnDead, unitEventParam, dmgNode);
                
                if(target.IsLive() == false) // 여전히 죽어 있으면
                {
                    caster.OnEvent(CombatEventType.OnKill, unitEventParam, dmgNode);
					target.OnEvent(CombatEventType.OnDeadCompletely, unitEventParam, dmgNode);
                }
            }
            else
            {
                EventParamUnit unitParam = new EventParamUnit(target);

                if(target.IsNearDeath(0.1f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath10, unitParam, dmgNode);
                } else if(target.IsNearDeath(0.2f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath20, unitParam, dmgNode);
                } else if(target.IsNearDeath(0.25f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath25, unitParam, dmgNode);
                } else if(target.IsNearDeath(0.3f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath30, unitParam, dmgNode);
                } else if(target.IsNearDeath(0.4f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath40, unitParam, dmgNode);
                } else if(target.IsNearDeath(0.5f) == true)
                {
                    target.OnEvent(CombatEventType.OnNearDeath50, unitParam, dmgNode);
                }

                var count = target.IsDamageHPPercent(preHPRate, 0.10f);
                for (int i = 0; i < count; ++i)
                {
	                target.OnEvent(CombatEventType.OnDamagedHP10, unitParam, dmgNode);
                }
                count = target.IsDamageHPPercent(preHPRate, 0.20f);
                for (int i = 0; i < count; ++i)
                {
	                target.OnEvent(CombatEventType.OnDamagedHP20, unitParam, dmgNode);
                }
                count = target.IsDamageHPPercent(preHPRate, 0.25f);
                for (int i = 0; i < count; ++i)
                {
	                target.OnEvent(CombatEventType.OnDamagedHP25, unitParam, dmgNode);
                }
                count = target.IsDamageHPPercent(preHPRate, 0.40f);
                for (int i = 0; i < count; ++i)
                {
	                target.OnEvent(CombatEventType.OnDamagedHP40, unitParam, dmgNode);
                }
                count = target.IsDamageHPPercent(preHPRate, 0.50f);
                for (int i = 0; i < count; ++i)
                {
	                target.OnEvent(CombatEventType.OnDamagedHP50, unitParam, dmgNode);
                }


            }
            
			CorgiCombatLog.Log(CombatLogCategory.Skill,"{0} Damage {1} to {2}", caster.Name, dmgNode.FinalDamage, target.Name);

	        return true;
        }

        protected void ApplyDamageEnhance(Unit target, DamageSkillCompLogNode dmgNode)
        {
            var caster = Owner;
	        
            if (dmgNode.IgnoreEnhance == false)
            {
	            EnhanceType[] enhanceTypes;

	            var actor = this.ParentActor;

	            if (actor.SkillActorType == SkillActorType.SkillContinuous)
	            {
		            enhanceTypes = new EnhanceType[] {EnhanceType.Damage, EnhanceType.SkillEffectDamage};
	            }else
	            {
		            var skillActor = actor as Skill;
		            if (skillActor == null)
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.Damage};
		            }
		            else if(skillActor.SkillType == SkillType.Attack)
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.Damage, EnhanceType.AttackDamage};
		            }
		            else
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.Damage, EnhanceType.SkillDamage};
		            }
	            }
	            
                caster.ApplyEnhance(enhanceTypes, dmgNode);
            }
            else
            {
                dmgNode.AddDetailLog($"Ignored Enhance by Feature");
            }
            
            if (dmgNode.IgnoreEnhance == false )
            {
	            EnhanceType[] enhanceTypes;

	            var actor = this.ParentActor;

	            if (actor.SkillActorType == SkillActorType.SkillContinuous)
	            {
		            enhanceTypes = new EnhanceType[] {EnhanceType.IncomingDamage, EnhanceType.IncomingSkillEffectDamage};
	            }else
	            {
		            var skillActor = actor as Skill;
		            if (skillActor == null)
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.IncomingDamage};
		            }
		            else if(skillActor.SkillType == SkillType.Attack)
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.IncomingDamage, EnhanceType.IncomingAttackDamage};
		            }
		            else
		            {
						enhanceTypes = new EnhanceType[] {EnhanceType.IncomingDamage, EnhanceType.IncomingSkillDamage};
		            }
	            }
	            
                target.ApplyEnhance(enhanceTypes, dmgNode);
            }
	        
        }
        
	    
        protected float GetDamageVariation(float damage)
        {
            var minRate = 1f - CorgiLogicConst.CombatVariationRate;
            var maxRate = 1f + CorgiLogicConst.CombatVariationRate;

            var retDamage = Owner.Dungeon.Random.Range(damage * minRate, damage * maxRate);

            if (retDamage < 1)
            {
                return 1f;
            }

            return retDamage;
        }

    }
}
