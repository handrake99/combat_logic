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
	public class InterruptSkillCompLogNode : ActiveSkillCompLogNode
	{

		/// <summary>
		/// Result
		/// </summary>
		public bool IsSuccess;

		public override SctMessageInfo SctMessageInfo
		{
			get
			{
//			    if (IsMiss)
//				    return new SctMessageInfo(SctMessageType.Miss);

				return new SctMessageInfo(SctMessageType.Interrupt);		
			}
		}
	    
	    public InterruptSkillCompLogNode()
			: base()
	    {
	    }

	    public InterruptSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
	    
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Interrupt;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(IsSuccess);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out IsSuccess);
	        
	        return this;
        }
	    
//	    protected override SctLog GetSctLog()
//	    {
//		    if (FinalDamage <= 0) { return SctLog.Null; }
//		    
//		    SctMessageType messageType = (IsCritical) ? SctMessageType.DamageCritical : SctMessageType.Damage;
//		    return new SctLog(TargetId, new SctMessageInfo(messageType, FinalDamage), StatusEffectNumber);
//	    }
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid cancel casting log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);
            
            var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
            index++;
            
		    combatLogData.Message = string.Format("{0} Casting Canceled {1} "
			    , casterName, targetName);
            logDataList.Add(combatLogData);
            
	    }
	}
}
