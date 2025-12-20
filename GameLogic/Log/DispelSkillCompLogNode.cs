using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLogic;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{

	[Serializable]
	public class DispelSkillCompLogNode : ActiveSkillCompLogNode
	{
		public CorgiStringList DispelList;
		public CorgiUlongList DispelUidList;
		public uint DispelCount;
		public uint Result;
		
		public DispelSkillCompLogNode()
		{}
		public DispelSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
			: base(caster, target, skillComp)
		{
			DispelList = new CorgiStringList();
			DispelUidList = new CorgiUlongList();
			DispelCount = (uint)skillComp.BaseAmount;
			Result = 0;
		}

		public uint DoDispel(SkillEffectInst inst)
		{
			if (inst.IsDispel == false)
			{
				return 0;
			}
			if (DispelCount <= 0 || inst.StackCount <= 0)
			{
				return 0;
			}

			DispelList.Add(inst.ObjectId);
			DispelUidList.Add(inst.Uid);
			
            var result = inst.DoOver(DispelCount);

			if (DispelCount < result)
			{
				var ret = DispelCount;
				
				Result += DispelCount;
				DispelCount = 0;
				
				return ret;
			}

			DispelCount -= result;
			Result += result;
			
            AddDetailLog($"Dispel {inst.Name}, stackCount {result}");
			
			return result;
			
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Dispel;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
			if (base.Serialize(writer) == false)
			{
				return false;
			}
			
			DispelList.Serialize(writer);
			DispelUidList.Serialize(writer);
			
			writer.Write(DispelCount);
			writer.Write(Result);

			return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}

			DispelList = new CorgiStringList();
			DispelList.DeSerialize(reader);
			DispelUidList = new CorgiUlongList();
			DispelUidList.DeSerialize(reader);

			reader.Read(out DispelCount);
			reader.Read(out Result);

			return this;
		}
		
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid dispel skillcomp log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);
            
            var combatLogData = new CombatLogData(CombatLogType.ActiveSkillComp, index, this);
            index++;

            
			combatLogData.Message = string.Format("{0} Dispel Count({1}) to {2}", casterName, Result, targetName);
			
			logDataList.Add(combatLogData);
			
	    }
	}
}
