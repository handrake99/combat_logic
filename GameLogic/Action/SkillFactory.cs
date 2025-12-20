using System;
using System.Collections.Generic;
using Corgi.DBSchema;
using Corgi.GameData;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public static class SkillFactory
	{
		private static readonly Dictionary<SkillType, Type> _skillMap = new Dictionary<SkillType, Type>();

		public static void Init()
		{
			_skillMap.Clear();
			_skillMap.Add(SkillType.Attack, typeof(SkillAttack));
			_skillMap.Add(SkillType.Active, typeof(SkillActive));
			_skillMap.Add(SkillType.Passive, typeof(SkillPassive));
			_skillMap.Add(SkillType.Casting, typeof(SkillCasting));
			_skillMap.Add(SkillType.Channeling, typeof(SkillChanneling));
			_skillMap.Add(SkillType.Relic, typeof(SkillRelic));
			_skillMap.Add(SkillType.External, typeof(SkillExternal));
		}

		public static Skill Create(DBSkill dbSkill, Unit owner, SkillSlot skillSlot = null)
		{
			if (dbSkill == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid db skill : null");
				return null;
			}

			var skillItemInfo = owner.Dungeon.GameData.GetData<SkillItemSpec>(dbSkill.Uid);

			if (skillItemInfo == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skill item : {0}", dbSkill.Uid);
				return null;
			}

			var inst = CreateInstance(skillItemInfo.SkillUid, owner);
			
			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skill info : {0}\n", skillItemInfo.SkillUid);
				return null;
			}
			
			inst.Init(owner);
			
			if (skillSlot != null)
			{
				inst.SetMastery(skillSlot.Mastery);
			}

			if (inst.Load(dbSkill) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"failed to load skill : {0}\n", skillItemInfo.SkillUid);
				return null;
			}

			return inst;
		}
		public static Skill Create(SharedSkillInfo skillInfo, Unit owner, SkillSlot skillSlot = null)
		{
			if (skillInfo == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skill info : null\n");
				return null;
			}
			
			var skillItemSheet = owner.Dungeon.GameData.GetData<SkillItemSpec>(skillInfo.uid);
			
			if (skillItemSheet == null)
			{
				return null;
			}

			var inst = CreateInstance(skillItemSheet.SkillUid, owner);

			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skill info : {0}\n", skillItemSheet.SkillUid);
				return null;
			}
			
			inst.Init(owner);
			
			if (skillSlot != null)
			{
				inst.SetMastery(skillSlot.Mastery);
			}
			
			if (inst.Load(skillInfo) == false)
			{
				return null;
			}

			return inst;
		}

		public static Skill Create(ulong uid, Unit owner)
		{
			var inst = CreateInstance(uid, owner);

			if (inst == null)
			{
				return null;
			}

			inst.Init(owner);

			if (inst.Load(uid) == false)
			{
				return null;
			}

			return inst;
		}

		static Skill CreateInstance(ulong uid, Unit owner)
		{
			var sheet = owner.Dungeon.GameData.GetData<SkillInfoSpec>(uid);
			if (sheet == null)
			{
				var uidStr = GameDataManager.GetStringByUid(uid);
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skill info : {0} for {1}", uidStr, owner.Name);
				return null;
			}
			
			var skillType = CorgiEnum.ParseEnum<SkillType>(sheet.SkillType);
			
			if (_skillMap.ContainsKey(skillType) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid SkillType when create instance, {0}", sheet.SkillType);
				return null;
			}
		    
			var type = _skillMap[skillType];
			if (type == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid skillType SkillComp type : <0>", sheet.SkillType);
				return null;
			}
			
			var inst = Activator.CreateInstance(type) as Skill;
			if (inst == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Failed Create Instance : <0>", sheet.SkillType);
				return null;
			}

			return inst;
		}
	}
}
