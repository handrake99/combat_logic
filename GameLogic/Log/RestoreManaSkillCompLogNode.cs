using System.Collections.Generic;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using UnityEngine;

namespace IdleCs.GameLog
{
    public class RestoreManaSkillCompLogNode : ActiveSkillCompLogNode
    {
	    public uint RestoreCount;
	    public int Result;

        public RestoreManaSkillCompLogNode()
        {
        }

        public RestoreManaSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
            : base(caster, target, skillComp)
        {
            RestoreCount = (uint)skillComp.BaseAmount;
            Result = 0;
        }
        
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.RestoreMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(RestoreCount);
	        writer.Write(Result);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out RestoreCount);
	        reader.Read(out Result);
	        
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

            combatLogData.Message = string.Format("{0} RestoreMana({1}/{2}) to {3}", casterName, Result, RestoreCount, targetName);
            index++;
			
			logDataList.Add(combatLogData);
			
	    }
    }

    public class ConsumeManaSkillCompLogNode : ActiveSkillCompLogNode
    {
	    public uint ConsumeCount;
	    public int Result;
		
        public ConsumeManaSkillCompLogNode()
        {}
        public ConsumeManaSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
            : base(caster, target, skillComp)
        {
            ConsumeCount = (uint)skillComp.BaseAmount;
            Result = 0;
        }
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.ConsumeMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(ConsumeCount);
	        writer.Write(Result);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out ConsumeCount);
	        reader.Read(out Result);
	        
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

            combatLogData.Message = string.Format("{0} ConsumeMana({1}/{2}) to {3}", casterName, Result, ConsumeCount, targetName);
            index++;
			
			logDataList.Add(combatLogData);
	    }
    }
    
    public class AbsorbManaSkillCompLogNode : ActiveSkillCompLogNode
    {
	    public uint AbsorbCount;
	    public int OwnerResult;
	    public int TargetResult;
		
        public AbsorbManaSkillCompLogNode()
        {}
        public AbsorbManaSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
            : base(caster, target, skillComp)
        {
            AbsorbCount = (uint)skillComp.BaseAmount;
            OwnerResult = 0;
            TargetResult = 0;
        }
        
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.AbsorbMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(AbsorbCount);
	        writer.Write(OwnerResult);
	        writer.Write(TargetResult);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out AbsorbCount);
	        reader.Read(out OwnerResult);
	        reader.Read(out TargetResult);
	        
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

            combatLogData.Message = string.Format("{0} Absorbe({1}->{2}/{3}) to {4}"
	            , casterName, TargetResult,  OwnerResult, AbsorbCount, targetName);
            index++;
			
			logDataList.Add(combatLogData);
			
	    }

    }

    public class RestoreNextManaSkillCompLogNode : ActiveSkillCompLogNode
    {
	    public uint RestoreCount;
	    public int Result;

	    public RestoreNextManaSkillCompLogNode()
	    {
	    }

	    public RestoreNextManaSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
		    : base(caster, target, skillComp)
	    {
		    RestoreCount = (uint)skillComp.BaseAmount;
		    Result = 0;
	    }
	    
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.RestoreNextMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(RestoreCount);
	        writer.Write(Result);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out RestoreCount);
	        reader.Read(out Result);
	        
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

		    combatLogData.Message = string.Format("{0} RestoreNextMana({1}/{2}) to {3}", casterName, Result, RestoreCount,
			    targetName);
		    index++;

		    logDataList.Add(combatLogData);

	    }
    }

    public class DecreaseManaCostSkillCompLogNode : ActiveSkillCompLogNode
    {
	    public uint DecreaseCount;
	    public int Result;

	    public DecreaseManaCostSkillCompLogNode()
	    {
	    }

	    public DecreaseManaCostSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
		    : base(caster, target, skillComp)
	    {
		    DecreaseCount = (uint)skillComp.BaseAmount;
		    Result = 0;
	    }

	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.RestoreNextMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
	    {
		    if (base.Serialize(writer) == false)
		    {
			    return false;
		    }

		    writer.Write(DecreaseCount);
		    writer.Write(Result);

		    return true;
	    }

	    public override ICorgiSerializable DeSerialize(IPacketReader reader)
	    {
		    if (base.DeSerialize(reader) == null)
		    {
			    return null;
		    }

		    reader.Read(out DecreaseCount);
		    reader.Read(out Result);

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

		    combatLogData.Message = string.Format("{0} DecreaseManaCost({1}/{2}) to {3}", casterName, Result,
			    DecreaseCount,
			    targetName);
		    index++;

		    logDataList.Add(combatLogData);

	    }
    }
}