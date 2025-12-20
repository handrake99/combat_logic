using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedEquipInfo : CorgiSharedObject
    {
        public ulong soulUidMain;
        public ulong soulUid1;
        public ulong soulUid2;
        public ulong soulUid3;
        public ulong soulUid4;
        
        public uint level;
        public uint grade;
        
        // soulEnergyLimit
        // SoulEnergyUseCount
        // inTransaction

        public string mainStat1;
        public long mainStat1Value;
        public long mainStat1PerLevel;
        public string mainStat2;
        public long mainStat2Value;
        public long mainStat2PerLevel;
        
        public string subStat1;
        public long subStat1Value;
        public long subStat1PerLevel;
        public string subStat2;
        public long subStat2Value;
        public long subStat2PerLevel;
        public string subStat3;
        public long subStat3Value;
        public long subStat3PerLevel;
        public string subStat4;
        public long subStat4Value;
        public long subStat4PerLevel;
        
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(soulUidMain);
            writer.Write(soulUid1);
            writer.Write(soulUid2);
            writer.Write(soulUid3);
            writer.Write(soulUid4);
            
            writer.Write(level);
            writer.Write(grade);
            
            writer.Write(mainStat1);
            writer.Write(mainStat1Value);
            writer.Write(mainStat1PerLevel);
            writer.Write(mainStat2);
            writer.Write(mainStat2Value);
            writer.Write(mainStat2PerLevel);
            
            writer.Write(subStat1);
            writer.Write(subStat1Value);
            writer.Write(subStat1PerLevel);
            writer.Write(subStat2);
            writer.Write(subStat2Value);
            writer.Write(subStat2PerLevel);
            writer.Write(subStat3);
            writer.Write(subStat3Value);
            writer.Write(subStat3PerLevel);
            writer.Write(subStat4);
            writer.Write(subStat4Value);
            writer.Write(subStat4PerLevel);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out soulUidMain);
            reader.Read(out soulUid1);
            reader.Read(out soulUid2);
            reader.Read(out soulUid3);
            reader.Read(out soulUid4);
            
            reader.Read(out level);
            reader.Read(out grade);
            
            reader.Read(out mainStat1);
            reader.Read(out mainStat1Value);
            reader.Read(out mainStat1PerLevel);
            reader.Read(out mainStat2);
            reader.Read(out mainStat2Value);
            reader.Read(out mainStat2PerLevel);
            
            reader.Read(out subStat1);
            reader.Read(out subStat1Value);
            reader.Read(out subStat1PerLevel);
            reader.Read(out subStat2);
            reader.Read(out subStat2Value);
            reader.Read(out subStat2PerLevel);
            reader.Read(out subStat3);
            reader.Read(out subStat3Value);
            reader.Read(out subStat3PerLevel);
            reader.Read(out subStat4);
            reader.Read(out subStat4Value);
            reader.Read(out subStat4PerLevel);

            return this;
        }
    }
}