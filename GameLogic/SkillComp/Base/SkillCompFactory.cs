
using System;
using System.Collections.Generic;
using IdleCs;
using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
	public static class SkillCompFactory
	{
	    // Static Factory
		static readonly Dictionary<ActiveSkillCompType, Type> _activeSkillCompMap = new Dictionary<ActiveSkillCompType, Type>();
		static readonly Dictionary<PassiveSkillCompType, Type> _passiveSkillCompMap = new Dictionary<PassiveSkillCompType, Type>();
		
		
		public static void Init()
		{
			// active
			_activeSkillCompMap.Clear();
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageAtkFactor, typeof(DamageAtkFactorSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageDefFactor, typeof(DamageDefFactorSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageMaxHPPercent, typeof(DamageMaxHPPercentSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageCurHPPercent, typeof(DamageCurHPPercentSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageReducedHPPercent, typeof(DamageReducedHPPercentSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageMaxHPPercentWorldBoss, typeof(DamageMaxHPPercentWorldBossSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageSkillOutput, typeof(DamageSkillOutputSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageTransfered, typeof(DamageTransferedSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DamageTransferedDOT, typeof(DamageTransferedDOTSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.HealAtkFactor, typeof(HealAtkFactorSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.HealMaxHPPercent , typeof(HealMaxHPPercentSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.DrainDamageHPPercent, typeof(DrainDamageHPPercentSkillComp));
			
			
			_activeSkillCompMap.Add(ActiveSkillCompType.DispelBuff, typeof(DispelBuffSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DispelDebuff, typeof(DispelDebuffSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.DispelSelfContinuous, typeof(DispelSelfContinuousSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.AbsorbBuff, typeof(AbsorbBuffSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.AbsorbDebuff, typeof(AbsorbDebuffSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.ChangeContinuousDuration, typeof(ChangeContinuousDurationSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.RestoreMana, typeof(RestoreManaSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.ConsumeMana, typeof(ConsumeManaSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.AbsorbMana, typeof(AbsorbManaSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.RestoreNextMana, typeof(RestoreNextManaSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.DecreaseManaCostSelf, typeof(DecreaseManaCostSelfSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.Interrupt, typeof(InterruptSkillComp));
			_activeSkillCompMap.Add(ActiveSkillCompType.ChangeCurHPPercentSelf, typeof(ChangeCurHPPercentSelfSkillComp));
			
			_activeSkillCompMap.Add(ActiveSkillCompType.Summon, typeof(SummonSkillComp));
			
			// passive
			_passiveSkillCompMap.Clear();
			_passiveSkillCompMap.Add(PassiveSkillCompType.Stat, typeof(StatPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.Enhance, typeof(EnhancePassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.Mez, typeof(MezPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.Immune, typeof(ImmunePassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.BarrierDamage, typeof(BarrierDamageByFactorPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.BarrierDamageByTransferDamage, typeof(BarrierDamageByTransferDamagePassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.Taunt, typeof(TauntPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.SaveFromDeath, typeof(SaveFromDeathPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.TransferDamage, typeof(TransferDamagePassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.ConvertToTrueDamage, typeof(ConvertToTrueDamagePassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.Vampiric, typeof(VampiricPassiveComp));
			_passiveSkillCompMap.Add(PassiveSkillCompType.ReflectDamage, typeof(ReflectDamagePassiveComp));
		}
		
		public static ActiveSkillComp CreateActive(ulong uid, Unit owner, ISkillActor skillActor)
		{
			var sheet = owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);
			if (sheet == null)
			{
				return null;
			}
		    
			var activeType = CorgiEnum.ParseEnum<ActiveSkillCompType>(sheet.OnActiveType);

			if (_activeSkillCompMap.ContainsKey(activeType) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ActiveType {0}:{1}/{2}",  uid, activeType.ToString(), sheet.OnActiveType);
				return null;
			}
		    
			var type = _activeSkillCompMap[activeType];
			if (type == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Active SkillComp type : <0>", activeType.ToString());
				return null;
			}
			var inst = Activator.CreateInstance(type) as ActiveSkillComp;

			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : <0>", activeType.ToString());
				return null;
			}
			inst.SetDefault(owner, skillActor);

			if (inst.Load(uid) == false)
			{
				return null;
			}
			return inst;
		}

		public static ContinuousSkillComp CreateEffect(ulong uid, Unit owner, ISkillActor skillActor)
		{
			var inst = new ContinuousSkillComp();

			inst.SetDefault(owner, skillActor);
			if (inst.Load(uid))
			{
				return inst;
			}
			return null;
			
		}
		
		public static PassiveSkillComp CreatePassive(ulong uid, Unit owner, ISkillActor skillActor)
		{
			var sheet = owner.Dungeon.GameData.GetData<SkillPassiveInfoSpec>(uid);
			if (sheet == null)
			{
				return null;
			}

			try
			{
				var compType = CorgiEnum.ParseEnum<PassiveSkillCompType>(sheet.PassiveType);

				if (_passiveSkillCompMap.ContainsKey(compType) == false)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid PassiveType when create instance, {0}:{1}", sheet.Uid, sheet.PassiveType);
					 return null;
				}
				
				var type = _passiveSkillCompMap[compType];
				if (type == null)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid Passive SkillComp type : {0}:{1}", sheet.Uid, sheet.PassiveType);
					 return null;
				}
				var inst = Activator.CreateInstance(type) as PassiveSkillComp;

				if (inst == null)
				{
					 CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : {0}:{1}", sheet.Uid, sheet.PassiveType);
					 return null;
				}
				inst.SetDefault(owner, skillActor);

				if (inst.Load(uid) == false)
				{
					 return null;
				}
				return inst;

			}
			catch (Exception e)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Crash for Create Instance : {0}", sheet.PassiveType);
				throw;
			}
		}
	}
}
