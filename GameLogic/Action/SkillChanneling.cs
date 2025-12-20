using IdleCs.GameLog;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class SkillChanneling : SkillActive
    {
	    // for channeling
	    private ulong _curChannelingTime = 0;
	    private ulong _curChannelingTick = 1000;
	    private uint _curTickCount = 0;
	    private bool _isChanneling = false;

	    private uint _totalTickCount = 0;
	    private ulong[] _curTimeArray = null;
	    private bool[] _isInvokedArray = null;
	    
	    private ulong _enhancedChannelingTime;
	    private ulong _enhancedChannelingTick;
	    
	    private Unit _curTarget = null;
	    
	    public override bool IsChanneling
	    {
		    get { return _isChanneling; }
	    }
	    
	    public override SkillActionType GetSkillActionType()
	    {
		    return SkillActionType.Skill;
	    }
	    
		// load active
		protected override bool LoadInternal(ulong uid)
		{
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}
			
		    CastingTime = GetSpec().CastingTime;
		    ChannelingTick = GetSpec().ChannelingTick;
		    
		    _totalTickCount = (uint)(CastingTime / ChannelingTick);
		    
		    _curTimeArray = new ulong[_totalTickCount];
		    _isInvokedArray = new bool[_totalTickCount];

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

			var retLogNode = base.DoSkill(target);
			
			if (retLogNode == null)
			{
				return null;
			}

			// todo channling tick // count or tickTime
			_enhancedChannelingTime = (ulong)Owner.ApplyEnhance(EnhanceType.CastingSpeed, (float)CastingTime, retLogNode);
			_enhancedChannelingTick = (ulong)Owner.ApplyEnhance(EnhanceType.CastingSpeed, (float)ChannelingTick, retLogNode);

			_curChannelingTime = _enhancedChannelingTime;
			_curChannelingTick = _enhancedChannelingTick;
		    _curTickCount = 0;

		    for (int i = 0; i < _totalTickCount; ++i)
		    {
			    _curTimeArray[i] = 0;
			    _isInvokedArray[i] = false;
		    }

		    _curTarget = target;
			
			//CorgiLog.LogLine("DoSkill for {0}/{1}", this.Name, Uid);

			if (_curChannelingTime <= 0)
			{
				return null;
			}

			_isChanneling = true;
			retLogNode.AddDetailLog($"Start Channeling Skill : {Owner.Dungeon.GameData.GetStrByUid(Uid)} / Channeling: {_curChannelingTime}");
			
			return retLogNode;
		}
		
	    protected override void Tick(ulong deltaTime, TickLogNode logNode)
	    {
		    // check target condition
		    if (_curTarget.IsLive() == false)
		    {
			    Owner.OnCancelCasting(logNode);
			    return;
		    }
		    
		    
		    bool isOnTick = false;
		    if (_curChannelingTick < deltaTime)
		    {
			    _curChannelingTick += _enhancedChannelingTick - deltaTime;
			    isOnTick = true;
		    }
		    else
		    {
                _curChannelingTick -= deltaTime;
		    }

		    if (_curChannelingTime < deltaTime)
		    {
			    _curChannelingTime = 0;
		    }
		    else
		    {
                _curChannelingTime -= deltaTime;
		    }

		    TickCurTimeArray(deltaTime, logNode);

		    if (_isChanneling == false)
		    {
			    return;
		    }

		    if (_curTickCount < _totalTickCount && (isOnTick || _curChannelingTime <= 0))
		    {
			    _curTickCount++;
			    var thisLogNode = new ChannelingTickLogNode(Owner, this, _curTarget);
			    
			    OnChannelingTick(thisLogNode);
			    
			    thisLogNode.ApplyChannelingTick(_curTickCount * _enhancedChannelingTick, _enhancedChannelingTime);
			    
			    logNode.AddChild(thisLogNode);
			    
				if (_curTarget!= null && _curTarget.IsLive() == false)
				{
					Owner.OnCancelCasting(logNode);
				}
		    }
	    }

	    void OnChannelingTick(ChannelingTickLogNode tickLogNode)
	    {
			tickLogNode.AddDetailLog($"Channeling Tick: {Owner.Dungeon.GameData.GetStrByUid(Uid)} / CastingTime : {_curChannelingTime}");
			
			var eventParam = new EventParamAction(Owner.Dungeon, this, tickLogNode);
			Owner.OnEvent(CombatEventType.OnChannelingTick, eventParam, tickLogNode);

	    }

	    void OnChannelingComplete(SkillActionLogNode logNode)
	    {
			logNode.AddDetailLog($"Completed Channeling Skill : {Owner.Dungeon.GameData.GetStrByUid(Uid)} / ChannelingTime : {_curChannelingTime}");
		    
            var eventParam = new EventParamAction(Owner.Dungeon, this, logNode);
            Owner.OnEvent(CombatEventType.OnChannelingComplete, eventParam, logNode);

            OnSkill(logNode);
			Owner.OnSkill(this, logNode);
	    }
	    
	    protected override void OnSkill(SkillActionLogNode logNode)
	    {
		    base.OnSkill(logNode);
		    
			_curChannelingTime = 0;
			_curChannelingTick = 1000;
			_curTickCount = 0;
            _isChanneling = false;

            IsActioned = true;
	    
			_curTarget = null;
	    }
	    
		public override bool OnCancelCasting(CombatLogNode logNode)
		{
			if (IsChanneling == false)
			{
				return false;
			}

			_isChanneling = false;
			_curChannelingTime = 0;
			_curChannelingTick = 0;
		    _curTickCount = 0;

			return base.OnCancelCasting(logNode);
		}

		private void TickCurTimeArray(ulong deltaTime, TickLogNode logNode)
		{
			for (int i = 0; i < _curTickCount; ++i)
			{
				if (_curTimeArray[i] >= SkillActionTime)
				{
					continue;
				}
				
				_curTimeArray[i] += deltaTime;

				if (_isInvokedArray[i] == false && _curTimeArray[i] >= SkillInvokeTime)
				{
					var invokeActionLogNode = new SkillInvokeActionLogNode(Owner, this, _curTarget);
					InvokeSkill(invokeActionLogNode);
					logNode.AddChild(invokeActionLogNode);
					_isInvokedArray[i] = true;
				}
			}

			if (_curTimeArray[_totalTickCount - 1] >= SkillActionTime)
			{
				var completeLogNode = new CompleteCastingLogNode(Owner, this, _curTarget);
				completeLogNode.IsChanneling = true;
					
				OnChannelingComplete(completeLogNode);
				logNode.AddChild(completeLogNode);
			}
		}
    }
}