using System;
using System.Collections.Generic;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using UnityEngine;



namespace IdleCs.GameLog
{
	[System.Serializable]
	public class TickLogNode : CombatLogNode
	{
		public ulong DeltaTime;
		public CorgiStringList UnitIdList;
	    
		public TickLogNode()
		{}
		public TickLogNode(ulong deltaTime)
		{
			DeltaTime = deltaTime;
			UnitIdList = new CorgiStringList();
		}

		public void AddUnit(Unit unit)
		{
			UnitIdList.Add(unit.ObjectId);
		}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Tick;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(DeltaTime);
            UnitIdList.Serialize(writer);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out DeltaTime);
            
            UnitIdList = new CorgiStringList();
            UnitIdList.DeSerialize(reader);

            return this;
		}

	    public override void LogDebug(IGameDataBridge bridge)
		{
			base.LogDebug(bridge);
		}
	}
}
