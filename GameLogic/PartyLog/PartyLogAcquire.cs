using System;
using Corgi.GameData;
using IdleCs.Network.NetLib;
using SteamB23.KoreanUtility.Grammar;

namespace IdleCs.GameLogic
{
    public class PartyLogAcquire : PartyLogMessage
    {
        public ulong AcquireUid;
        public int AcquireCount = 1;
        
        public PartyLogAcquire(){}
        public PartyLogAcquire(PartyLogType type, string userName, ulong uid)
            : base(type, userName)
        {
            AcquireUid = uid;
        }

	    public override int GetClassType()
        {
            return (int)PartyLogCategory.Acquire;
        }
        
        public override bool Aggregate(PartyLogMessage logMessage)
        {
            var thisLog = logMessage as PartyLogAcquire;
            if (thisLog == null)
            {
                return false;
            }

            Timestamp = thisLog.Timestamp;
            AcquireUid = thisLog.AcquireUid;
            AcquireCount += 1;
            return true;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
	        writer.Write(AcquireUid);
	        writer.Write(AcquireCount);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out AcquireUid);
            reader.Read(out AcquireCount);
            return this;
        }

        public override string GetLogStr(IGameDataBridge bridge)
        {
            string formatStr = null;
            string itemName = null;
            switch (LogType)
            {
                case PartyLogType.AcquireSkill:
                    if (AcquireCount == 1)
                    {
                        formatStr =bridge.GetString("lang.party.log.acquire.skill");
                    }
                    else
                    {
                        formatStr =bridge.GetString("lang.party.log.acquire.skill.etc");
                    }

                    formatStr = formatStr.Replace("을(를)", "[을를]");
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    
                    var skillItemSpec = bridge.GetSpec<SkillItemSpec>(AcquireUid);
                    if (skillItemSpec == null)
                    {
                        return String.Empty;
                    }

                    var skillSpec = bridge.GetSpec<SkillInfoSpec>(skillItemSpec.SkillUid);
                    if (skillSpec == null)
                    {
                        return String.Empty;
                    }

                    itemName = skillSpec.Name;
                    break;
                case PartyLogType.AcquireEquip:
                    if (AcquireCount == 1)
                    {
                        formatStr =bridge.GetString("lang.party.log.acquire.equip");
                    }
                    else
                    {
                        formatStr =bridge.GetString("lang.party.log.acquire.equip.etc");
                    }
                    
                    formatStr = formatStr.Replace("을(를)", "[을를]");
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    
                    var equipSpec = bridge.GetSpec<EquipSpec>(AcquireUid);
                    if (equipSpec == null)
                    {
                        return String.Empty;
                    }

                    itemName = equipSpec.Name;
                    break;
            }

            if (string.IsNullOrEmpty(formatStr) || string.IsNullOrEmpty(itemName))
            {
                return null;
            }

            var userName = $"{UserName}";
            var resultName = $"{itemName}";

            var text = AcquireCount == 1 ? 
                string.Format(formatStr, userName, resultName) : 
                string.Format(formatStr, userName, resultName, AcquireCount);

            text = 조사.문자처리(text);

            if (!string.IsNullOrEmpty(userName))
            {
                text = text.Replace(userName, $"<b><color=#ffffffff>{userName}</color></b>");
            }

            if (!string.IsNullOrEmpty(resultName))
            {
                text = text.Replace(resultName, $"<b><color=#ffffffff>{resultName}</color></b>");    
            }

            return text;
        }
    }
}
