using System;
using System.Collections.Generic;
using UnityEngine;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Library;
using IdleCs.Utils;
using IdleCs.Managers;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
	/// <summary>
	/// SkillActive
	/// 입력을 받고 실행
	/// </summary>
    [System.Serializable]
	public class SkillActive : Skill
	{
		/// <summary>
		/// Skill Data
		/// </summary>
		///
		///
		private bool _isAreaEffect = false;

		public bool IsAreaEffect => _isAreaEffect;

		/// <summary>
	    /// Dynamic Data
	    /// </summary>
	    private ulong _skillInvokeTime = 0;
		private ulong _origSkillInvokeTime = 0;
	    private ulong _skillActionTime = 0;		// attackSpeed 변화로 바뀐 actionTime
	    private ulong _origSkillActionTime = 0; // sheet에서 가져오는 actionTime
	    
	    private ulong _curTime = 0;
	    private bool _isInvoked = false;
	    private bool _isActioned = false;
	    private Unit _target = null;
	    

	    public bool IsActioned
	    {
		    get { return _isActioned; }
		    protected set { _isActioned = value; }
	    }
	    protected Unit Target => _target;

	    public virtual bool IsCasting
	    {
		    get { return false; }
	    }
	    
	    public virtual bool IsChanneling
	    {
		    get { return false; }
	    }

	    public bool IsImmune
	    {
		    get { return GetSpec().IsImmune; }
	    }
	    
	    public ulong SkillActionTime
	    {
		    get { return _skillActionTime; }
	    }
	    
	    public ulong SkillInvokeTime
	    {
		    get { return _skillInvokeTime; }
	    }
	    
	    public delegate SkillActionLogNode OnInvokeSkillCallbackType(Unit target);

	    public OnInvokeSkillCallbackType OnInvokeSkill;
	    
	    public override SkillActionType GetSkillActionType()
	    {
		    return SkillActionType.Skill;
	    }

		// get current cooltime
		public SkillActive()
		{
		}

		// load active
		protected override bool LoadInternal(ulong uid)
		{
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}

			var spec = GetSpec();
			
		    var animationInfo = Owner.Dungeon.GameData.GetData<ArtAnimationInfoSpec>(spec.AnimationUid);
		    if (animationInfo != null)
		    {
			    _skillInvokeTime = animationInfo.InvokeTime;
			    _origSkillInvokeTime = _skillInvokeTime;
			    _skillActionTime = animationInfo.ActionTime;
			    _origSkillActionTime = _skillActionTime;
		    }
		    else
		    {
			    _skillInvokeTime = 500;
			    _origSkillInvokeTime = 500;
			    _skillActionTime = 1000;
			    _origSkillActionTime = 1000;
		    }

		    try
		    {
				var onActive = spec.OnActive;
				var isAreaEffect = false;
				
				foreach (var curData in onActive)
				{ 
					var targetStr = curData.TargetType;
					var skillCompUid = curData.SkillCompUid;
					var conditionUid = curData.ConditionCompUid;
					
					var skillCompInfo = SkillCompInfo.CreateOnActive(Owner, this, targetStr, skillCompUid, conditionUid);

					if (skillCompInfo != null)
				    {
						if (skillCompInfo.SkillCompType == SkillCompType.Active)
						{
							var activeSkillCompInfo = skillCompInfo as ActiveSkillCompInfo;
							
							ActiveList.Add(activeSkillCompInfo);
							
                            if (activeSkillCompInfo.IsAreaEffect())
                            {
                                isAreaEffect = true;
                            }
						}else if (skillCompInfo.SkillCompType == SkillCompType.Continuous)
						{
							EffectList.Add(skillCompInfo as EffectSkillCompInfo);
						}else if (skillCompInfo.SkillCompType == SkillCompType.Array)
						{
							ArrayList.Add(skillCompInfo as ArraySkillCompInfo);
						}
				    }
				}

				_isAreaEffect = isAreaEffect;
		    }
		    catch (Exception e)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid onActive {0}", spec.Name);
			    return false;
		    }

		    //CorgiLog.LogLine("Load Skill({0}) for {1}", spec.Name, Owner.Name);

		    return true;

		}

		/// <summary>
		/// 스킬 실행 진입점.
		/// </summary>
		public override SkillActionLogNode DoSkill(Unit target)
		{
			if (target == null)
			{
				return null;
			}
			var retLogNode = new SkillActionLogNode(Owner, this, target);
			retLogNode.ManaCost = GetManaCost();

			_curTime = 0;

			_isInvoked = false;
			_isActioned = false;

			_target = target;
			
			if (OnInvokeSkill != null)
			{
				retLogNode.RelicLogNode = OnInvokeSkill(target);
				Owner.OnUpdateEffect(retLogNode);
			}

			CorgiCombatLog.Log(CombatLogCategory.Skill, "{0} Do Skill({1}/{2})", Name, _curTime, _skillInvokeTime);
			
			return retLogNode;
		}
		
		public bool InvokeSkill(SkillActionLogNode logNode)
		{
			logNode.AddDetailLog($"Invoke Skill : {Name}, Level : {Level}");
			InvokeSkillComp(ArrayList, logNode);
			InvokeSkillComp(ActiveList, logNode);
			InvokeSkillComp(EffectList, logNode);

			if (SkillType == SkillType.Active || SkillType == SkillType.Casting || SkillType == SkillType.Channeling)
			{
				var eventParam = new EventParamAction(Owner.Dungeon, this, logNode);
				Owner.OnEvent(CombatEventType.OnSkill, eventParam, logNode);
			}else if(SkillType == SkillType.Attack)
			{
				var eventParam = new EventParamAction(Owner.Dungeon, this, logNode);
				Owner.OnEvent(CombatEventType.OnAttack, eventParam, logNode);
			}

			CorgiCombatLog.Log(CombatLogCategory.Skill,
				$"{Owner.Name} Invoke {Name} Action({_curTime}/{_skillInvokeTime})");

			return true;
		}

		protected override void Tick(ulong deltaTime, TickLogNode logNode)
		{
			_curTime += deltaTime;

			if (_isInvoked == false && _curTime >= _skillInvokeTime)
			{
				var thisTarget = _target;
				var skillLogNode = new SkillInvokeActionLogNode(Owner, this, _target);
				skillLogNode.CastingTime = CastingTime;
				skillLogNode.ManaCost = GetManaCost();
				
				InvokeSkill(skillLogNode);
				OnSkill(skillLogNode);


				if (SkillType == SkillType.Attack)
				{
					Owner.OnAttack(this, skillLogNode);
				}else
				{
					Owner.OnSkill(this, skillLogNode);
				}
				
				_isInvoked = true;
				

				logNode.AddChild(skillLogNode);

			}

			if (_curTime >= _skillActionTime)
			{
				_isActioned = true;
				CorgiCombatLog.Log(CombatLogCategory.Skill, $"{Owner.Name} is {Name} Actioned({_curTime}/{_skillActionTime})");
			}
		}
		
	    protected virtual void OnSkill(SkillActionLogNode logNode)
	    {
			_target = null;
	    }
		

		public virtual bool OnCancelCasting(CombatLogNode logNode)
		{
			var invokeLogNode = new CancelCastingLogNode(Owner, this, _target);
			invokeLogNode.ResultType = SkillResultType.SkillResultInterrupted;
			logNode.AddChild(invokeLogNode);

			_isInvoked = true;

			return true;
		}

		/// <summary>
		/// Mez 걸렸을때
		/// todo: implement
		/// </summary>
		public bool OnMez(EventParam eventParam, CombatLogNode logNode)
		{
			return false;
		}

		/// <summary>
		/// 죽음에 이르렀을때
		/// todo: implement
		/// </summary>
		bool OnDead(EventParam eventParam, CombatLogNode logNode)
		{
			return false;
		}

		/// <summary>
		/// Skill 사용 가능 체크.
		/// 추가 구현 필요.
		/// </summary>
		public bool CheckSkillUsage(Unit target)
		{
			return false;
		}

		public void ResetSkillActionTime(long diff)
		{
			if (diff < 0)
			{
				var newActionTime = (long)_skillActionTime + diff;

				if (newActionTime <= 0)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill, "zero actionTime in attack[{0}]", Owner.Dungeon.GameData.GetStrByUid(Uid));
					newActionTime = 0;
				}

				_skillActionTime = (ulong)newActionTime;
			}
			else
			{
				_skillActionTime += (ulong)diff;
			}

			_skillInvokeTime = _skillActionTime < _origSkillInvokeTime ? _skillActionTime : _origSkillInvokeTime;
		}
	}
}
