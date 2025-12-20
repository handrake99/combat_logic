using System.IO;
using System.Text;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using UnityEngine.Serialization;

namespace IdleCs.GameLog
{
    [System.Serializable]
    public class PartyMemberDungeonEnterLogNode : CombatLogNode
    {
        public string Nickname;

        public PartyMemberDungeonEnterLogNode(){}
        
        public PartyMemberDungeonEnterLogNode(string nickname) : base()
        {
            Nickname = nickname;
        }
        public override int GetClassType()
        {
            return (int)CombatLogNodeType.PartyMemberDungeonEnter;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }

            var convertedName = Encoding.UTF8.GetBytes(Nickname);
            writer.Write(convertedName);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out byte[] convertedName);
            Nickname = Encoding.UTF8.GetString(convertedName);

            return this;
        }
    }
}