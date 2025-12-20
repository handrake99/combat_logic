using System;
using System.Text;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace IdleCs.GameLogic
{
    public abstract class PartyLogMessage : CorgiSerializable
    {
	    public ulong Timestamp;
	    public PartyLogType LogType;
	    public string UserName;

        public PartyLogMessage()
        {
        }

        public PartyLogMessage(PartyLogType type, string userName)
        {
	        Timestamp = CorgiTime.UtcNowULong;
            LogType = type;
            UserName = userName;
        }

        public bool IsSame(PartyLogMessage logMessage)
        {
	        return LogType == logMessage.LogType && UserName == logMessage.UserName;
        }

        public virtual bool Aggregate(PartyLogMessage logMessage)
        {
	        return false;
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

	        writer.Write(Timestamp);
	        writer.WriteEnum(LogType);

			byte[] convertedName = Encoding.UTF8.GetBytes(UserName);
			writer.Write(convertedName);

	        return true;
        }
        
        public override void OnSerialize(IPacketWriter writer)
        {
	        writer.Write(GetClassType());
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}

			reader.Read(out Timestamp);
			reader.ReadEnum(out LogType);

			byte[] convertedName;
			reader.Read(out convertedName);
            UserName = Encoding.UTF8.GetString(convertedName);

	        return this;
        }

        public static PartyLogMessage Create(PartyLogCategory logCategory)
        {
	        PartyLogMessage instance = null;
	        switch (logCategory)
	        {
		        case PartyLogCategory.User:
				    instance = new PartyLogUser();
				    break;
			    case PartyLogCategory.Battle:
				    instance = new PartyLogBattle();
				    break;
			    case PartyLogCategory.Acquire:
					instance = new PartyLogAcquire();
					break;
			    case PartyLogCategory.Party:
				    instance = new PartyLogParty();
				    break;
			    case PartyLogCategory.Rift:
				    instance = new PartyLogRift();
				    break;
	        }

	        return instance;
        }
        

        public abstract string GetLogStr(IGameDataBridge bridge);
        
    }
}