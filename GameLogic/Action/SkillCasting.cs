using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class SkillCasting : SkillActive
    {
	    // for casting & channeling
	    private ulong _curCastingTime = 0;
	    private bool _isCasting = false;
	    
	    public override bool IsCasting
	    {
		    get { return _isCasting; }
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

			var castingTime = CastingTime;
			
			castingTime = (ulong)Owner.ApplyEnhance(EnhanceType.CastingSpeed, (float)castingTime, retLogNode);

			_curCastingTime = castingTime;
			
			//CorgiLog.LogLine("DoSkill for {0}/{1}", this.Name, Uid);

			if (_curCastingTime <= 0)
			{
				return null;
			}

			_isCasting = true;
			
			retLogNode.AddDetailLog($"Start Casting Skill : {Owner.Dungeon.GameData.GetStrByUid(Uid)} / CastingTime : {_curCastingTime}");
			
			return retLogNode;
		}
		
		
	    protected override void Tick(ulong deltaTime, TickLogNode logNode)
	    {
		    // Casting -> Acion 처리 수순
		    if (_isCasting == false)
		    {
			    base.Tick(deltaTime, logNode);
			    return;
		    }
		    
		    // unit is casting
		    if (_curCastingTime < deltaTime)
		    {
			    _curCastingTime = 0;
		    }
		    else
		    {
                _curCastingTime -= deltaTime;
		    }

		    if (_curCastingTime == 0)
		    {
			    _isCasting = false;
			    
			    var thisLogNode = new CompleteCastingLogNode(Owner, this, Target);
			    OnCastingComplete(thisLogNode);
			    logNode.AddChild(thisLogNode);
			    //Owner.OnSkill(this, thisLogNode);
		    }
	    }
	    
	    protected override void OnSkill(SkillActionLogNode logNode)
	    {
		    base.OnSkill(logNode);
		    
			var eventParam = new EventParamAction(Owner.Dungeon, this, logNode);
			Owner.OnEvent(CombatEventType.OnCastingComplete, eventParam, logNode);
		    
		    _curCastingTime = 0;
	    }
	    
		void OnCastingComplete(SkillActionLogNode logNode)
		{
			// 각 component 실행.
			logNode.AddDetailLog($"Completed Casting Skill : {Owner.Dungeon.GameData.GetStrByUid(Uid)} / CastingTime : {_curCastingTime}");
			//InvokeSkill(logNode);

			// var eventParam = new EventParamAction(Owner.Dungeon, logNode);
			// Owner.OnEvent(CombatEventType.OnCastingComplete, eventParam, logNode);
			
			//CorgiLog.LogLine("[Skill:{0}] Casting Complete ({1})", _curCastingTime, Name);

			//OnSkill(logNode);
		}
		public override bool OnCancelCasting(CombatLogNode logNode)
		{
			if (IsCasting == false)
			{
				return false;
			}

			_isCasting = false;
			_curCastingTime = 0;
			return base.OnCancelCasting(logNode);
		}
	    
        
    }
}