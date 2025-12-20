using System.Runtime.Serialization;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic
{
    public interface ICorgiSerializable
    {
        int GetClassType();
        bool Serialize(IPacketWriter writer);
        void OnSerialize(IPacketWriter writer);
        
        ICorgiSerializable DeSerialize(IPacketReader reader);
    }

    public abstract class CorgiSerializable : ICorgiSerializable
    {
        public abstract int GetClassType();

        public virtual bool Serialize(IPacketWriter writer)
        {
            OnSerialize(writer);
            
            writer.Write(1);

            return true;
        }

        public virtual void OnSerialize(IPacketWriter writer)
        {
            
        }

        public virtual ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            int value;
            reader.Read(out value);

            if (value == 0)
            {
                return null;
            }

            return this;
        }
    }
}