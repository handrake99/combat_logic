using IdleCs.Network.NetLib;
using System.Text;
using Corgi.GameData;
using SteamB23.KoreanUtility.Grammar;

namespace IdleCs.GameLogic
{
    public class PartyLogRift : PartyLogMessage
    {
        private int _count;
        private ulong _stageUid;
        private uint _grade;

        public PartyLogRift(){}
        public PartyLogRift(PartyLogType type, string userName, ulong stageUid, uint grade)
            : base(type, userName)
        {
            _count = 1;
            _stageUid = stageUid;
            _grade = grade;
        }
        
        public override int GetClassType()
        {
            return (int)PartyLogCategory.Rift;
        }
        
        public override bool Aggregate(PartyLogMessage logMessage)
        {
            if (!(logMessage is PartyLogRift logRift))
            {
                return false;
            }
            
            if (LogType != PartyLogType.RiftJoin || logRift.LogType != PartyLogType.RiftJoin)
            {
                return false;
            }
            
            if (UserName != logRift.UserName)
            {
                return false;
            }

            if (_stageUid != logRift._stageUid)
            {
                return false;
            }

            if (_grade != logRift._grade)
            {
                return false;
            }
            
            Timestamp = logRift.Timestamp;
            _count += logRift._count;
            
            return true;
        }

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(_count);
            writer.Write(_stageUid);
            writer.Write(_grade);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out _count);
            reader.Read(out _stageUid);
            reader.Read(out _grade);
            
            return this;
        }
        public override string GetLogStr(IGameDataBridge bridge)
        {
            string formatStr;
            string userName;
            
            var bossUids = bridge.GetSpec<InstanceRiftInfoSpec>(_stageUid)?.BossUids;

            if (bossUids == null || bossUids.Count == 0)
            {
                return null;
            }
            
            var monsterName = bridge.GetSpec<MonsterInfoSpec>(bossUids[0])?.Name;

            if (string.IsNullOrEmpty(monsterName))
            {
                return null;
            }

            string resultStr;

            switch (LogType)
            {
                case PartyLogType.RiftOpen:
                    formatStr = bridge.GetString("lang.party.log.rift.open");
                    
                    formatStr = formatStr.Replace("을(를)", "[을를]");
                    formatStr = formatStr.Replace(" [을를]", "[을를]");

                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }

                    userName = UserName;

                    var grade = _grade switch
                    {
                        1 => "lang.equip.grade.uncommon",
                        2 => "lang.equip.grade.rare",
                        3 => "lang.equip.grade.epic",
                        4 => "lang.equip.grade.legendary",
                        5 => "lang.equip.grade.transcendence",
                        _ => null
                    };

                    if (string.IsNullOrEmpty(grade))
                    {
                        return null;
                    }

                    grade = bridge.GetString(grade);
                    
                    resultStr = string.Format(formatStr, userName, grade + " " + monsterName);
                    resultStr = 조사.문자처리(resultStr);

                    if (!string.IsNullOrEmpty(userName))
                    {
                        resultStr = resultStr.Replace(userName, $"<b><color=#ffffffff>{userName}</color></b>");
                    }

                    if (!string.IsNullOrEmpty(grade))
                    {
                        resultStr = resultStr.Replace(grade, $"<b><color=#ffffffff>{grade}</color></b>");
                    }

                    if (!string.IsNullOrEmpty(monsterName))
                    {
                        resultStr = resultStr.Replace(monsterName, $"<b><color=#ffffffff>{monsterName}</color></b>");
                    }

                    return resultStr;
                    
                case PartyLogType.RiftJoin:
                    formatStr = bridge.GetString("lang.party.log.rift.join");
                    
                    formatStr = formatStr.Replace("을(를)", "[을를]");
                    formatStr = formatStr.Replace(" [을를]", "[을를]");

                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    
                    userName = $"<b><color=#ffffffff>{UserName}</color></b>";
                    
                    var count = $"<b><color=#ffffffff>{_count}</color></b>";

                    resultStr = string.Format(formatStr, userName, monsterName, count);
                    resultStr = 조사.문자처리(resultStr);
                    resultStr = resultStr.Replace(monsterName, $"<b><color=#ffffffff>{monsterName}</color></b>");
                    return resultStr;
                
                default:
                    return null;
            }
        }
    }
}