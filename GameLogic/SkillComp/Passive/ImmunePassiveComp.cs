
using System;
using System.Collections.Generic;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class ImmunePassiveComp : PassiveSkillComp
    {
	    /// <summary>
	    /// Setting
	    /// </summary>
	    private ImmuneType _immuneType = ImmuneType.All;

		public ImmuneType ImmuneType 
		{
			get { return _immuneType; }
		}

	    public ImmunePassiveComp() : base()
	    {
		    PassiveType = PassiveSkillCompType.Immune;
	    }
	    
		protected override bool LoadInternal(ulong uid)
		{
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}

			var spec = GetSpec();
			if (spec == null)
			{
				return false;
			}

			_immuneType = CorgiEnum.ParseEnum<ImmuneType>(spec.PassiveSubType);
			if (_immuneType == ImmuneType.None)
			{
				return false;
			}
			
			return true;
		}
	    
		public override bool CheckImmune(ActiveSkillComp skillComp)
		{
			if (skillComp == null)
			{
				return false;
			}
			
			if (_immuneType == ImmuneType.All || _immuneType == ImmuneType.Damage)
			{
				var convertComp = skillComp as DamageSkillComp;
				if(convertComp != null)
				{
					return true;
				}
			}

			return base.CheckImmune(skillComp);
		}

		public override bool CheckImmune(SkillEffectInst skillInst)
		{
			if (skillInst == null)
			{
				return false;
			}
			
            if ((_immuneType == ImmuneType.All || _immuneType == ImmuneType.Debuff)
                && skillInst.BenefitType == ContinuousBenefitType.Debuff)
            {
                return true;
            }

            if ((_immuneType == ImmuneType.All || _immuneType == ImmuneType.Mez))
            {
	            var passiveList = new List<PassiveSkillComp>();

	            if (skillInst.GetPassiveComp(PassiveSkillCompType.Mez, passiveList))
	            {
		            return IsNotFortifyNorExhausted(passiveList);
	            }
            }

			return false;
		}
	    public override bool CheckImmune(ImmuneType immuneType)
	    {
		    return (_immuneType == ImmuneType.All || immuneType == ImmuneType.All || _immuneType == immuneType);
	    }

	    protected bool IsNotFortifyNorExhausted(List<PassiveSkillComp> passiveList)
	    {
		    var ret = true;
		    
		    foreach (var passiveComp in passiveList)
		    {
			    var mezPassiveComp = passiveComp as MezPassiveComp;
			    
			    if (mezPassiveComp == null)
			    {
				    continue;
			    }

			    if (mezPassiveComp.MezType != MezType.Fortify && mezPassiveComp.MezType != MezType.Exausted)
			    {
				    continue;
			    }

			    ret = false;
		    }

		    return ret;
	    }
	}
}

//
//using System;
//using System.Collections.Generic;
//using IdleCs;
//using IdleCs.Spec;
//
//
//namespace IdleCs.GameLogic
//{
//    [System.Serializable]
//	public class ImmunePassiveComp : PassiveSkillComp 
//	{
//		ImmuneType _immuneType;
//		//StatChangeType _statChangeType;
//
//		public ImmuneType ImmuneType {get {return _immuneType; }}
//		//public StatChangeType StatChangeType {get {return _statChangeType; }}
//		//public float ChangeValue {get {return _spec.param2; }}
//
//		public ImmunePassiveComp(IUnit caster, SkillInst skillInst)
//			: base(caster, skillInst)
//		{}
//
//		public override bool Load(long code)
//		{
//			if(base.Load(code) == false)
//			{
//				return false;
//			}
//
//			_immuneType = (ImmuneType)CorgiSpecHelper.ParseEnum(typeof(ImmuneType), _spec.param0);
//
//			return true;
//		}
//
//
//		public override bool CheckImmune(ActiveSkillComp skillComp)
//		{
//			if(_immuneType == ImmuneType.Damage)
//			{
//				DamageSkillComp convertComp = skillComp as DamageSkillComp;
//				if(convertComp != null)
//				{
//					return true;
//				}
//			}else if(_immuneType == ImmuneType.Heal)
//			{
//				HealSkillComp convertComp = skillComp as HealSkillComp;
//				if(convertComp != null)
//				{
//					return true;
//				}
//			}else if(_immuneType == ImmuneType.Interrupt)
//			{
//				InterruptSkillComp convertComp = skillComp as InterruptSkillComp;
//				if(convertComp != null)
//				{
//					return true;
//				}
//			}
//
//			return base.CheckImmune(skillComp);
//		}
//
//		public override bool CheckImmune(SkillInst inst)
//		{
//			List<PassiveSkillComp> passiveList = new List<PassiveSkillComp>();
//            if(_immuneType == ImmuneType.Debuff && inst.BenefitType == SkillContinuousType.Debuff)
//            {
//                return true;
//            }else if(inst.BenefitType == SkillContinuousType.Debuff && _immuneType == ImmuneType.DebuffMez)
//			{
//				inst.GetPassiveComp(PassiveEffectType.Mez, passiveList);
//				if(passiveList.Count > 0)
//				{
//					return true;
//				}
//            }else if(inst.BenefitType == SkillContinuousType.Debuff && inst.IsDispel == true && _immuneType == ImmuneType.DebuffDispelable)
//            {
//                return true;
//
//			}else if(_immuneType == ImmuneType.Stun)
//			{
//				inst.GetPassiveComp(PassiveEffectType.Mez, passiveList);
//				if(passiveList.Count <= 0)
//				{
//					return false;
//				}
//
//				foreach(MezPassiveComp skillComp in passiveList)
//				{
//					if (skillComp != null && skillComp.MezType == MezType.Stun)
//					{
//						return true;
//					}
//				}
//			}
//
//			return false;
//		}
//
//	}
//}