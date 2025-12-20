using System;
using System.Collections.Generic;
using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IdleCs.GameLog
{
	[Serializable]
    public abstract class LogNode : CorgiSerializable
    {
	    public ulong CurTick;

	    public LogNode()
	    {
		    CurTick = CorgiTime.UtcNowULong;
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
	        
	        writer.Write(CurTick);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out CurTick);
	        
	        return this;
        }

//        public static LogNode DeSerialize(IPacketReader reader)
//        {
//	        int size;
//	        string jsonStr;
//
//	        try
//	        {
//				reader.Read(out size);
//				reader.Read(out jsonStr);
//	        
//				var setting = new JsonSerializerSettings();
//				setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
//				setting.NullValueHandling = NullValueHandling.Ignore;
//				setting.TypeNameHandling = TypeNameHandling.Auto;
//				 
//				var retObject = CorgiJson.DeserializeObject<LogNode>(jsonStr, setting);
//				
//				retObject.OnDeSerialize(reader);
//				
//				return retObject;
//
//	        }
//	        catch (Exception e)
//	        {
//		        //CorgiLog.LogError(e);
//		        //CorgiLog.LogError(jsonStr);
//		        return null;
//	        }
//        }
//        public virtual void OnDeSerialize(IPacketReader reader)
//        {
//        }
        
        // debugging 용 console output
	    public virtual void LogDebug(IGameDataBridge bridge)
	    {
	    }

    }
}