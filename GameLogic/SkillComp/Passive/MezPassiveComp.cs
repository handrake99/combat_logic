using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class MezPassiveComp : PassiveSkillComp
    {
	    /// <summary>
	    /// Setting
	    /// </summary>
	    private MezType _mezType = MezType.None;
	    //private bool _passAction = false;
	    private bool _isRemovedByHit = false;
	    

		public MezType MezType
		{
			get { return _mezType; }
		}

		public MezPassiveComp()
		{
			PassiveType = PassiveSkillCompType.Mez;
	        EventManager.Register(CombatEventType.OnSkillEffectEnter, OnSkillEffectEnter);
	        EventManager.Register(CombatEventType.OnBeingHit, OnBeingHit);
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

			_mezType = CorgiEnum.ParseEnum<MezType>(spec.PassiveSubType);
			if (_mezType == MezType.None)
			{
				return false;
			}

			if (_mezType == MezType.Sleep)
			{
				_isRemovedByHit = true;
			}

			return true;
		}
		
	    public override SkillCompLogNode CreateLogNode(Unit target)
	    {
		    return new MezPassiveSkillCompLogNode(Owner, target, this);
	    }

	    bool OnSkillEffectEnter(EventParam eventParam, CombatLogNode logNode)
	    {
            var caster = Parent.Caster;
            var target = Parent.Target;
		    if (target.IsCasting || target.IsChanneling)
		    {
			    var curLogNode = new InterruptSkillCompLogNode(caster, target, this);
			    if (target.InterruptCasting(Owner, curLogNode))
			    {
                    target.OnEvent(CombatEventType.OnCastingCancel, new EventParamSkillComp(Owner.Dungeon, this, curLogNode), curLogNode);
                    caster.OnEvent(CombatEventType.OnCastingInterrupt, new EventParamSkillComp(Owner.Dungeon, this, curLogNode), curLogNode);
			    }
		    }
		    return false;
	    }

	    bool OnBeingHit(EventParam eventParam, CombatLogNode logNode)
	    {
		    if (_isRemovedByHit)
		    {
                Parent.DoOver(logNode);
			    return true;
		    }

		    return false;
	    }
	}
}
//{
//    [System.Serializable]
//	public class MezPassiveComp : PassiveSkillComp 
//	{
//		MezType _mezType;
//        int _mezGrade;
//
//		public MezType MezType { get { return _mezType; } }
//        public int MezGrade { get { return _mezGrade; } }        
//
//		public MezPassiveComp(IUnit caster, SkillInst skillInst)
//			: base(caster, skillInst)
//		{
//
//		}
//
//		protected override void InitGameObject ()
//		{
//			base.InitGameObject ();
//
//			_eventManager.Register(CombatEventType.OnBeingHit, this.OnBeingHit);
//		}
//
//		public override bool Load(long code)
//		{
//			if(base.Load(code) == false)
//			{
//				return false;
//			}
//
//			_mezType = (MezType)CorgiSpecHelper.ParseEnum(typeof(MezType), _spec.param0);
//            _mezGrade = (int)_spec.param2;
//
//			if(_mezType == IdleCs.MezType.None)
//			{
//				//Debug.LogError("invalid mez type : " + _spec.param0);
//			}
//
//			return true;
//		}
//
//
//		bool OnBeingHit(IEventParam eventParam, CombatLogNode logNode)
//		{
//			if(MezType == MezType.Blind)
//			{
//                EventParamSkillOutput param = eventParam as EventParamSkillOutput;
//                if(param == null)
//                {
//                    return false;
//                }
//
//                SkillOutputActive output = param.OutputActive;
//                if(output == null)
//                {
//                    return false;
//                }
//
//                if(output.IgnoreMezBlind == false)
//                {
//                    SkillInst.State = SkillInstState.Dead;
//                }
//
//			}
//			return false;
//		}
//
//		public override bool GetAvailableTarget(IAction action, IUnit caster, ref List<int> targetList)
//		{
//			if(MezType == MezType.Confuse)
//			{
//				targetList.Clear();
//				UnitIndexType indexType;
//				if(caster.UnitType == UnitType.Character)
//				{
//					indexType = UnitIndexType.MonsterRandom;
//				}else
//				{
//					indexType = UnitIndexType.CharacterRandom;
//				}
//				targetList.Add((int)indexType);
//			}
//
//			return true;
//		}
//
//		public override bool CheckImmune(ActiveSkillComp inst)
//		{
//			if(_mezType == MezType.Banish)
//			{
//				return true;
//			}
//
//			return false;
//		}
//
//		public override bool CheckImmune(SkillInst inst)
//		{
//			if(_mezType == MezType.Banish)
//			{
//				return true;
//			}
//
//			return false;
//		}
//
//        public override string GetSCTKey()
//        {
//            return "Mez_" + _mezType;
//        }
//
//        public override void GetTotalEffectList(List<long> compList, ref List<SpecSkillEffectObjectInfo> effectList)
//        {
//            base.GetTotalEffectList(compList, ref effectList);            
//
//            long effectCode = GetStatusEffectCodeByMezType(_mezType);
//            SpecSkillEffectObjectInfo spec = CorgiStaticData.Instance.GetSkillEffectObjectList(effectCode) as SpecSkillEffectObjectInfo;
//
//            if (spec != null)
//            {                
//                effectList.Add(spec);
//            }
//        }
//
//        private long GetStatusEffectCodeByMezType(MezType type)
//        {
//            long result = 0;
//
//            switch (type)
//            {
//                case IdleCs.MezType.Disappear:
//                    {
//                        result = (long)StatusEffectCode.Disapear;
//
//                        break;
//                    }
//                case IdleCs.MezType.Banish:
//                    {
//                        result = (long)StatusEffectCode.Banish;
//
//                        break;
//                    }
//                case IdleCs.MezType.Stun:
//                    {
//                        result = (long)StatusEffectCode.Stun;
//
//                        break;
//                    }
//                case IdleCs.MezType.NoAction:
//                    {
//                        result = (long)StatusEffectCode.NoAction;
//
//                        break;
//                    }
//                case IdleCs.MezType.Blind:
//                    {
//                        result = (long)StatusEffectCode.Blind;
//
//                        break;
//                    }
//                case IdleCs.MezType.Silence:
//                    {
//                        result = (long)StatusEffectCode.Silence;
//
//                        break;
//                    }
//                case IdleCs.MezType.DisableAttack:
//                    {
//                        result = (long)StatusEffectCode.DisableAttack;
//
//                        break;
//                    }
//                case IdleCs.MezType.Confuse:
//                    {
//                        result = (long)StatusEffectCode.Confuse;
//
//                        break;
//                    }
//                case IdleCs.MezType.Casting:
//                    {
//                        result = (long)StatusEffectCode.Casting;
//
//                        break;
//                    }
//            }
//
//            return result;
//        }
//	}
//}
