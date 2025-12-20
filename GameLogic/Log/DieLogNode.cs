using System;
using System.Collections.Generic;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;


namespace IdleCs.GameLog
{
	public class DieLogNode : CombatLogNode
	{
		//Unit _unit;
		public string DeadUnitId;

		public DieLogNode()
			: base()
		{
		}

		public DieLogNode(Unit unit)
			: base()
		{
			DeadUnitId = unit.ObjectId;
		}

		public override int GetClassType()
		{
			return (int)CombatLogNodeType.Die;
		}

		public override bool Serialize(IPacketWriter writer)
		{
			if (base.Serialize(writer) == false)
			{
				return false;
			}
			
			writer.Write(DeadUnitId);

			return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}

			reader.Read(out DeadUnitId);

			return this;
		}
	}
}