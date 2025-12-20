using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;


namespace IdleCs.GameLog
{
    [Serializable]
	public abstract class ActiveSkillCompLogNode : SkillCompLogNode
	{
		/// <summary>
		/// results
		/// </summary>
		public bool IsFeatured;

		public bool IsMiss;
		public bool IsCritical;

		public bool ForceCritical;
		public bool ForceHit;

		public bool IgnoreDefence;
		public bool IgnoreCritical;
		public bool IgnoreEvent;
		public bool IgnoreBarrier;
		public bool IgnoreImmune;
		public bool IgnoreEnhance;

		public float IgnoreDefenceFactor;

		
		public ActiveSkillCompLogNode()
			: base()
		{
			IsMiss = false;
			IsCritical = false;
			ForceCritical = false;

			IgnoreDefenceFactor = 1f;
		}

		public ActiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}

		public virtual void AmplifyOutput(float mod)
		{
		}
		
		public virtual int DrainOutput(float factor, int min, int max)
		{
			return 0;
		}
		
		public virtual void ApplyEnhance(float absoluteValue, float percentPlusValue, float percentMinusValue)
		{
		}

		public virtual float TransferOutput(long transferValue, float transferRate, float reduceRate)
		{
			return 0f;
		}
		
        public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(IsFeatured);
	        writer.Write(IsMiss);
	        writer.Write(IsCritical);
	        writer.Write(ForceCritical);
	        writer.Write(ForceHit);
	        
	        writer.Write(IgnoreDefence);
	        writer.Write(IgnoreCritical);
	        writer.Write(IgnoreEvent);
	        writer.Write(IgnoreBarrier);
	        writer.Write(IgnoreImmune);
	        writer.Write(IgnoreEnhance);
	        
	        writer.Write(IgnoreDefenceFactor);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out IsFeatured);
	        reader.Read(out IsMiss);
	        reader.Read(out IsCritical);
	        reader.Read(out ForceCritical);
	        reader.Read(out ForceHit);
	        
	        reader.Read(out IgnoreDefence);
	        reader.Read(out IgnoreCritical);
	        reader.Read(out IgnoreEvent);
	        reader.Read(out IgnoreBarrier);
	        reader.Read(out IgnoreImmune);
	        reader.Read(out IgnoreEnhance);
	        
	        reader.Read(out IgnoreDefenceFactor);
	        
	        return this;
        }
		
		
	    public override void LogDebug(IGameDataBridge bridge)
	    {
		    var spec = bridge.GetSpec<SkillActiveInfoSpec>(SkillCompUid);
		    CorgiCombatLog.Log(CombatLogCategory.Skill, "Use SkillComp({0})", spec?.Name);
		    
		    base.LogDebug(bridge);
	    }

	}


