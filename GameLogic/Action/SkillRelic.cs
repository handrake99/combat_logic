using System;
using System.Collections.Generic;

using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
	/// <summary>
	/// Skill Passive
	/// 전투에 들어갈시 바로 동작
	/// </summary>
    [System.Serializable]
	public class SkillRelic : SkillActive
	{
		private SkillActive _activeSkill;
		
		// 현재 active 됐는지 체크.
		private bool _isActive = false;
		
	    /// <summary>
	    /// Passive Components
	    /// </summary>
        protected List<PassiveSkillCompInfo> PassiveList = new List<PassiveSkillCompInfo>();
	    
		
		public SkillRelic()
		{
		}
		
		protected override bool LoadInternal(ulong uid)
		{
			var ret = base.LoadInternal(uid);
			if (ret == false)
			{
				return false;
			}

			var spec = GetSpec();
			
		    try
		    {
				var onPassive = spec.OnPassive;
				
				foreach (var curData in onPassive)
				{
					var targetStr = curData.TargetType;
					var skillCompUid = curData.SkillCompUid;
					var conUid = curData.ConditionCompUid;
					
					var skillCompInfo = SkillCompInfo.CreateOnPassive(Owner, this, targetStr, skillCompUid, conUid);

					if (skillCompInfo != null)
					{
						if (skillCompInfo.SkillCompType == SkillCompType.Active)
						{
							// error
							CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid onPassive Comp");
						}else if (skillCompInfo.SkillCompType == SkillCompType.Continuous)
						{
							PassiveList.Add(skillCompInfo);
						}
					}
				}
		    }
		    catch (Exception e)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid onPassive {0}", spec.Name);
			    return false;
		    }
		    
		    //CorgiLog.LogLine("Load Skill({0}) for {1}", spec.Name, Owner.Name);
			
			// 전투 입장시 active
			//EventManager.Register(CombatEventType.OnEnterCombat, OnActive);

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
			var retLogNode = new RelicActionLogNode(Owner, this, target);

			InvokeSkill(retLogNode);

			OnActive(null, retLogNode);

			//CorgiLog.LogLine("[Skill:{0}] Do Action({1})", _curTime, Name);
			
			return retLogNode;
		}
		
		// component 실행
		bool OnActive(EventParam eventParam, CombatLogNode logNode)
		{
			if (Owner.IsLive() == false || _isActive == true)
			{
				return false;
			}
			
			var passiveLog = new SkillPassiveLogNode(Owner, this);

			passiveLog.AddDetailLog($"Passive Skill On : {Owner.Dungeon.GameData.GetStrByUid(Uid)}");
			
			foreach (var skillCompInfo in PassiveList)
			{
				if (skillCompInfo == null)
				{
					continue;
				}

				logNode.AddDetailLog($"Invoke ContinuousSkillComp : {Owner.Dungeon.GameData.GetStrByUid(skillCompInfo.SkillComp.Uid)}");
				skillCompInfo.Invoke(passiveLog);
				//count++;
			}
			
			logNode.AddChild(passiveLog);

			return true;
		}
		
		bool OnDead(EventParam eventParam, CombatLogNode logNode)
		{
			if (Owner.IsLive() == true)
			{
				return false;
			}

			_isActive = false;

			return true;
		}
	}
}
