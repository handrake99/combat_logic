using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedRelicInfo : CorgiSharedObject
    {
        public string characterId;
        public ulong baseUid;
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(characterId);
            writer.Write(baseUid);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out characterId);
            reader.Read(out baseUid);

            return this;
        }
    }
}