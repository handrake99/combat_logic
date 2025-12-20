using IdleCs.GameLogic;
using IdleCs.Network.NetLib;



namespace IdleCs.GameLog
{
	[System.Serializable]
	public class PauseLogNode : CombatLogNode
	{
		public ulong NextGuideUid;
	    
		public PauseLogNode()
		{}

	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Pause;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(NextGuideUid);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out NextGuideUid);

            return this;
		}

	}
}
