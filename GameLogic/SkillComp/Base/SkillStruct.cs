using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace IdleCs.GameLogic
{
	public class SkillCompInfo
	{
		public Unit Owner;
		public SkillCompType SkillCompType;
		public SkillTargetComp TargetComp;
		public List<ConditionComp> ConditionComps;

		public virtual bool Init(Unit owner, ISkillActor skillActor, string targetStr, ulong uid, ulong conUid)
		{
			if (string.IsNullOrEmpty(targetStr) || uid == 0)
			{
				return false;
			}

			Owner = owner;
			
			var curTargetType = CorgiEnum.ParseEnum<SkillTargetType>(targetStr);
				  
			TargetComp = SkillTargetCompFactory.Create(owner, curTargetType);

			if (conUid != 0)
			{
				ConditionComps = SkillConditionCompFactory.Create(conUid, owner);
			}

			return true;
		}

		public virtual bool Invoke(CombatLogNode logNode)
		{
			return false;
		}

		public static SkillCompInfo CreateOnActive(Unit owner, ISkillActor skillActor, string targetStr, ulong skillCompUid, ulong conUid)
		{
			if (string.IsNullOrEmpty(targetStr) || skillCompUid == 0)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid OnActive Json");
				return null;
			}
		
			var sheet0 = owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(skillCompUid);
			if (sheet0 != null)
			{
				var skillCompInfo = new ActiveSkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, skillCompUid, conUid))
				{
					return skillCompInfo;
				}

				return null;
			}
			
			var sheet1 = owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(skillCompUid);
			if (sheet1 != null)
			{
				var skillCompInfo = new EffectSkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, skillCompUid, conUid))
				{
					return skillCompInfo;
				}

				return null;
			}
			
			var sheet2 = owner.Dungeon.GameData.GetData<SkillArrayCompInfoSpec>(skillCompUid);
			if (sheet2 != null)
			{
				var skillCompInfo = new ArraySkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, skillCompUid, conUid))
				{
					return skillCompInfo;
				}

				return null;
			}

			return null;
		}
		
		public static PassiveSkillCompInfo CreateOnPassive(Unit owner, ISkillActor skillActor, string targetStr, ulong skillCompUid, ulong conUid)
		{
			if (string.IsNullOrEmpty(targetStr) || skillCompUid == 0)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid OnActive Json");
				return null;
			}
		
			var sheet0 = owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(skillCompUid);
			if (sheet0 != null)
			{
				var skillCompInfo = new PassiveSkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, skillCompUid, conUid))
				{
					return skillCompInfo;
				}

				return null;
			}

			return null;
		}

		public static OnEventSkillCompInfo CreateOnEvent
			(Unit owner, ISkillActor skillActor, string eventTypeStr, string targetStr, ulong uid, ulong conUid)
		{
			if (string.IsNullOrEmpty(eventTypeStr) || string.IsNullOrEmpty(targetStr) || uid == 0)
			{
				return null;
			}
			
			var eventInfo = new OnEventSkillCompInfo();

			if (eventInfo.Init(owner, skillActor, eventTypeStr, targetStr, uid, conUid))
			{
				return eventInfo;
			}

			return null;
		}
	}
	
    public class ActiveSkillCompInfo : SkillCompInfo
	{
		public ActiveSkillComp SkillComp;

		public override bool Init(Unit owner, ISkillActor skillActor, string targetStr, ulong uid, ulong conUid)
		{
			SkillCompType = SkillCompType.Active;
			if (base.Init(owner, skillActor, targetStr, uid, conUid) == false)
			{
				return false;
			}

			SkillComp = SkillCompFactory.CreateActive(uid, owner, skillActor);
			//FollowingSkillComp = FollowingSkillCompFactory.CreateActive(assetStruct.FollowingAsset, owner, skillActor);
			
			if (TargetComp == null || SkillComp == null)
			{
				return false;
			}

			//SkillComp.IgnoreEvent = true;
				
			return true;
		}
		public override bool Invoke(CombatLogNode logNode)
		{
            if (SkillComp == null || TargetComp == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ActiveSkillComp Info");
                return false;
            }

            var targetList = TargetComp.GetTargetList(logNode);

            if (targetList == null)
            {
                return false;
            }

            SkillComp.DoApply(targetList, ConditionComps, logNode);
            logNode.AddDetailLog($"Invoke ActiveSkillComp : {SkillComp.GetName()}");
			return true;
		}

		public bool IsAreaEffect()
		{
			if (SkillComp.CanAreaEffect == false)
			{
				return false;
			}

			if (TargetComp != null && TargetComp.IsAreaEffect)
			{
				return true;
			}

			return false;
		}
	}
	
	public class EffectSkillCompInfo : SkillCompInfo
	{
		public ContinuousSkillComp SkillComp;
		//public EffectFollowingSkillComp FollowingSkillComp;
		
		public override bool Init(Unit owner, ISkillActor skillActor, string targetStr, ulong uid, ulong conUid)
		{
			SkillCompType = SkillCompType.Continuous;
			if (base.Init(owner, skillActor, targetStr, uid, conUid) == false)
			{
				return false;
			}
			
			SkillComp = SkillCompFactory.CreateEffect(uid, owner, skillActor);
			//FollowingSkillComp = FollowingSkillCompFactory.CreateActive(assetStruct.FollowingAsset, owner, skillActor);
			
			if (TargetComp == null || SkillComp == null)
			{
				return false;
			}
			
			//SkillComp.IgnoreEvent = true;
				
			return true;
		}
		
		public override bool Invoke(CombatLogNode logNode)
		{
            if (SkillComp == null || TargetComp == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ContinuousSkillComp Info");
                return false;
            }

            var targetList = TargetComp.GetTargetList(logNode);

            if (targetList == null)
            {
                return false;
            }

            if (SkillComp.DoApply(targetList, ConditionComps, logNode) == false)
            {
	            return false;
            }
            logNode.AddDetailLog($"Invoke ContinuousSkillComp : {SkillComp.GetName()}");
			return true;
		}
	}
	
	public class PassiveSkillCompInfo : SkillCompInfo
	{
		public ContinuousSkillComp SkillComp;
		//public EffectFollowingSkillComp FollowingSkillComp;
		
		public override bool Init(Unit owner, ISkillActor skillActor, string targetStr, ulong uid, ulong conUid)
		{
			SkillCompType = SkillCompType.Continuous;
			if (base.Init(owner, skillActor, targetStr, uid, conUid) == false)
			{
				return false;
			}
			
			SkillComp = SkillCompFactory.CreateEffect(uid, owner, skillActor);
			//FollowingSkillComp = FollowingSkillCompFactory.CreateActive(assetStruct.FollowingAsset, owner, skillActor);
			
			if (TargetComp == null || SkillComp == null)
			{
				return false;
			}
			
			
			//SkillComp.IgnoreEvent = true;
				
			return true;
		}
		
		public override bool Invoke(CombatLogNode logNode)
		{
            if (SkillComp == null || TargetComp == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ContinuousSkillComp Info");
                return false;
            }

            if (SkillComp.DoApplyAura(Owner, TargetComp, ConditionComps, logNode) == false)
            {
	            return false;
            }
            logNode.AddDetailLog($"Invoke ContinuousSkillComp : {SkillComp.GetName()}");
			return true;
		}
	}
	
    public class ArraySkillCompInfo : SkillCompInfo
    {
	    public ulong ArrayCompUid;
		public List<ActiveSkillComp> ActiveSkillComps = new List<ActiveSkillComp>();
		public List<ContinuousSkillComp> ContinuousSkillComps = new List<ContinuousSkillComp>();

		public override bool Init(Unit owner, ISkillActor skillActor, string targetStr, ulong uid, ulong conUid)
		{
			SkillCompType = SkillCompType.Array;
			if (base.Init(owner, skillActor, targetStr, uid, conUid) == false)
			{
				return false;
			}
			
			var sheet = owner.Dungeon.GameData.GetData<SkillArrayCompInfoSpec>(uid);

			if (sheet == null)
			{
				return false;
			}

			ArrayCompUid = uid;

			foreach (var curUid in sheet.ActiveUids)
			{
				var skillComp = SkillCompFactory.CreateActive(curUid, owner, skillActor);
				if (skillComp == null)
				{
					continue;
				}

				ActiveSkillComps.Add(skillComp);
			}
			
			foreach (var curUid in sheet.ContinuousUids)
			{
				var skillComp = SkillCompFactory.CreateEffect(curUid, owner, skillActor);
				if (skillComp == null)
				{
					continue;
				}

				ContinuousSkillComps.Add(skillComp);
			}

			if (TargetComp == null || (ActiveSkillComps.Count == 0 && ContinuousSkillComps.Count == 0))
			{
				return false;
			}

			return true;
		}
		public override bool Invoke(CombatLogNode logNode)
		{
			if (TargetComp == null || (ActiveSkillComps.Count == 0 && ContinuousSkillComps.Count == 0))
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid ArraySkillComp Info");
                return false;
            }

            var targetList = TargetComp.GetTargetList(logNode);

            if (targetList == null)
            {
                return false;
            }

            logNode.AddDetailLog($"Invoke ArraySkillComp ");
            foreach (var curSkillComp in ActiveSkillComps)
            {
	            if (curSkillComp == null)
	            {
		            continue;
	            }
	            
				curSkillComp.DoApply(targetList, ConditionComps, logNode);
				logNode.AddDetailLog($"Invoke ActiveSkillComp : {curSkillComp.GetName()}");
            }
            foreach (var curSkillComp in ContinuousSkillComps)
            {
	            if (curSkillComp == null)
	            {
		            continue;
	            }
	            
				curSkillComp.DoApply(targetList, ConditionComps, logNode);
				logNode.AddDetailLog($"Invoke ContinuousSkillComp : {curSkillComp.GetName()}");
            }

			return true;
		}
	}
    
	public class OnEventSkillCompInfo
	{
		public CombatEventType EventType;
		public SkillCompInfo SkillCompInfo;
		
		//public FollowingSkillComp FollowingSkillComp;
		
		public bool Init(Unit owner, ISkillActor skillActor, string eventTypeStr, string targetStr, ulong uid, ulong conUid)
		{
			EventType = CorgiEnum.ParseEnum<CombatEventType>(eventTypeStr);

			if (EventType == CombatEventType.EventNone)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid event type string({0}), in skillActor({1})", eventTypeStr, owner.Name);
				return false;
			}
		
			var sheet0 = owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);
			if (sheet0 != null)
			{
				var skillCompInfo = new ActiveSkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, uid, conUid))
				{
					skillCompInfo.SkillComp.IgnoreEvent = true;
					if (EventType == CombatEventType.OnSkillEffectTick)
					{
						skillCompInfo.SkillComp.IgnoreCritical = true;
					}
					SkillCompInfo = skillCompInfo;
					return true;
				}

				return false;
			}
			
			var sheet2 = owner.Dungeon.GameData.GetData<SkillContinuousInfoSpec>(uid);
			if (sheet2 != null)
			{
				var skillCompInfo = new EffectSkillCompInfo();
				if (skillCompInfo.Init(owner, skillActor, targetStr, uid, conUid))
				{
					skillCompInfo.SkillComp.IgnoreEvent = true;
					SkillCompInfo = skillCompInfo;
					return true;
				}

				return false;
			}

			return false;
		}
	}
	
}
