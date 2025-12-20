using Corgi.GameData;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	/// <summary>
	/// SkillActive
	/// 입력을 받고 실행
	/// </summary>
    [System.Serializable]
	public class SkillAttack : SkillActive
	{
	    /// <summary>
	    /// Skill Data
	    /// </summary>
		
	    
		/// <summary>
		/// passive component
		/// </summary>
        //protected List<EffectSkillCompStruct> PassiveList = new List<EffectSkillCompStruct>();
		
	    /// <summary>
	    /// Dynamic Data
	    /// </summary>
	    /// 
	    public override SkillActionType GetSkillActionType()
	    {
		    return SkillActionType.Attack;
	    }
	    

		// get current cooltime
		public SkillAttack()
	    {
		    Level = 1;
	    }

		/// <summary>
		/// 스킬 실행 진입점.
		/// </summary>
		public override SkillActionLogNode DoSkill(Unit target)
		{
			var retLogNode = base.DoSkill(target);
			if (retLogNode == null)
			{
				return null;
			}

			retLogNode.ManaCost = 0;
			
			retLogNode.AddDetailLog($"AttackSpeed : {Owner.AttackSpeed}");


//			if (logNode.Target != null)
//			{
//                logNode.Target.OnEvent(CombatEventType.OnSkillTargeted, eventParam, logNode);
//			}

//			if (logNode.HaveLogNode(typeof(DamageSkillCompLogNode)))
//			{
//				// damage 를 포함한 스킬 사용시
//                Owner.OnEvent(CombatEventType.OnSkillDamage, eventParam, logNode);
//			}
			
			//OnSkill(logNode);

			return retLogNode;
		}

	    protected override void OnSkill(SkillActionLogNode skillActionLogNode)
	    {
		    base.OnSkill(skillActionLogNode);
		    
		    
		    Owner.AddMana(1, skillActionLogNode);
	    }

		public virtual void OnCastingComplete(SkillActionLogNode logNode)
		{
			
		}

		public virtual void OnChannelingTick(SkillActionLogNode logNode)
		{
			
		}
		
		public bool OnTurn(CombatLogNode logNode)
		{
			return true;
		}
		
		bool OnActive(EventParam eventParam, CombatLogNode logNode)
		{
			if (Owner.IsLive() == false)
			{
				return false;
			}
			
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

//		public void SetSkillAttributeType(SkillAttributeType attrType)
//		{
//			AttributeType = attrType;
//		}
	}
}