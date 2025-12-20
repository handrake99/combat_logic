using System.Data;
using Corgi.GameData;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using SteamB23.KoreanUtility.Grammar;

namespace IdleCs.GameLogic
{
    public class PartyLogBattle : PartyLogMessage
    {
        public uint BattleCount;
        public ulong StageUid;

        public PartyLogBattle()
        {
        }


        public PartyLogBattle(PartyLogType type, string userName, ulong stageUid)
            : base(type, userName)
        {
            BattleCount = 1;
            StageUid = stageUid;
        }

        public override bool Aggregate(PartyLogMessage logMessage)
        {
            var thisLog = logMessage as PartyLogBattle;
            if (thisLog == null)
            {
                return false;
            }

            Timestamp = thisLog.Timestamp;
            BattleCount += 1;
            StageUid = thisLog.StageUid;
            return true;
        }
	    public override int GetClassType()
        {
            return (int)PartyLogCategory.Battle;
        }
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            writer.Write(BattleCount);
            writer.Write(StageUid);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out BattleCount);
            reader.Read(out StageUid);
            return this;
        }

        public override string GetLogStr(IGameDataBridge bridge)
        {
            string formatStr = null;
            string resultStr = null;
            string advName = null;
            string instName = null;
            
            var userName = $"{UserName}";
            var battleCount = $"<b><color=#ffffffff>{BattleCount}</color></b>";
            
            switch (LogType)
            {
                case PartyLogType.BattleAutoHunting:
                    formatStr = bridge.GetString("lang.party.log.battle.auto_hunting");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    var battleCount2 = $"<b><color=#ffffffff>{BattleCount*2}</color></b>";
                    resultStr = CorgiString.Format(formatStr, userName, battleCount, battleCount2);
                    break;
                case PartyLogType.BattleChallenge:
                    formatStr = bridge.GetString("lang.party.log.battle.challenge");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    var advSpec = bridge.GetSpec<AdventureStageInfoSpec>(StageUid);
                    if (advSpec == null)
                    {
                        return null;
                    }
                    
                    advName = $"{advSpec.Name}";
                    resultStr = CorgiString.Format(formatStr, userName, battleCount, advName);
                    break;
                case PartyLogType.BattleInstance:
                    formatStr = bridge.GetString("lang.party.log.battle.instance");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    var instSpec = bridge.GetSpec<InstanceChapterInfoSpec>(StageUid);
                    if (instSpec == null)
                    {
                        return null;
                    }

                    instName = $"{instSpec.Name}";
                    resultStr = CorgiString.Format(formatStr, userName, battleCount);
                    break;
                case PartyLogType.BattleStageLose:
                    formatStr = bridge.GetString("lang.party.log.battle.stage_drop");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    formatStr = formatStr.Replace(" [을를]", "[을를]");

                    resultStr = CorgiString.Format(formatStr, battleCount);
                    break;
            }

            if (string.IsNullOrEmpty(resultStr))
            {
                return null;
            }

            resultStr = 조사.문자처리(resultStr);

            if (!string.IsNullOrEmpty(userName))
            {
                resultStr = resultStr.Replace(userName, $"<b><color=#ffffffff>{userName}</color></b>");
            }

            if (!string.IsNullOrEmpty(advName))
            {
                resultStr = resultStr.Replace(advName, $"<b><color=#ffffffff>{advName}</color></b>");
            }
            
            if (!string.IsNullOrEmpty(instName))
            {
                resultStr = resultStr.Replace(instName, $"<b><color=#ffffffff>{instName}</color></b>");
            }
            return resultStr;
        }
    }
}