using System;
using Google.Protobuf;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;
using UnityEngine.PlayerLoop;

namespace IdleCs.GameLogic.SharedInstance
{
    public abstract class CorgiSharedObject : CorgiSerializable
    {
        public string dbId;
        public ulong uid ;
        public string objectId;

        public CorgiSharedObject()
        {
            objectId = string.Empty;
            dbId = string.Empty;
        }

        // for combat server
        public virtual void Init(CorgiObject original)
        {
            if (original.ObjectId != null)
            {
                objectId = original.ObjectId;
            }
            if (original.DBId != null)
            {
                dbId = original.DBId;
            }
            uid = original.Uid;
        }

        public override int GetClassType()
        {
            throw new NotImplementedException();
        }

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            writer.Write(objectId);
            writer.Write(dbId);
            writer.Write(uid);
            
            return true;
        }
        
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out objectId);
            reader.Read(out dbId);
            reader.Read(out uid);
            
            return this;
        }
    }
}