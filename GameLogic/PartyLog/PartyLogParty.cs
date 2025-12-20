using System.Data;
using Corgi.GameData;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using SteamB23.KoreanUtility.Grammar;

namespace IdleCs.GameLogic
{
    public class PartyLogParty : PartyLogMessage
    {
        public ulong StageUid;
        public int Count;

        public PartyLogParty()
        {
        }


        public PartyLogParty(PartyLogType type, ulong stageUid)
            : base(type, string.Empty)
        {
            StageUid = stageUid;
        }

	    public override int GetClassType()
        {
            return (int)PartyLogCategory.Party;
        }
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            writer.Write(StageUid);
            writer.Write(Count);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out StageUid);
            reader.Read(out Count);
            return this;
        }
        
        public override string GetLogStr(IGameDataBridge bridge)
        {
            string formatStr = null;
            string resultStr = null;
            string riseChapterName = null;
            string dropChapterName = null;
            
            switch (LogType)
            {
                case PartyLogType.PartyChapterRise:
                {
                    formatStr = bridge.GetString("lang.party.log.party.chapter_rise");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    formatStr = formatStr.Replace(" [을를]", "[을를]");
                    
                    var riseStageSpec = bridge.GetSpec<AdventureStageInfoSpec>(StageUid);
                    if (riseStageSpec == null)
                    {
                        return null;
                    }

                    var chapterUidStr = CorgiString.Format($"chapter.{riseStageSpec.ChapterIndex}");
                    var chapterSpec = bridge.GetSpec<ChapterInfoSpec>(chapterUidStr);
                    if (chapterSpec == null)
                    {
                        return null;
                    }

                    riseChapterName = $"{chapterSpec.Name}";
                    resultStr = string.Format(formatStr, riseChapterName);
                    break;
                }
                case PartyLogType.PartyChapterDrop:
                {
                    formatStr = bridge.GetString("lang.party.log.party.chapter_drop");
                    if (string.IsNullOrEmpty(formatStr))
                    {
                        return null;
                    }
                    formatStr = formatStr.Replace(" [을를]", "[을를]");

                    var dropStageSpec = bridge.GetSpec<AdventureStageInfoSpec>(StageUid);
                    if (dropStageSpec == null)
                    {
                        return null;
                    }

                    var chapterUidStr = CorgiString.Format($"chapter.{dropStageSpec.ChapterIndex}");
                    var chapterSpec = bridge.GetSpec<ChapterInfoSpec>(chapterUidStr);
                    if (chapterSpec == null)
                    {
                        return null;
                    }

                    dropChapterName = $"{chapterSpec.Name}";
                    resultStr = string.Format(formatStr, dropChapterName);
                    break;
                }
                
            }

            if (string.IsNullOrEmpty(resultStr))
            {
                return null;
            }

            resultStr = 조사.문자처리(resultStr);
            if (!string.IsNullOrEmpty(riseChapterName))
            {
                resultStr = resultStr.Replace(riseChapterName, $"<b><color=#ffffffff>{riseChapterName}</color></b>");
            }
            
            if (!string.IsNullOrEmpty(dropChapterName))
            {
                resultStr = resultStr.Replace(dropChapterName, $"<b><color=#ffffffff>{dropChapterName}</color></b>");
            }

            return resultStr;
        }
    }
}
