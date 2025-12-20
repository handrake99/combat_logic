//using System;
//using System.Collections.Generic;
//using IdleCs;
//using IdleCs.Spec;
//
//namespace IdleCs.GameLogic
//{
//    [System.Serializable]
//	public class SkillCompArray : ISkillComp, ISkillEffect, ICorgiEvent
//    {
//        List<ISkillComp> _skillCompList = new List<ISkillComp>();
//
//		IUnit       _owner;
//		IUnit       _caster;
//		ISkillActor _actor;
//		List<SkillTargetComp> _targetList = new List<SkillTargetComp>();
//
//        public String Name { get { return null; } }
//		public long Code { get { return -1; } }
//        public IUnit Owner { get{return _owner;} }
//        public IUnit Caster { get{return _caster;} }
//        public ISkillActor ParentActor { get{return _actor;} }
//        public int CoolTime { get{return 0;} }
//        public SkillSchoolType SchoolType { get{return SkillSchoolType.All;} }
//        public SkillType SkillType { get{return SkillType.None;} }
//        public int ActiveCount { get { return 1; } set { ;} }
//
//		public List<SpecSkillEffectObjectInfo> EffectList { get { return null; } }
//		public void GetTotalEffectList(List<long> compList, ref List<SpecSkillEffectObjectInfo> effectList) 
//        {
//            foreach(ISkillComp skillComp in _skillCompList)
//            {
//                List<SpecSkillEffectObjectInfo> curEffectList = skillComp.EffectList;
//
//                effectList.AddRange(curEffectList);
//            }
//        }
//        public List<SkillConditionComp> ConditionCompList { get{return null;} }
//		
//		protected List<UnitAggro> _savedAggroList = new List<UnitAggro>();
//		public List<UnitAggro> SavedAggroList { get { return _savedAggroList; } }
//
//        public TargetAliveType TargetAliveType 
//        { 
//            get
//            {
//                if( _skillCompList.Count > 0 )
//                {
//                    return _skillCompList[0].TargetAliveType;
//                }
//
//                return TargetAliveType.All;
//            } 
//        }
//        public bool IgnoreCoolTime 
//        { 
//            get
//            {
//                foreach(ISkillComp skillComp in _skillCompList)
//                {
//                    if(skillComp != null && skillComp.IgnoreCoolTime == true)
//                    {
//                        return true;
//                    }
//                }
//                return false;
//            } 
//            set{} 
//        }
//
//        public bool IsCombatLog { get{return false;} }
//        public bool IsCombatMessage { get{return false;} }
//
//		public SkillCompArray(IUnit caster, ISkillActor actor)
//		{
//			if(caster == null)
//			{
//				CorgiLog.Assert(false);
//			}
//			if(actor == null)
//			{
//				CorgiLog.Assert(false);
//			}
//			_owner = caster;
//			_caster = caster;
//			_actor = actor;
//
//			//_eventManager.Register(CombatEventType.OnTurn, this.OnTurn);
//		}
//
//		public SkillCompArray(IUnit owner, IUnit caster, ISkillActor actor)
//		{
//			if(owner == null)
//			{
//				CorgiLog.Assert(false);
//			}
//			if(caster == null)
//			{
//				CorgiLog.Assert(false);
//			}
//			if(actor == null)
//			{
//				//CorgiLog.Assert(false);
//			}
//			
//			_owner = owner;
//			_caster = caster;
//			_actor = actor;
//		}
//
//        public bool DoApply(ISkillOutput output, CombatLogNode logNode)
//        {
//			ActionInput input = null;
//
//			// input 을 가져온다.
//			input = logNode.ActionInput;
//			
//			if(_caster != null && _caster.UnitType == UnitType.Monster)
//			{
//				_savedAggroList.Clear();
//				_savedAggroList.AddRange(_caster.AggroList);
//			}
//
//			List<IUnit> targetList = new List<IUnit>();
//
//			// input 을 토대로 TargetList 를 가져온다.
//            if( this.GetTargetList(input, targetList) == false )
//            {
//                CorgiLog.Assert(false);
//            }
//
//            /*if(output != null && targetList.Count == 0)
//            {
//                targetList.Add(output.Target);
//            }*/
//
//			// 각 Target 에 대하여 DoApply 를 적용한다.
//			bool ret = false;
//
//
//            foreach(IUnit target in targetList)
//            {
//                CombatLogNode curLogNode = logNode;
//
//                if(target == null)
//                {
//                    CorgiLog.Assert(false);
//                    continue;
//                }
//
//                foreach(ISkillComp skillComp in _skillCompList)
//                {
//                    //CorgiLog.LogLine("skill comp DoApply try Call");
//                    bool curRet = false;
//
//                    skillComp.ActiveCount = skillComp.CheckActive(target, output, logNode);
//
//                    if(skillComp.ActiveCount > 0 )
//                    {
//                        if (output == null)
//                        {
//                            curRet = skillComp.DoApply(target, input, curLogNode);
//                        }
//                        else
//                        {
//                            curRet = skillComp.DoApply(target, input, output, curLogNode);
//                        }
//                    }
//
//                    if(curRet == true)
//                    {
//                        ret = true;
//                    }
//
//                    if(curLogNode.LastChild != null)
//                    {
//                        curLogNode = curLogNode.LastChild;
//                    }
//                }
//            }
//
//			return ret;
//
//        }
//        public virtual bool DoApply(IUnit target, ActionInput actionInput, CombatLogNode logNode) { return false; }
//        public virtual bool DoApply(IUnit target, ActionInput actionInput, ISkillOutput output, CombatLogNode logNode) { return false; }
//
//        public void DoCancel(CombatLogNode logNode)
//        {
//			ActionInput input = logNode.ActionInput;
//
//			List<IUnit> targetList = new List<IUnit>();
//
//			if( this.GetTargetList(input, targetList) == false )
//			{
//				CorgiLog.Assert(false);
//			}
//			//CorgiLog.LogLine("skill comp target count : " + targetList.Count);
//
//            foreach (IUnit target in targetList)
//            {
//                CombatLogNode curLogNode = logNode;
//
//                foreach (ISkillComp skillComp in _skillCompList)
//                {
//                    if (target == null)
//                    {
//                        CorgiLog.Assert(false);
//                        continue;
//                    }
//                    skillComp.DoCancel(target, curLogNode);
//
//                    if(curLogNode.LastChild != null)
//                    {
//                        curLogNode = curLogNode.LastChild;
//                    }
//                }
//            }
//        }
//        public bool DoCancel(IUnit target, CombatLogNode logNode)
//        {
//            return false;
//        }
//		
//		public void SetTargetList(SkillTargetType targetType)
//		{
//			_targetList.Clear();
//
//			SkillTargetComp comp = SkillTargetCompFactory.Create(targetType);
//			if(comp != null)
//			{
//				_targetList.Add(comp);
//			}
//		}
//
//        public void SetTargetList(List<SkillTargetType> targetList)
//        {
//			_targetList.Clear();
//			foreach(SkillTargetType targetType in targetList)
//			{
//				SkillTargetComp comp = SkillTargetCompFactory.Create(targetType);
//				if(comp != null)
//				{
//					_targetList.Add(comp);
//				}
//			}
//        }
//        public bool GetTargetList(ActionInput input, List<IUnit> targetList)
//        {
//			foreach(SkillTargetComp targetComp in _targetList)
//			{
//				targetComp.GetTargetsList(this, input, targetList);
//			}
//
//			return true;
//        }
//        public bool IsCoolTime()
//        {
//            return false;
//        }
//        public int CheckActive(IUnit target, ISkillOutput output, CombatLogNode logNode)
//        {
//            return 1;
//        }
//
//		public bool OnEvent(IEventParam eventParam, CombatLogNode logNode)
//		{
//			ISkillOutput output = null;
//
//            output = eventParam.Output;
//
//			return DoApply(output, logNode);
//		}
//
//		public bool OnEvent(CombatEventType eventType, IEventParam eventParam, CombatLogNode logNode)
//		{
//            foreach(ISkillComp skillComp in _skillCompList)
//            {
//                skillComp.OnEvent(eventType, eventParam, logNode);
//            }
//
//			return false;
//		}
//		public bool OnTurn(IEventParam eventParam, CombatLogNode logNode)
//		{
//            foreach(ISkillComp skillComp in _skillCompList)
//            {
//                skillComp.OnTurn(eventParam, logNode);
//            }
//
//			return false;
//		}
//
//        public void AddSkillComp(ISkillComp skillComp)
//        {
//            _skillCompList.Add(skillComp);
//        }
//
//        public string GetDesc()
//        {
//            return null;
//        }
//        public string GetSCT() { return null; }
//
//        public void Serialize(ref JSONObject jsonObject) { }
//
//		public virtual void OnDestroy()
//        {
//            foreach(ISkillComp skillComp in _skillCompList)
//            {
//                if(skillComp == null)
//                {
//                    continue;
//                }
//                skillComp.OnDestroy();
//            }
//            _skillCompList.Clear();
//
//		    _owner = null;
//		    _caster = null;
//		    _actor = null;
//
//            _targetList.Clear();
//        }
//    }
//}