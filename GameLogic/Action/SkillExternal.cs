using System;
using System.Collections.Generic;

using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Network;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
	/// <summary>
	/// Skill External 
	/// SkillPassive와 비슷하게 동작
	/// Time Check를 한다
	/// </summary>
    [System.Serializable]
	public class SkillExternal : SkillPassive
	{
		private ulong _validTimestamp;
		
		public SkillExternal()
		{
		}

		public void SetTimestamp(ulong timestamp)
		{
			_validTimestamp = timestamp;
		}

		protected void Tick(ulong deltaTime, TickLogNode logNode)
		{
			if (IsActive == false)
			{
				return;
			}
			
			var curTimestamp = CorgiTime.UtcNowULong;
			if (curTimestamp >= _validTimestamp)
			{
				// over
				OnInActive(logNode);
			}
		}
		
	}
}
