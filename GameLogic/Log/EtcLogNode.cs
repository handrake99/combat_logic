using System;
using System.Collections.Generic;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

//
namespace IdleCs.GameLog
{
	public class AbsorbDamageLogNode : SkillCompLogNode
	{
		public string SkillEffectId;
		public int AbsorbAmount;

		public override SctMessageInfo SctMessageInfo => new SctMessageInfo(SctMessageType.Absorb, false, IsImmune, null, $"({AbsorbAmount})");

		public AbsorbDamageLogNode()
		{}
		public AbsorbDamageLogNode(SkillEffectInst skillInst, SkillComp skillComp, int absorbAmount)
			: base(skillInst.Caster, skillInst.Target, skillComp)
		{
			SkillEffectId = skillInst.ObjectId;
			AbsorbAmount = absorbAmount;
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.AbsorbDamage;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(SkillEffectId);
	        writer.Write(AbsorbAmount);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out SkillEffectId);
	        reader.Read(out AbsorbAmount);
	        
	        return this;
        }
	}
	
	public class ChangeCurHPPercentSelfLogNode : SkillCompLogNode
	{
		public long DiffHP; // 바뀐 HP량
		
		
	    public long PreHP; // 적용전 HP
	    public long CurHP; // 적용후 HP
	    
		public ChangeCurHPPercentSelfLogNode()
		{}
	    public ChangeCurHPPercentSelfLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.ChangeCurHPSelf;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(DiffHP);
	        writer.Write(PreHP);
	        writer.Write(CurHP);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out DiffHP);
	        reader.Read(out PreHP);
	        reader.Read(out CurHP);
	        
	        return this;
        }
	}
	
	public class SummonLogNode : SkillCompLogNode
	{
		public CorgiStringList SummonedMonsters = new CorgiStringList();
		
		public SummonLogNode()
		{}
	    public SummonLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Summon;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }

	        SummonedMonsters.Serialize(writer);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        SummonedMonsters.DeSerialize(reader);
	        
	        return this;
        }
	}
	
	public class ChangeContinuousDurationLogNode : SkillCompLogNode
	{
		public CorgiStringList ChangedContinuousIds = new CorgiStringList();
		public long ChangedDuration = 0;
		
		public ChangeContinuousDurationLogNode()
		{}
	    public ChangeContinuousDurationLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.ChangeContinuousDuration;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }

	        ChangedContinuousIds.Serialize(writer);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        ChangedContinuousIds.DeSerialize(reader);
	        
	        return this;
        }
        
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid action log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);
            
            var combatLogData = new CombatLogData(CombatLogType.ActiveSkillComp, index, this);

			combatLogData.Message = string.Format("{0} ChangeDuration {1}({2}) to {3}", casterName, ChangedDuration, ChangedContinuousIds.Count, targetName);
            index++;
			
			logDataList.Add(combatLogData);
			
	    }
	}

