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
	public class SkillPassive : Skill
	{
		// 현재 active 됐는지 체크.
		private bool _isActive = false;
		protected bool IsActive => _isActive;

		/// <summary>
	    /// Passive Components
	    /// </summary>
        protected List<PassiveSkillCompInfo> PassiveList = new List<PassiveSkillCompInfo>();
		
		// Buff/Debuff List
		protected List<SkillEffectInst> SkillEffectInsts = new List<SkillEffectInst>();
		
		public SkillPassive()
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
							PassiveList.Add(skillCompInfo as PassiveSkillCompInfo);
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
			EventManager.Register(CombatEventType.OnEnterCombat, OnActive);

			return true;
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
			
			foreach (var effectCompInfo in PassiveList)
			{
				if (effectCompInfo == null)
				{
					continue;
				}

				logNode.AddDetailLog($"Invoke ContinuousSkillComp : {Owner.Dungeon.GameData.GetStrByUid(effectCompInfo.SkillComp.Uid)}");
				effectCompInfo.Invoke(passiveLog);
				//count++;
			}
			
			logNode.AddChild(passiveLog);

			return true;
		}
		
		protected bool OnInActive(CombatLogNode logNode)
		{
			if (Owner.IsLive() == false || IsActive == false)
			{
				return false;
			}

			foreach (var skillInst in SkillEffectInsts)
			{
				if (skillInst == null)
				{
					continue;
				}
				Owner.CancelSkillEffect(skillInst, logNode);
			}

			_isActive = false;
			
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
