using System;
using System.Text;
using Corgi.GameData;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class ChattingMessage : CorgiSerializable
    {
	    public ulong TimeStamp;
	    public ChattingType ChattingType;
	    public string CharacterId;
	    public ulong Uid;
	    public string Nickname;
	    public int CharacterGrade;
	    public string Message;

	    public ChattingMessage()
	    {
			CharacterId = string.Empty;
			Nickname = string.Empty;
			Message = string.Empty;
	    }

	    public override int GetClassType()
	    {
		    throw new NotImplementedException();
	    }

	    // for loading from redis 
	    public static ChattingMessage DeserializeJson(string jsonStr)
	    {
	        try
	        {
		        var thisObject = JsonConvert.DeserializeObject<ChattingMessage>(jsonStr);
	        
		        return thisObject;
	        }
	        catch(Exception e)
	        {
                CorgiCombatLog.LogError(CombatLogCategory.Chatting,"Occur exception : {0}", e.ToString());
		        return null;
	        }
	    }
	    
	    // for save to redsi
	    public string SerializeJson()
	    {
		    var ret = JsonConvert.SerializeObject(this);
		    return ret;
	    }

	    // for protocol 
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        writer.Write(TimeStamp);
	        writer.WriteEnum(ChattingType);
	        writer.Write(CharacterId);
	        writer.Write(Uid);
	        
			byte[] convertedName = Encoding.UTF8.GetBytes(Nickname);
			writer.Write(convertedName);
			
	        writer.Write(CharacterGrade);
	        
	        
			byte[] convertedMessage = Encoding.UTF8.GetBytes(Message);
	        writer.Write(convertedMessage);
	        
			CorgiCombatLog.Log(CombatLogCategory.Chatting, "Send Data Size({0})", Message.Length);

			return true;
        }

	    // for protocol 
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out TimeStamp);
	        reader.ReadEnum(out ChattingType);
	        reader.Read(out CharacterId);
	        reader.Read(out Uid);
	        
			byte[] convertedName;
			reader.Read(out convertedName);
            Nickname = Encoding.UTF8.GetString(convertedName);
            
	        reader.Read(out CharacterGrade);
	        
			byte[] convertedMessage;
	        reader.Read(out convertedMessage);
            Message = Encoding.UTF8.GetString(convertedMessage);
	        
			CorgiCombatLog.Log(CombatLogCategory.Chatting,"Get Data Size({0})", Message.Length);

			return this;
        }

    }
}