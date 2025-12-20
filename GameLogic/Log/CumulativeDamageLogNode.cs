using IdleCs.GameLogic;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLog
{
    public class CumulativeDamageLogNode : CombatLogNode
    {
        public string DBId;
        public long Damage;

        public CumulativeDamageLogNode(){}
        
        public CumulativeDamageLogNode(string dbId, long damage)
        {
            DBId = dbId;
            Damage = damage;
        }
        
        public override int GetClassType()
        {
            return (int)CombatLogNodeType.CumulativeDamage;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(DBId);
            writer.Write(Damage);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }

            reader.Read(out DBId);
            reader.Read(out Damage);

            return this;
        }
    }
}