
using System;
using System.Collections.Generic;

//using IdleCs.GameData;


namespace IdleCs.GameLogic
{
	/// <summary>
	/// Unit의 액션을 실행하기 위한 Input.
	/// </summary>
	public class ActionInput
	{
		/// <summary>
		/// Default ActionInput
		/// SetInput 후에 사용한다
		/// 아직 사용 안함.
		/// </summary>
		static public ActionInput DefaultActionInput = new ActionInput();
		
		/// <summary>
		/// _ownerId : 실행하는 Unit 의 Id. 반드시 존재.
		/// _skillId : 해당 Unit의 사용하는 스킬. 유저 입력을 받을 경우에는 존재한다.
		/// _targetId : 해당 Unit 스킬의 타겟. 유저 입력을 받을 경우에는 존재한다
		/// 
		/// 몬스터 혹은 캐릭터의 자동스킬인 경우 _skillId 와 _targetId는 null 로 세팅한다
		/// </summary>
		private string 		_ownerId;
		private string		_skillId;
		private string		_targetId;
		
		public string OwnerId
		{
			get
			{
				return _ownerId;
				
			}
		}

		public string SkillId
	    {
		    get { return _skillId; }
	    }

	    public string TargetId
	    {
		    get { return _targetId; }
	    }

		// static instance 용
		public ActionInput()
		{
		}
		
		public ActionInput(string ownerId)
		{
			_ownerId = ownerId;
		}

		// 매번 새로운 instance를 만들어야 할때
	    public ActionInput(string ownerId, string skillId, string targetId)
	    {
		    _ownerId = ownerId;
			_skillId = skillId;
			_targetId = targetId;
		}

		// DefaultActionInput용
		public void SetInput(string ownerId, string skillId, string targetId)
		{
		    _ownerId = ownerId;
			_skillId = skillId;
			_targetId = targetId;
		}

		public bool CheckTargetAlive()
		{
			return false;
		}
	}
}