using IdleCs.Network.NetLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedRiftCharacter : CorgiSharedObject
    {
        public string characterId;
        public string nickname;
        
        public ulong damage;
        public uint tryCount;

        public bool isChallenging;
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            writer.Write(characterId);
            writer.Write(nickname);
            writer.Write(damage);
            writer.Write(tryCount);
            writer.Write(isChallenging);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out characterId);
            reader.Read(out nickname);
            reader.Read(out damage);
            reader.Read(out tryCount);
            reader.Read(out isChallenging);

            return this;
        }

        public SharedRiftCharacter Clone()
        {
            var shared = new SharedRiftCharacter();
            
            shared.characterId = characterId;
            shared.nickname = nickname;
            shared.damage = damage;
            shared.tryCount = tryCount;
            shared.isChallenging = isChallenging;

            return shared;
        }
        
        public JObject ToJson()
        {
            var ret = new JObject();
            
            ret.Add("characterId", characterId);
            ret.Add("nickname", nickname);
            ret.Add("damage", damage);
            ret.Add("tryCount", tryCount);
            ret.Add("isChallenging", isChallenging);
            
            return ret;
        }
    }
}