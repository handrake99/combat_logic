
using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedCharInfo : CorgiSharedObject
    {
        public string nickname;
        public int grade;
        public string skinUid;

        public ulong exp;
        public int level;

        public int ST_MaxHP;
        public int ST_AttackSpeed;
        public int ST_AttackPower;
        public int ST_Defence;
        public int ST_Hit;
        public int ST_Evasion;
        public int ST_Crit;
        public int ST_Resilience;
        public int ST_CritDmg;

        public string defaultSettingId;
        public CorgiUlongList expertTalents;

        public string weaponId;
        public string armorId;
        public string cloakId;
        public string helmetId;
        public string gauntletId;
        public string bootsId;
        public string trinket1Id;
        public string trinket2Id;
        public string skill0Id;
        public string skill1Id;
        public string skill2Id;
        public string skill3Id;

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(nickname);
            writer.Write(grade);
            writer.Write(skinUid);
            
            writer.Write(exp);
            writer.Write(level);
            
            
            writer.Write(ST_MaxHP);
            writer.Write(ST_AttackSpeed);
            writer.Write(ST_AttackPower);
            writer.Write(ST_Defence);
            writer.Write(ST_Hit);
            writer.Write(ST_Evasion);
            writer.Write(ST_Crit);
            writer.Write(ST_Resilience);
            writer.Write(ST_CritDmg);
            
            writer.Write(defaultSettingId);
            expertTalents?.Serialize(writer);
            
            writer.Write(weaponId);
            writer.Write(armorId);
            writer.Write(cloakId);
            writer.Write(helmetId);
            writer.Write(gauntletId);
            writer.Write(bootsId);
            writer.Write(trinket1Id);
            writer.Write(trinket2Id);
            writer.Write(skill0Id);
            writer.Write(skill1Id);
            writer.Write(skill2Id);
            writer.Write(skill3Id);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out nickname);
            reader.Read(out grade);
            reader.Read(out skinUid);
            
            reader.Read(out exp);
            reader.Read(out level);
            
            
            reader.Read(out ST_MaxHP);
            reader.Read(out ST_AttackSpeed);
            reader.Read(out ST_AttackPower);
            reader.Read(out ST_Defence);
            reader.Read(out ST_Hit);
            reader.Read(out ST_Evasion);
            reader.Read(out ST_Crit);
            reader.Read(out ST_Resilience);
            reader.Read(out ST_CritDmg);
            
            reader.Read(out defaultSettingId);
            expertTalents.DeSerialize(reader);
            
            reader.Read(out weaponId);
            reader.Read(out armorId);
            reader.Read(out cloakId);
            reader.Read(out helmetId);
            reader.Read(out gauntletId);
            reader.Read(out bootsId);
            reader.Read(out trinket1Id);
            reader.Read(out trinket2Id);
            reader.Read(out skill0Id);
            reader.Read(out skill1Id);
            reader.Read(out skill2Id);
            reader.Read(out skill3Id);

            return this;
        }
    }
}