//    [Serializable]
//	public class ImmuneLogNode : SkillCompLogNode
//	{
//
//		public ImmuneLogNode(DungeonLogType logType, ISkillComp skillComp, IUnit target)
//			: base(logType, skillComp, target)
//		{
//		}
//
//		public override string GetDesc()
//		{
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//
//			return CorgiString.Format(baseDesc, _target.NameForLog, SkillComp.Name);
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisNodeDelay = nodeDelay;
//			float retDelay = nodeDelay;
//
//			if (_targetIndex != -1)
//			{
//				long skillCode = -1;
//				
//				if(this.ParentActor is Skill)
//				{
//					Skill skill = this.ParentActor as Skill;
//					if(skill != null)
//					{
//						skillCode = skill.Code;
//						
//						if(skill.ActivateType != ActionActivateType.Active)
//							return retDelay;
//					}
//				}
//				else if(this.ParentActor is SkillInst)
//				{
//					SkillInst skillInst = this.ParentActor as SkillInst;
//					if(skillInst != null)
//					{
//						skillCode = skillInst.Code;
//						
//						if(skillInst.ParentActor is Skill)
//						{
//							Skill skill = skillInst.ParentActor as Skill;
//							if(skill.ActivateType != ActionActivateType.Active)
//								return retDelay;
//						}
//					}
//				}
//					
//				if(skillCode > 0)
//				{
//					string ret = CorgiStaticStringData.Instance.GetString(LangStringType.system, "immune");
//					string immuneStr = "";
//					if(this.ParentActor.Name.Equals("Invalid Skill Name") || this.ParentActor.Name.Equals("Invalid Name"))
//					{
//						immuneStr = CorgiString.Format(ret, skillCode);
//					}
//					else
//						immuneStr = CorgiString.Format(ret, this.ParentActor.Name);
//	
//					TextUIMessage msg = new TextUIMessage(TextUIType.Info, immuneStr);
//					base.ProcessLogNodeText(manager, msg, _logType, ref thisNodeDelay);
//	
//					retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//				}
//			}
//
//			return retDelay;
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//
//	}
//
//    [Serializable]
//	public class ResistLogNode : SkillCompLogNode
//	{
//		public ResistLogNode(DungeonLogType logType, ISkillComp skillComp, IUnit target)
//			: base(logType, skillComp, target)
//		{
//		}
//
//		public override string GetDesc()
//		{
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//
//			return CorgiString.Format(baseDesc, _target.NameForLog, SkillComp.Name);
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisNodeDelay = nodeDelay;
//			float retDelay = base.ProcessLogNodeEffect(manager, ref nodeDelay);
//			//float textDelay = 0f;
//
//			if (_targetIndex != -1)
//			{
//				string ret = CorgiStaticStringData.Instance.GetString(LangStringType.system, "resist");
//				//string sctStr = CorgiColor.BBC_COLOR_MUSTARD + CorgiString.Format(ret, SkillComp.Name) + CorgiColor.BBC_COLOR_END;
//				string sctStr = CorgiString.Format(ret, SkillComp.Name);
//
//				TextUIMessage msg = new TextUIMessage(TextUIType.Info, sctStr);
//				base.ProcessLogNodeText(manager, msg, _logType, ref thisNodeDelay);
//
//				retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return retDelay;
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//			thisObject.AddField("unitIndex", Target.UnitIndex);
//			thisObject.AddField("code", SkillComp.Code);
//		}
//	}
//
//
//    [Serializable]
//	public class LuckLogNode : CombatLogNode
//	{
//		IUnit _unit;
//
//		public LuckLogNode(DungeonLogType logType, IUnit unit)
//			: base(logType)
//		{
//			_unit = unit;
//		}
//
//		public override string GetDesc()
//		{
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//			return CorgiString.Format(baseDesc, _unit.NameForLog);
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//			thisObject.AddField("unitIndex", _unit.UnitIndex);
//		}
//	}
//	
//	[Serializable]
//	public class SwitchCharacterLogNode : CombatLogNode
//	{
//		IUnit _exitUnit;
//		IUnit _enterUnit;
//
//		public SwitchCharacterLogNode(DungeonLogType logType, IUnit exitUnit, IUnit enterUnit)
//			: base(logType)
//		{
//			_exitUnit = exitUnit;
//			_enterUnit = enterUnit;
//		}
//
//		public override string GetDesc()
//		{
//			string ret = CorgiStaticStringData.Instance.GetString(LangStringType.combat, "changeMember");
//			
//			if(string.IsNullOrEmpty(ret) == false)
//				return CorgiString.Format(ret, _exitUnit.NameForLog, _enterUnit.NameForLog);
//			else
//				return CorgiString.Format("{0} -> {1}", _exitUnit.NameForLog, _enterUnit.NameForLog);
//		}
//		
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//			thisObject.AddField("unitIndex", _exitUnit.UnitIndex);
//			thisObject.AddField("unitIndex2", _enterUnit.UnitIndex);
//		}
//	}
//	
//	[Serializable]
//	public class RaidJoinCharacterLogNode : CombatLogNode
//	{
//		long _joinCharacterCode;
//		string _userId;
//		string _nickname;
//		
//		public RaidJoinCharacterLogNode(DungeonLogType logType, long joinCharacterCode, string userId, string nickname)
//			:base(logType)
//		{
//			_joinCharacterCode = joinCharacterCode;
//			_userId = userId;
//			_nickname = nickname;
//		}
//		
//		public override string GetDesc ()
//		{
//			string ret = CorgiStaticStringData.Instance.GetString(LangStringType.combat, "guildRaidAdd");
//			
//			string characterName = "invalid name";
//			SpecCharacterInfo specCharInfo = (SpecCharacterInfo)CorgiStaticData.Instance.GetData("characterInfo", _joinCharacterCode);
//			if(specCharInfo != null)
//			{
//				characterName = specCharInfo.Name;
//			}
//			
//			if(string.IsNullOrEmpty(ret) == false)
//			{
//				return CorgiString.Format(ret, _nickname, characterName);
//			}
//			else
//			{
//				return "invalid RaidJoinCharacterLogNode.GetDesc";
//			}
//		}
//		
//		public override float ProcessLogNodeEffect (ICombatUIInterface manager, ref float delay)
//		{
//			manager.OnGuildRaidActionLogEvent(DungeonLogType.RaidJoin, _joinCharacterCode, _userId, _nickname, delay);
//			
//			delay += 0.75f;
//			return base.ProcessLogNodeEffect (manager, ref delay);
//		}
//	}
//	
//	[Serializable]
//	public class BlessingLogNode : CombatLogNode
//	{
//		long _blessCode;
//		
//		public BlessingLogNode(DungeonLogType logType, long blessCode)
//			:base(logType)
//		{
//			_blessCode = blessCode;
//		}
//		
//		public override string GetDesc ()
//		{
//			string ret = CorgiStaticStringData.Instance.GetString(LangStringType.combat, "blessing");
//			string strDesc = "";
//			
//			SpecBlessingInfo blessingInfo = CorgiLogicHelper.GetBlessingInfo(_blessCode);
//			if(blessingInfo != null)
//			{
//				if(string.IsNullOrEmpty(ret))
//					strDesc = string.Format("#{0}", blessingInfo.name);
//				else
//					strDesc = CorgiString.Format(ret, blessingInfo.name);
//			}
//			else
//			{
//				SpecGuildBlessingInfo guildBlessingInfo = CorgiLogicHelper.GetGuildBlessingInfo(_blessCode);
//				if(guildBlessingInfo != null)
//				{
//					if(string.IsNullOrEmpty(ret))
//						strDesc = string.Format("#{0}", guildBlessingInfo.name);
//					else
//						strDesc = CorgiString.Format(ret, guildBlessingInfo.name);
//				}
//			}
//			return strDesc;
//		}
//	}
//	
//	[Serializable]
//	public class ParryLogNode : SkillCompLogNode
//	{
//		int _amount;
//		
//		public ParryLogNode(DungeonLogType logType, ISkillComp skillComp, IUnit target, int amount)
//			: base(logType, skillComp, target)
//		{
//			_amount = amount;
//		}
//
//		public override string GetDesc()
//		{
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//
//			return CorgiString.Format(baseDesc, _target.NameForLog, _amount);
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisNodeDelay = nodeDelay;
//			float retDelay = nodeDelay;
//
//			if (_targetIndex != -1)
//			{
//				string ret = CorgiStaticStringData.Instance.GetString(LangStringType.SCT, "Parry");
//				if(string.IsNullOrEmpty(ret))
//				{
//					ret = "Parry";
//				}
//				
//				TextUIMessage msg = new TextUIMessage(TextUIType.Info, ret);
//				base.ProcessLogNodeText(manager, msg, _logType, ref thisNodeDelay);
//
//				retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return retDelay;
//		}
//	}
//	
//	[Serializable]
//	public class BitterLogNode : CombatLogNode
//	{
//		IUnit _unit;
//		
//		public BitterLogNode(DungeonLogType logType, IUnit target)
//			: base(logType)
//		{
//			_unit = target;
//		}
//
//		public override string GetDesc()
//		{
//			return null;
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisNodeDelay = nodeDelay;
//			float retDelay = nodeDelay;
//
//			if (_unit != null)
//			{
//				string ret = CorgiStaticStringData.Instance.GetString(LangStringType.system, "BitterRating");
//				if(string.IsNullOrEmpty(ret))
//				{
//					ret = "Bitter";
//				}
//				
//				TextUIMessage msg = new TextUIMessage(TextUIType.Info, ret);
//				manager.ShowSCT(null, ActionInput.UnitIndex, ActionInput.UnitIndex, msg, DungeonLogType.Bitter, ref nodeDelay, 0);
//
//				retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return retDelay;
//		}
//	}
//	
//	[Serializable]
//	public class CounterLogNode : SkillActionLogNode
//	{
//		public CounterLogNode(DungeonLogType logType, IUnit unit, IAction action, ActionInput input)
//			: base(logType, unit, action, input)
//		{
//		}
//
//		public override string GetDesc()
//		{ 
//			return null;
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisNodeDelay = nodeDelay;
//			float retDelay = nodeDelay;
//
//			if (_unit != null)
//			{
//				string ret = CorgiStaticStringData.Instance.GetString(LangStringType.system, "CounterRating");
//				if(string.IsNullOrEmpty(ret))
//				{
//					ret = "Counter";
//				}
//				
//				TextUIMessage msg = new TextUIMessage(TextUIType.Info, ret);
//				manager.ShowSCT(null, ActionInput.UnitIndex, ActionInput.UnitIndex, msg, DungeonLogType.Bitter, ref nodeDelay, 0);
//
//				retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return retDelay;
//		}
//	}
//	
//	[Serializable]
//	public class HidingLogNode : CombatLogNode
//	{
//		IUnit _unit;
//
//		public HidingLogNode(DungeonLogType logType, IUnit unit)
//			: base(logType)
//		{
//			_unit = unit;
//		}
//
//		public override string GetDesc()
//		{
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//			return CorgiString.Format(baseDesc, _unit.NameForLog);
//		}
//		
//		public override float ProcessLogNodeEffect (ICombatUIInterface manager, ref float delay)
//		{
//			float resDelay = base.ProcessLogNodeEffect (manager, ref delay);
//			
//			manager.OnUnitCombatLogEvent(_unit.UnitIndex, _logType, resDelay);
//			
//			return resDelay;
//		} 
//	}
}