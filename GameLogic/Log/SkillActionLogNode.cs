using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLog;
using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Library;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;


namespace IdleCs.GameLog
{
	public class SkillActionLogNode : SkillActorLogNode
	{
		public int ManaCost;

		public string SkillId = string.Empty;

		public ulong CastingTime;
		public SkillResultType ResultType;

		public bool IsChanneling;
		
		public SkillActionLogNode RelicLogNode;
		
		public SkillAttributeType SkillAttribute
		{
			get
			{
				var spec = GameDataManager.Instance.GetData<SkillInfoSpec>(SkillUid);
				return spec?.SkillAttribute ?? SkillAttributeType.SatNone;
			}
		}
		
		public string PlayableDirectorName
		{
			get
			{
				var spec = GameDataManager.Instance.GetData<SkillInfoSpec>(SkillUid);
				return spec?.PlayableDirectorName;
			}
		}


		public SkillActionLogNode()
			: base()
		{
		}

		public SkillActionLogNode(Unit owner, Skill skill, Unit target)
			: base(owner, owner, target)
		{
			if (skill.ObjectId != null)
			{
				SkillId = skill.ObjectId;
			}

			var activeSkill = skill as SkillActive;
			if (activeSkill != null)
			{
				if (activeSkill.IsAreaEffect)
				{
                    SkillAreaType = SkillAreaType.Area;
				}
				else
				{
                    SkillAreaType = SkillAreaType.Single;
				}
			}

			SetSkillActorInfo(skill);
		}

		public override string GetName()
		{
			var spec = GameDataManager.Instance.GetData<SkillInfoSpec>(SkillUid);
			if (spec == null)
			{
				return String.Empty;
			}

			return spec.Name;
		}

		public void ApplyCasting(ulong castingTime)
		{
			CastingTime = castingTime;
		}

		public void ApplyChanneling(ulong channelingTime)
		{
			CastingTime = channelingTime;
			IsChanneling = true;
		}

		public bool HaveLogNode(Type logType)
		{
			foreach (var curLog in Childs)
			{
				if (curLog.GetType().Name == logType.Name)
				{
					return true;
				}
			}

			return false;
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillAction;
	    }
	    
	    public override void GetLog(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
		    base.GetLog(bridge, ref index, logDataList);
		    if (RelicLogNode != null)
		    {
				RelicLogNode.GetLog(bridge, ref index, logDataList);
			    
		    }
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(ManaCost);
	        writer.Write(SkillId);
	        writer.Write(CastingTime);
	        
	        writer.WriteEnum(ResultType);
	        writer.Write(IsChanneling);

	        if (RelicLogNode == null)
	        {
                writer.Write((int)CombatLogNodeType.None);
	        }
	        else
	        {
				RelicLogNode.Serialize(writer);
	        }

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out ManaCost);
	        
	        reader.Read(out SkillId);
	        reader.Read(out CastingTime);
	        
	        reader.ReadEnum(out ResultType);
	        reader.Read(out IsChanneling);
	        
	        
	        CorgiCombatLog.Log(CombatLogCategory.Skill,"({0}) Invoked Result({1})", GameDataManager.GetStringByUid(SkillUid), ResultType.ToString());

	        RelicLogNode = CombatLogNode.DeSerializeStatic(reader) as SkillActionLogNode;
	        
	        return this;
        }
        
	    public override void SetRoot(DungeonLogNode dungeonLogNode)
	    {
		    base.SetRoot(dungeonLogNode);

		    if (RelicLogNode != null)
		    {
			    RelicLogNode.SetParent(this);
			    RelicLogNode.SetRoot(dungeonLogNode);
		    }
	    }

		public override void LogDebug(IGameDataBridge bridge)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			if (SkillType == SkillType.Active)
			{
				if (CastingTime > 0)
				{
					CorgiCombatLog.Log(CombatLogCategory.Skill,"Skill Start Casting ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name,
						ManaCost, CastingTime);

				}
				else
				{
					CorgiCombatLog.Log(CombatLogCategory.Skill,"Skill Action ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name, ManaCost,
						CastingTime);

				}

			}

