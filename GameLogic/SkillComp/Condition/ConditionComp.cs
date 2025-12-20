
using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.Utils;
using IdleCs.GameLog;
using IdleCs.Managers;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
	[System.Serializable]
	public abstract class ConditionComp : CorgiObject
	{
		private Unit _owner;
		private SkillConditionInfoSpec _spec;

		private SkillTargetType _criteriaType;
		

		public Unit Owner
		{
			get { return _owner; }
		}

		public string Desc
		{
			get { return _spec.Desc; }
		}

		public bool Countable
		{
			get { return _spec.Countable; }
		}

		public ConditionCaseType CaseType { get; set; }

		public SkillConditionInfoSpec GetSpec()
		{
			return _spec;
		}

		protected ConditionComp()
		{
		}

		protected override bool LoadInternal(ulong uid)
		{
			var sheet = _owner.Dungeon.GameData.GetData<SkillConditionInfoSpec>(uid);
			if (sheet == null)
			{
				return false;
			}

			_spec = sheet;
			_criteriaType = CorgiEnum.ParseEnum<SkillTargetType>(sheet.CriteriaType);
			
			return base.LoadInternal(uid);
		}

		public void SetDefault(Unit owner)
	    {
		    if (owner == null)
		    {
				CorgiLog.Assert(false);
				return;
		    }
		    _owner = owner;
	    }

		public static int CheckActive(List<ConditionComp> conditionComps, SkillActionLogNode logNode)
		{
			var retActive = 0;
			foreach (var curCondition in conditionComps)
			{
				if (curCondition == null)
				{
					continue;
				}
				var curActive = curCondition.CheckActive(logNode);

				if (curActive == 0)
				{
					if (curCondition.CaseType == ConditionCaseType.CaseAnd)
					{
						return 0;
					}
				}
				if (curActive > retActive)
				{
					retActive = curActive;
				}
			}

			return retActive;
			
		}

		public static int CheckActive(List<ConditionComp> conditionComps, SkillCompLogNode logNode)
		{
			var retActive = 0;
			foreach (var curCondition in conditionComps)
			{
				if (curCondition == null)
				{
					continue;
				}
				var curActive = curCondition.CheckActive(logNode);

				if (curActive == 0)
				{
					if (curCondition.CaseType == ConditionCaseType.CaseAnd)
					{
						return 0;
					}
				}
				if (curActive > retActive)
				{
					retActive = curActive;
				}
			}

			return retActive;
		}
		
		public static int CheckActive(List<ConditionComp> conditionComps, SkillEventLogNode logNode)
		{
			var retActive = 0;
			foreach (var curCondition in conditionComps)
			{
				if (curCondition == null)
				{
					continue;
				}
				var curActive = curCondition.CheckActive(logNode);

				if (curActive == 0)
				{
					if (curCondition.CaseType == ConditionCaseType.CaseAnd)
					{
						return 0;
					}
				}
				if (curActive > retActive)
				{
					retActive = curActive;
				}
			}

			return retActive;
		}

	    /// <summary>
	    /// Only 1 Target
	    /// </summary>
	    /// <param name="logNode"></param>
	    /// <returns>
	    /// 0 : false
	    /// 1 : true
	    /// 2~ : iterate
	    /// </returns>
	    public virtual int CheckActive(SkillActionLogNode logNode)
	    {
		    return 0;
	    }
	    public virtual int CheckActive(SkillCompLogNode logNode)
	    {
		    return 0;
	    }
	    
	    public virtual int CheckActive(SkillEventLogNode logNode)
	    {
		    return 0;
	    }

		protected Unit GetUnit(Unit owner, Unit target, SkillCompLogNode logNode)
		{
			Unit unit = null;
			Dungeon dungeon = owner.Dungeon;
			
            if(_criteriaType == SkillTargetType.Self)
            {
                unit = owner;

            }else if(_criteriaType == SkillTargetType.Caster)
            {
	            unit = dungeon.GetUnit(logNode.CasterId);
			}else if(_criteriaType == SkillTargetType.Target)
			{
				unit = target;
            }else if(_criteriaType == SkillTargetType.Attacker)
            {
	            unit = dungeon.GetUnit(logNode.CasterId);
            }

			return unit;
		}
		
		protected Unit GetUnit(Unit owner, Unit target, SkillEventLogNode logNode)
		{
			Unit unit = null;
			Dungeon dungeon = owner.Dungeon;
			
            if(_criteriaType == SkillTargetType.Self)
            {
                unit = owner;

            }else if(_criteriaType == SkillTargetType.Caster)
            {
	            unit = dungeon.GetUnit(logNode.CasterId);
			}else if(_criteriaType == SkillTargetType.Target)
			{
				unit = target;
            }else if(_criteriaType == SkillTargetType.Attacker)
            {
	            unit = dungeon.GetUnit(logNode.CasterId);
            }

			return unit;
		}
		
		protected int GetRetValue(int count)
		{
			if (count <= 0)
			{
				return 0;
			}

			if (Countable)
			{
				return count;
			}

			return 1;
		}

		protected TEnum ParseParam<TEnum>(JObject json, string key) where TEnum : struct, IConvertible
		{
			if (CorgiJson.IsValidString(json, key) == false)
			{
				return default(TEnum);
			}
			
			var typeStr = CorgiJson.ParseString(json, key);
			if (string.IsNullOrEmpty(typeStr))
			{
				return default(TEnum);
			}

			return CorgiEnum.ParseEnum<TEnum>(typeStr);
		}
		
		protected TEnum ParseParamPascal<TEnum>(JObject json, string key) where TEnum : struct, IConvertible
		{
			if (CorgiJson.IsValidString(json, key) == false)
			{
				return default(TEnum);
			}
			
			var typeStr = CorgiJson.ParseString(json, key);
			if (string.IsNullOrEmpty(typeStr))
			{
				return default(TEnum);
			}

			var pascalStr = CorgiString.PascalCase(typeStr);

			return CorgiEnum.ParseEnum<TEnum>(pascalStr);
		}


	}
}
