using System;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;


namespace IdleCs.GameLog
{
	[Serializable]
	public class StageLogNode : CombatLogNode
	{
		public ulong StageUid;

		public StageLogNode()
		{}
		
		public StageLogNode(Stage stage)
		{
			StageUid = stage.Uid;
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Stage;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(StageUid);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out StageUid);

            return this;
		}
		
		
	}
}