//
//    [Serializable]
//	public class DrainHPSkillCompLogNode : ActiveSkillCompLogNode
//	{
//		//DamageSkillComp _skillComp;
//		int _drainAmount;
//
//		public DrainHPSkillCompLogNode(DungeonLogType logType, SkillOutputActive output, int drainAmount)
//			: base(logType, output)
//		{
//			_drainAmount = drainAmount;
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
//			string tempStr = CorgiColor.BBC_COLOR_GREEN + (int)_drainAmount+ CorgiColor.BBC_COLOR_END;
//			return CorgiString.Format(baseDesc, _target.NameForLog, SkillComp.ParentActor.Name, tempStr);
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//
//		protected override float OnAttackedEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisLogNode = nodeDelay;
//
//			if (_targetIndex != -1)
//			{
//				string healStr = "+" + _drainAmount.ToString();
//				//string healStr = CorgiColor.BBC_COLOR_GREEN + _drainAmount.ToString() + CorgiColor.BBC_COLOR_END;
//
//				TextUIMessage msg = new TextUIMessage(TextUIType.Heal, healStr);
//				base.ProcessLogNodeHPUpdate(manager, _drainAmount, _logType, thisLogNode);
//				base.ProcessLogNodeText(manager, msg, _logType, ref thisLogNode);
//
//				thisLogNode += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return thisLogNode;
//		}
//	}
//
//
//    [Serializable]
//	public class RestorePowerSkillCompLogNode : ActiveSkillCompLogNode
//	{
//		//RestorePowerSkillComp _skillComp;
//		//int _restoreAmount;
//
//		public RestorePowerSkillCompLogNode(DungeonLogType logType, SkillOutputActive output)
//			: base(logType, output)
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
//			string tempStr = CorgiColor.BBC_COLOR_BLUE + _output.AmountFinal+ CorgiColor.BBC_COLOR_END;
//			return CorgiString.Format(baseDesc, _target.NameForLog, _output.SkillComp.Caster.NameForLog, _output.SkillComp.ParentActor.Name, tempStr);
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//
//		protected override float OnAttackedEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float thisLogNode = nodeDelay;
//
//			if (_targetIndex != -1)
//			{
//				string restoreStr = "";
//				
//				if(_output.AmountFinal > 0)
//					restoreStr = "+" + _output.AmountFinal.ToString();
//				else
//					restoreStr =  _output.AmountFinal.ToString();
//
//				TextUIMessage msg = new TextUIMessage(TextUIType.Power, restoreStr);
//                base.ProcessLogNodeText(manager, msg, _logType, ref thisLogNode, _output.AmountFinal);
//
//				thisLogNode += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return thisLogNode;
//		}
//	}
//
//    [Serializable]
//	public class ContinuousSkillCompLogNode : SkillCompLogNode
//	{
//		ContinuousSkillComp _skillComp;
//
//		public ContinuousSkillCompLogNode(DungeonLogType logType, ContinuousSkillComp skillComp, IUnit target)
//			: base(logType, skillComp, target)
//		{
//			_skillComp = skillComp;
//		}
//
//		public override string GetDesc()
//		{
//			if (_skillComp.Visible == false)
//			{
//				return null;
//			}
//
//			string baseDesc = base.GetDesc();
//
//			if (baseDesc == null)
//			{
//				return null;
//			}
//			return CorgiString.Format(baseDesc, _target.NameForLog, _skillComp.Name);
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//            float retDelay = base.ProcessLogNodeEffect(manager, ref nodeDelay);
//			
//			/*
//			if (_targetIndex != -1)
//			{
//				//sound
//				if(_skillComp.EffectList != null && _skillComp.EffectList.Count >0)
//				{
//					if(_skillComp.BenefitType == SkillContinuousType.Buff)
//					{
//						manager.PlaySoundOneShot("sound_fx_buff_001");
//					}
//					else
//					{
//						manager.PlaySoundOneShot("sound_fx_debuff_001");
//					}
//				}
//			}					
//			 */
//			
//			return retDelay;
//		}
//	}
//
//    [Serializable]
//	public class ReviveSkillCompLogNode : SkillCompLogNode
//	{
//		ReviveSkillComp _skillComp;
//
//		public ReviveSkillCompLogNode(DungeonLogType logType, IUnit target, ReviveSkillComp skillComp)
//			: base(logType, skillComp, target)
//		{
//			_skillComp = skillComp;
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
//			return CorgiString.Format(baseDesc, _target.NameForLog);
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//	}
//	
//	[Serializable]
//	public class SummonSkillCompLogNode : SkillCompLogNode
//	{
//		int _unitIndex;
//		
//		public SummonSkillCompLogNode(DungeonLogType logType, IUnit target, ISkillComp skillComp, int unitIndex)
//			: base(logType, skillComp, target)
//		{
//			_unitIndex = unitIndex;
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
//			return CorgiString.Format(baseDesc, _target.NameForLog);
//		}
//
//		public override float ProcessEffect (ICombatUIInterface manager, ref float nodeDelay, int depthCount)
//		{
//			float resNodeDelay = base.ProcessEffect (manager, ref nodeDelay, depthCount);
//			
//			manager.OnUnitCombatLogEvent(_unitIndex, DungeonLogType.Summon, resNodeDelay);
//			return resNodeDelay;
//		}
//
//	}
//
//    [Serializable]
//	public class RemoveContinuousLogNode : SkillCompLogNode
//	{
//		SkillInst _skillInst;
//
//		public RemoveContinuousLogNode(DungeonLogType logType, SkillInst skillInst)
//			: base(logType, skillInst.SkillComp, skillInst.Target)
//		{
//			_skillInst = skillInst;
//		}
//
//		public override string GetDesc()
//		{
//			if (_skillInst.Visible == false)
//			{
//				return null;
//			}
//
//			string baseDesc = base.GetDesc();
//
//			if(baseDesc == null)
//			{
//				return null;
//			}
//			if(_skillInst.Target.IsAlive() == false)
//			{
//				return null;
//			}
//			return CorgiString.Format(baseDesc, _skillInst.Target.NameForLog, _skillInst.Name);
//		}
//
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//		{
//			List<SpecSkillEffectObjectInfo> effectList = _skillInst.SkillComp.EffectList;
//			
//			//manager.HideSkillEffect(effectList, _casterUnit, _targetUnit);
//
//			return nodeDelay;
//		}
//
//		protected override void SerializeNode(JSONObject thisObject)
//		{
//		}
//
//	}
//
//    [Serializable]
//	public class DispelSkillCompLogNode : ActiveSkillCompLogNode
//	{
//		SkillInst _skillInst;
//
//		public DispelSkillCompLogNode(DungeonLogType logType, SkillOutputActive output, SkillInst skillInst = null)
//			: base(logType, output)
//		{
//			_skillInst = skillInst;
//		}
//
//		public override string GetDesc()
//		{
//            if(_skillInst == null)
//            {
//                return null;
//            }
//
//			string baseDesc = base.GetDesc();
//
//			if (baseDesc == null)
//			{
//				return null;
//			}
//			return CorgiString.Format(baseDesc, _target.NameForLog, _skillInst.Name);
//		}
//		public override float ProcessLogNodeEffect(ICombatUIInterface manager, ref float nodeDelay)
//        {
//            if(_skillInst == null)
//            {
//                return base.ProcessLogNodeEffect(manager, ref nodeDelay);
//
//            }else
//            {
//                return 0;
//            }
//        }
//	}
//
//    [Serializable]
//	public class InterruptSkillCompLogNode : ActiveSkillCompLogNode
//	{
//        IAction _interruptedAction = null;
//
//		public InterruptSkillCompLogNode(DungeonLogType logType, SkillOutputActive output, IAction action)
//			: base(logType, output)
//		{
//            _interruptedAction = action;
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
//			return CorgiString.Format(baseDesc, _target.NameForLog, _interruptedAction.Name);
//		}
//		
//		public override float ProcessLogNodeEffect (ICombatUIInterface manager, ref float nodeDelay)
//		{
//			float thisNodeDelay = nodeDelay;
//			float retDelay = base.ProcessLogNodeEffect(manager, ref nodeDelay);
//
//			if (_targetIndex != -1)
//			{
//				string ret = CorgiStaticStringData.Instance.GetString(LangStringType.SCT, "Interrupt");
//				if(string.IsNullOrEmpty(ret))
//				{
//					ret = "Interrput";
//				}
//				
//				TextUIMessage msg = new TextUIMessage(TextUIType.Info, ret, null,_interruptedAction.Name);
//				base.ProcessLogNodeText(manager, msg, _logType, ref thisNodeDelay);
//
//				retDelay += CorgiConst.SCT_SHOW_INTERVAL;
//			}
//
//			return retDelay;
//		}
//	}
}