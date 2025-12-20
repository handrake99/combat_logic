using System;
using System.Collections.Generic;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class TauntPassiveComp : PassiveSkillComp 
	{
		TauntType 	_tauntType = TauntType.Target;
		Unit 		_tauntTarget;

		public TauntType TauntType { get {return _tauntType; }}
		public Unit TauntTarget { get {return _tauntTarget; }}

		
		public TauntPassiveComp()
		{
			PassiveType = PassiveSkillCompType.Taunt;
			EventManager.Register(CombatEventType.OnSkillEffectEnter, OnSkillEffectEnter);
		}
		
		
//		protected override bool LoadPassive(StatusEffectType effectType, StatusEffectSubType subType, float value1, float value2)
//		{
//			if (effectType != StatusEffectType.SET_Taunt)
//			{
//				CorgiLog.LogError("Failed to load TauntPassiveComp : " + effectType + " / " + subType + " / " + value1 + " / " + value2);
//				return false;
//			}
//			
//			switch (subType)
//			{
//				case StatusEffectSubType.SEST_TauntTarget:
//					_tauntType = TauntType.Target;
//					break;
//				default:
//					CorgiLog.LogError("Failed to load TauntPassiveComp : " + effectType + " / " + subType + " / " + value1 + " / " + value2);
//					return false;
//			}
//		    
//			return true;
//		}

		bool OnSkillEffectEnter(EventParam eventParam, CombatLogNode logNode)
		{
			//DebugUtils.Log("taunt passive skill on enter called ");
			if (_tauntType == TauntType.Target)
			{
				_tauntTarget = Parent.Caster;
			}
			else if (_tauntType == TauntType.Caster)
			{
				// todo??
				_tauntTarget = Parent.Caster;
			}

			return false;
		}


	    public override bool GetAvailableTarget(Skill skill, Unit caster, ref List<Unit> targetList)
		{
			if (_tauntTarget == null)
			{
				return false;
			}

			if (_tauntTarget.IsFriend(caster) == true)
			{
				// 적일때만 target 고정 허용.
				return false;
			}

			if (_tauntTarget.IsLive() == false)
			{
				// 살아 있을 때만 처리
				return false;
			}

			targetList.Clear();
			targetList.Add(_tauntTarget);

			return true;
		}

	}
}
