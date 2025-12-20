using System;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;


namespace IdleCs.GameLog
{
	[Serializable]
	public class ExplorationLogNode : CombatLogNode
	{
		public ulong StageUid;

		public ExplorationLogNode()
		{}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Exploration;
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
}