			base.LogDebug(bridge);
		}

		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
			var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

			if (skillSpec == null || caster == null || target == null)
			{
				throw new CorgiException("invalid action log node");
			}

			var casterName = caster.GetName(bridge);
			var targetName = target.GetName(bridge);

			var logType = SkillType == SkillType.Attack ? CombatLogType.Attack : CombatLogType.SkillActive;

			var combatLogData = new CombatLogData(logType, index, this);
			index++;

			if (SkillType == SkillType.Attack)
			{
				combatLogData.Message = string.Format("{0} Attack to {1}"
					, casterName, targetName);

			}
			else
			{
				if (CastingTime > 0)
				{
					combatLogData.Message = string.Format(
						"{0} DoSkill Casting to {1} ({2}) : Mana({3}), CastingTime({4})"
						, casterName, targetName, skillSpec?.Name, ManaCost, CastingTime);
				}
				else
				{
					combatLogData.Message = string.Format("{0} DoSkill to {1} ({2}) : Mana({3})"
						, casterName, targetName, skillSpec?.Name, ManaCost);
				}

			}

			logDataList.Add(combatLogData);

		}

//		[OnDeserialized]
//		private void OnDeserialized(StreamingContext context)
//		{
//			if (SkillType == SkillType.Active)
//			{
//				CorgiLog.LogLine("Action Log Type : {0}/{1}", SkillType.ToString(), SkillUid);
//			}
//		}

	}


	[System.Serializable]
	public class CancelCastingLogNode : SkillActionLogNode
	{
		public CancelCastingLogNode()
		{
		}

		public CancelCastingLogNode(Unit owner, Skill skill, Unit target)
			: base(owner, skill, target)
		{
			ResultType = SkillResultType.SkillResultInterrupted;
		}

		public override void LogDebug(IGameDataBridge bridge)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			CorgiCombatLog.Log(CombatLogCategory.Skill,"Action Cancel ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name, ManaCost,
				CastingTime);

			base.LogDebug(bridge);
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillActionCancelCasting;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        return this;
        }
	}
	
	[System.Serializable]
	public class SkillInvokeActionLogNode : SkillActionLogNode
	{
		public SkillInvokeActionLogNode()
		{
		}

		public SkillInvokeActionLogNode(Unit owner, Skill skill, Unit target)
			: base(owner, skill, target)
		{
			ResultType = skill.GetSpec().SkillResultType;
		}

		public override void LogDebug(IGameDataBridge bridge)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			 CorgiCombatLog.Log(CombatLogCategory.Skill,"Skill Invoke Action ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name, ManaCost,
			 	CastingTime);

			base.LogDebug(bridge);
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillInvokeAction;
	    }

	    public List<SkillCompResultStruct> GetSkillResultStruct()
	    {
		    uint dealTargetCount = 0;
		    ulong deal = 0;
		    uint healTargetCount = 0;
		    ulong heal = 0;

		    var resultList = new List<SkillCompResultStruct>();
		    var continuousList = new List<SkillCompResultStruct>();

		    var continuousBuffMap = new Dictionary<ulong, uint>();
		    var continuousDebuffMap = new Dictionary<ulong, uint>();

		    foreach (var curLog in Childs)
		    {
			    var skillCompLog = curLog as SkillCompLogNode;
			    if (skillCompLog == null)
			    {
				    continue;
			    }

			    var logType = skillCompLog.GetLogType();
			    if (logType == SkillCompResultType.Deal)
			    {
				    var damageLog = curLog as DamageSkillCompLogNode;
				    if (damageLog == null)
				    {
					    continue;
				    }
				    dealTargetCount += 1;
				    deal += damageLog.FinalDamage;
			    }else if (logType == SkillCompResultType.Heal)
			    {
				    var healLog = curLog as HealSkillCompLogNode;
				    if (healLog == null)
				    {
					    continue;
				    }
				    healTargetCount += 1;
				    heal += healLog.FinalHeal;
			    }else if (logType == SkillCompResultType.Buff)
			    {
				    var addContinuousLog = curLog as AddContinuousLogNode;
				    if (addContinuousLog == null)
				    {
					    continue;
				    }

				    var uid = addContinuousLog.SkillCompUid;
				    if (continuousBuffMap.ContainsKey(uid) == false)
				    {
					    continuousBuffMap.Add(uid, 1);
				    }
				    else
				    {
					    continuousBuffMap[uid] += 1;
				    }
				    
			    }else if (logType == SkillCompResultType.Debuff)
			    {
				    var addContinuousLog = curLog as AddContinuousLogNode;
				    if (addContinuousLog == null)
				    {
					    continue;
				    }

				    var uid = addContinuousLog.SkillCompUid;
				    if (continuousDebuffMap.ContainsKey(uid) == false)
				    {
					    continuousDebuffMap.Add(uid, 1);
				    }
				    else
				    {
					    continuousDebuffMap[uid] += 1;
				    }
			    }
		    }
		    if (dealTargetCount > 0)
		    {
				var logStruct = new SkillCompResultStruct(SkillCompResultType.Deal);
				var langStr = TextDataManager.Instance.GetText("lang.skill.timeline.active.damage.desc");
				logStruct.ResultDesc = CorgiString.Format(langStr, dealTargetCount, deal);
				resultList.Add(logStruct);
		    }
		    
		    if (healTargetCount > 0)
		    {
				var logStruct = new SkillCompResultStruct(SkillCompResultType.Heal);
				var langStr = TextDataManager.Instance.GetText("lang.skill.timeline.active.heal.desc");
				logStruct.ResultDesc = CorgiString.Format(langStr, healTargetCount, heal);
				resultList.Add(logStruct);
		    }

		    foreach (var continuousData in continuousBuffMap)
		    {
			    var uid = continuousData.Key;
			    var count = continuousData.Value;
			    var sheetData = GameDataManager.Instance.GetData<SkillContinuousInfoSpec>(uid);
			    if (sheetData == null)
			    {
				    continue;
			    }
			    
				var logStruct = new SkillCompResultStruct(SkillCompResultType.Buff);
				var langStr = TextDataManager.Instance.GetText("lang.skill.timeline.continuous.buff.desc");
				logStruct.ResultDesc = CorgiString.Format(langStr, count, sheetData.Name);
				resultList.Add(logStruct);

				if (sheetData.VisibleInfo == false)
				{
					continue;
				}
				var continuousLogStruct = new SkillCompResultStruct(SkillCompResultType.Buff);
				logStruct.ResultName = sheetData.Name;
				logStruct.ResultDesc = sheetData.Desc;
				continuousList.Add(continuousLogStruct);
		    }
		    
		    foreach (var continuousData in continuousDebuffMap)
		    {
			    var uid = continuousData.Key;
			    var count = continuousData.Value;
			    var sheetData = GameDataManager.Instance.GetData<SkillContinuousInfoSpec>(uid);
			    if (sheetData == null)
			    {
				    continue;
			    }
			    
				var logStruct = new SkillCompResultStruct(SkillCompResultType.Debuff);
				var langStr = TextDataManager.Instance.GetText("lang.skill.timeline.continuous.debuff.desc");
				logStruct.ResultDesc = CorgiString.Format(langStr, count, sheetData.Name);
				resultList.Add(logStruct);
				
				if (sheetData.VisibleInfo == false)
				{
					continue;
				}
				var continuousLogStruct = new SkillCompResultStruct(SkillCompResultType.Buff);
				logStruct.ResultName = sheetData.Name;
				logStruct.ResultDesc = sheetData.Desc;
				continuousList.Add(continuousLogStruct);
		    }

		    resultList.AddRange(continuousList);

		    return resultList;
	    }
	    
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        return this;
        }

        // for debug
		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			if (SkillType != SkillType.Casting && SkillType != SkillType.Channeling)
			{
				return;
			}

			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
			var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

			if (skillSpec == null || caster == null || target == null)
			{
				throw new CorgiException("invalid action log node");
			}

			var casterName = caster.GetName(bridge);
			var targetName = target.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} Invoked {1} ({2}-{3}) : Mana({4}), CastingTime({5})"
				, casterName, targetName, ResultType.ToString(), skillSpec?.Name, ManaCost, CastingTime);
			logDataList.Add(combatLogData);

		}
	}

	[System.Serializable]
	public class CompleteCastingLogNode : SkillActionLogNode
	{
		public CompleteCastingLogNode()
		{
		}

		public CompleteCastingLogNode(Unit owner, Skill skill, Unit target)
			: base(owner, skill, target)
		{
		}

		public override void LogDebug(IGameDataBridge bridge)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			CorgiCombatLog.Log(CombatLogCategory.Skill,"Skill Casting Complete ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name, ManaCost,
				CastingTime);

			base.LogDebug(bridge);
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillActionCompleteCasting;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        return this;
        }

		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			if (SkillType != SkillType.Casting && SkillType != SkillType.Channeling)
			{
				return;
			}

			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
			var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

			if (skillSpec == null || caster == null || target == null)
			{
				throw new CorgiException("invalid action log node");
			}

			var casterName = caster.GetName(bridge);
			var targetName = target.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} Casting Complete {1} ({2}) : Mana({3}), CastingTime({4})"
				, casterName, targetName, skillSpec?.Name, ManaCost, CastingTime);
			logDataList.Add(combatLogData);

		}
	}

	[System.Serializable]
	public class ChannelingTickLogNode : SkillActionLogNode
	{
		public ulong CurChennelingTick = 0;
		public ulong MaxChennelingTick = 0;

		public ChannelingTickLogNode()
			: base()
		{
		}
		
		public ChannelingTickLogNode(Unit owner, Skill skill, Unit target)
			: base(owner, skill, target)
		{
			IsChanneling = true;
		}

		public void ApplyChannelingTick(ulong curTick, ulong maxTick)
		{
			CurChennelingTick = curTick;
			MaxChennelingTick = maxTick;
		}

		public override void LogDebug(IGameDataBridge bridge)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			CorgiCombatLog.Log(CombatLogCategory.Skill,"Action Channeling ({0}) : Mana({1}), CastingTime({2})", skillSpec?.Name, ManaCost,
				CastingTime);

			base.LogDebug(bridge);
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillActionChannelingTick;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(CurChennelingTick);
	        writer.Write(MaxChennelingTick);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }

	        reader.Read(out CurChennelingTick);
	        reader.Read(out MaxChennelingTick);
	        
	        return this;
        }
		
		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			if (SkillType != SkillType.Channeling)
			{
				return;
			}

			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
			var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

			if (skillSpec == null || caster == null || target == null)
			{
				//throw new CorgiException("invalid action log node");
				return;
			}

			var casterName = caster.GetName(bridge);
			var targetName = target.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} Channeling to {1} ({2}) : Time({3}/{4})"
				, casterName, targetName, skillSpec?.Name, CurChennelingTick, MaxChennelingTick);
			logDataList.Add(combatLogData);

		}

	}

	public class RelicActionLogNode : SkillActionLogNode 
	{
		private ulong SkillUid;
		
		public RelicActionLogNode()
			: base()
		{
		}

		public RelicActionLogNode(Unit owner, Skill skill, Unit target)
			: base(owner , skill, target)
		{
			SkillUid = skill.Uid;
		}

		public override void LogDebug(IGameDataBridge bridge)
		{
			//var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			CorgiCombatLog.Log(CombatLogCategory.Relic,"Action Relic ({0})" );

			base.LogDebug(bridge);
		}

		public override int GetClassType()
		{
			return (int) CombatLogNodeType.RelicSkillAction;
		}

		public override bool Serialize(IPacketWriter writer)
		{
			if (base.Serialize(writer) == false)
			{
				return false;
			}

			writer.Write(SkillUid);

			return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}

			reader.Read(out SkillUid);

			return this;
		}
		
		public override string GetName()
		{
			var spec = GameDataManager.Instance.GetData<SkillInfoSpec>(SkillUid);
			if (spec == null)
			{
				return String.Empty;
			}

			return spec.Name;
		}

		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
			var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

			if (skillSpec == null || caster == null || target == null)
			{
				//throw new CorgiException("invalid action log node");
				return;
			}

			var casterName = caster.GetName(bridge);
			var targetName = target.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} Use(Relic) to {1} ({2}))"
				, casterName, targetName, skillSpec?.Name);
			logDataList.Add(combatLogData);

		}
	}

}
