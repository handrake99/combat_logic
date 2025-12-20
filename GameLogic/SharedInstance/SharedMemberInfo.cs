
using System;
using System.Collections.Generic;
using Corgi.DBSchema;
using Google.Protobuf;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json.Converters;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedMemberInfo : CorgiSharedObject
    {
        public class UserInfo : CorgiSerializable
        {
            public string dbId;
            public string vid;
            public string nickname;
            public string region;
            public string lang;

            public override int GetClassType()
            {
                throw new System.NotImplementedException();
            }

            public override bool Serialize(IPacketWriter writer)
            {
                if (base.Serialize(writer) == false)
                {
                    return false;
                }
                writer.Write(dbId);
                writer.Write(vid);
                writer.Write(nickname);
                writer.Write(region);
                writer.Write(lang);

                return true;
            }
            
            public override ICorgiSerializable DeSerialize(IPacketReader reader)
            {
                if (base.DeSerialize(reader) == null)
                {
                    return null;
                }
                reader.Read(out dbId);
                reader.Read(out vid);
                reader.Read(out nickname);
                reader.Read(out region);
                reader.Read(out lang);

                return this;
            }
        }
        public virtual void Init(IMessage original)
        {
            var dbUser = original as DBUser;

            if (dbUser == null)
            {
                throw new CorgiException("invalid Argument for SharedUserInfo");
            }
        }

        public UserInfo user;

        public ulong joinTimestamp;
        public string leagueId;
        public int leagueSerial;

        public SharedCharInfo character;
        public SharedAlmanacStat almanacStat;
            
        public SharedPersonalSettingInfo characterSetting;
        public SharedPartySettingInfo characterSoloPartySetting;
        public SharedPartySettingInfo characterSoloOffenceSetting;
        public SharedPartySettingInfo characterSoloDefenceSetting;
        public CorgiList<SharedSkillInfo> skills;
        public CorgiList<SharedSkillSlot> skillSlots;
        public CorgiList<SharedEquipInfo> equips;
        public CorgiList<SharedRelicInfo> relics;
        public CorgiList<SharedBindingStone> bindingStones;
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            user?.Serialize(writer);
 
            writer.Write(joinTimestamp);
            
            characterSetting?.Serialize(writer);
            characterSoloPartySetting?.Serialize(writer);
            characterSoloOffenceSetting?.Serialize(writer);
            characterSoloDefenceSetting?.Serialize(writer);
            
            skills?.Serialize(writer);
            skillSlots?.Serialize(writer);
            equips?.Serialize(writer);
            relics?.Serialize(writer);
            bindingStones?.Serialize(writer);

            return true;
        }
        
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            user = new UserInfo();
            user.DeSerialize(reader);
            reader.Read(out joinTimestamp);
            
            characterSetting = new SharedPersonalSettingInfo();
            characterSetting.DeSerialize(reader);
            characterSoloPartySetting = new SharedPartySettingInfo();
            characterSoloPartySetting.DeSerialize(reader);
            characterSoloOffenceSetting = new SharedPartySettingInfo();
            characterSoloOffenceSetting.DeSerialize(reader);
            characterSoloDefenceSetting = new SharedPartySettingInfo();
            characterSoloDefenceSetting.DeSerialize(reader);

            skills = new CorgiList<SharedSkillInfo>();
            skills = skills.DeSerialize(reader);
            skillSlots = new CorgiList<SharedSkillSlot>();
            skillSlots = skillSlots.DeSerialize(reader);
            equips = new CorgiList<SharedEquipInfo>();
            equips = equips.DeSerialize(reader);
            relics = new CorgiList<SharedRelicInfo>();
            relics = relics.DeSerialize(reader);
            bindingStones = new CorgiList<SharedBindingStone>();
            bindingStones = bindingStones.DeSerialize(reader);

            return this;
        }
    }


    public class SharedPersonalSettingInfo : CorgiSharedObject
    {
        public ulong skill0Uid;
        public ulong skill1Uid;
        public ulong skill2Uid;
        public ulong skill3Uid;

        public string skill0Slot;
        public string skill1Slot;
        public string skill2Slot;
        public string skill3Slot;
        
        public ulong relic0Uid;
        public ulong relic1Uid;
        public ulong relic2Uid;
        public ulong relic3Uid;
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(skill0Uid);
            writer.Write(skill1Uid);
            writer.Write(skill2Uid);
            writer.Write(skill3Uid);

            writer.Write(skill0Slot);
            writer.Write(skill1Slot);
            writer.Write(skill2Slot);
            writer.Write(skill3Slot);
            
            writer.Write(relic0Uid);
            writer.Write(relic1Uid);
            writer.Write(relic2Uid);
            writer.Write(relic3Uid);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out skill0Uid);
            reader.Read(out skill1Uid);
            reader.Read(out skill2Uid);
            reader.Read(out skill3Uid);
            
            reader.Read(out skill0Slot);
            reader.Read(out skill1Slot);
            reader.Read(out skill2Slot);
            reader.Read(out skill3Slot);
            
            reader.Read(out relic0Uid);
            reader.Read(out relic1Uid);
            reader.Read(out relic2Uid);
            reader.Read(out relic3Uid);

            return this;
        }
    }

    public class SharedPartySettingInfo : CorgiSharedObject
    {
        public string character0Id;
        public ulong character0Skill0Uid;
        public ulong character0Skill1Uid;
        public ulong character0Skill2Uid;
        public ulong character0Skill3Uid;

        public string character0Slot0Id;
        public string character0Slot1Id;
        public string character0Slot2Id;
        public string character0Slot3Id;

        public ulong character0Relic0Uid;
        public ulong character0Relic1Uid;
        public ulong character0Relic2Uid;
        public ulong character0Relic3Uid;

        public string character1Id;
        public ulong character1Skill0Uid;
        public ulong character1Skill1Uid;
        public ulong character1Skill2Uid;
        public ulong character1Skill3Uid;

        public string character1Slot0Id;
        public string character1Slot1Id;
        public string character1Slot2Id;
        public string character1Slot3Id;
        
        public ulong character1Relic0Uid;
        public ulong character1Relic1Uid;
        public ulong character1Relic2Uid;
        public ulong character1Relic3Uid;

        public string character2Id;
        public ulong character2Skill0Uid;
        public ulong character2Skill1Uid;
        public ulong character2Skill2Uid;
        public ulong character2Skill3Uid;

        public string character2Slot0Id;
        public string character2Slot1Id;
        public string character2Slot2Id;
        public string character2Slot3Id;
        
        public ulong character2Relic0Uid;
        public ulong character2Relic1Uid;
        public ulong character2Relic2Uid;
        public ulong character2Relic3Uid;

        public string character3Id;
        public ulong character3Skill0Uid;
        public ulong character3Skill1Uid;
        public ulong character3Skill2Uid;
        public ulong character3Skill3Uid;

        public string character3Slot0Id;
        public string character3Slot1Id;
        public string character3Slot2Id;
        public string character3Slot3Id;
        
        public ulong character3Relic0Uid;
        public ulong character3Relic1Uid;
        public ulong character3Relic2Uid;
        public ulong character3Relic3Uid;
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(character0Id);
            writer.Write(character0Skill0Uid);
            writer.Write(character0Skill1Uid);
            writer.Write(character0Skill2Uid);
            writer.Write(character0Skill3Uid);
            
            writer.Write(character0Id);
            writer.Write(character0Slot0Id);
            writer.Write(character0Slot1Id);
            writer.Write(character0Slot2Id);
            writer.Write(character0Slot3Id);
            
            writer.Write(character0Id);
            writer.Write(character0Relic0Uid);
            writer.Write(character0Relic1Uid);
            writer.Write(character0Relic2Uid);
            writer.Write(character0Relic3Uid);
            
            writer.Write(character1Id);
            writer.Write(character1Skill0Uid);
            writer.Write(character1Skill1Uid);
            writer.Write(character1Skill2Uid);
            writer.Write(character1Skill3Uid);
            
            writer.Write(character1Id);
            writer.Write(character1Slot0Id);
            writer.Write(character1Slot1Id);
            writer.Write(character1Slot2Id);
            writer.Write(character1Slot3Id);
            
            writer.Write(character1Id);
            writer.Write(character1Relic0Uid);
            writer.Write(character1Relic1Uid);
            writer.Write(character1Relic2Uid);
            writer.Write(character1Relic3Uid);
            
            writer.Write(character2Id);
            writer.Write(character2Skill0Uid);
            writer.Write(character2Skill1Uid);
            writer.Write(character2Skill2Uid);
            writer.Write(character2Skill3Uid);
            
            writer.Write(character2Id);
            writer.Write(character2Slot0Id);
            writer.Write(character2Slot1Id);
            writer.Write(character2Slot2Id);
            writer.Write(character2Slot3Id);
            
            writer.Write(character2Id);
            writer.Write(character2Relic0Uid);
            writer.Write(character2Relic1Uid);
            writer.Write(character2Relic2Uid);
            writer.Write(character2Relic3Uid);
            
            writer.Write(character3Id);
            writer.Write(character3Skill0Uid);
            writer.Write(character3Skill1Uid);
            writer.Write(character3Skill2Uid);
            writer.Write(character3Skill3Uid);
            
            writer.Write(character3Id);
            writer.Write(character3Slot0Id);
            writer.Write(character3Slot1Id);
            writer.Write(character3Slot2Id);
            writer.Write(character3Slot3Id);
            
            writer.Write(character3Id);
            writer.Write(character3Relic0Uid);
            writer.Write(character3Relic1Uid);
            writer.Write(character3Relic2Uid);
            writer.Write(character3Relic3Uid);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out character0Id);
            reader.Read(out character0Skill0Uid);
            reader.Read(out character0Skill1Uid);
            reader.Read(out character0Skill2Uid);
            reader.Read(out character0Skill3Uid);

            reader.Read(out character0Id);
            reader.Read(out character0Slot0Id);
            reader.Read(out character0Slot1Id);
            reader.Read(out character0Slot2Id);
            reader.Read(out character0Slot3Id);
            
            reader.Read(out character0Id);
            reader.Read(out character0Relic0Uid);
            reader.Read(out character0Relic1Uid);
            reader.Read(out character0Relic2Uid);
            reader.Read(out character0Relic3Uid);
            
            reader.Read(out character1Id);
            reader.Read(out character1Skill0Uid);
            reader.Read(out character1Skill1Uid);
            reader.Read(out character1Skill2Uid);
            reader.Read(out character1Skill3Uid);
            
            reader.Read(out character1Id);
            reader.Read(out character1Slot0Id);
            reader.Read(out character1Slot1Id);
            reader.Read(out character1Slot2Id);
            reader.Read(out character1Slot3Id);
            
            reader.Read(out character1Id);
            reader.Read(out character1Relic0Uid);
            reader.Read(out character1Relic1Uid);
            reader.Read(out character1Relic2Uid);
            reader.Read(out character1Relic3Uid);
            
            reader.Read(out character2Id);
            reader.Read(out character2Skill0Uid);
            reader.Read(out character2Skill1Uid);
            reader.Read(out character2Skill2Uid);
            reader.Read(out character2Skill3Uid);
            
            reader.Read(out character2Id);
            reader.Read(out character2Slot0Id);
            reader.Read(out character2Slot1Id);
            reader.Read(out character2Slot2Id);
            reader.Read(out character2Slot3Id);
            
            reader.Read(out character2Id);
            reader.Read(out character2Relic0Uid);
            reader.Read(out character2Relic1Uid);
            reader.Read(out character2Relic2Uid);
            reader.Read(out character2Relic3Uid);
            
            reader.Read(out character3Id);
            reader.Read(out character3Skill0Uid);
            reader.Read(out character3Skill1Uid);
            reader.Read(out character3Skill2Uid);
            reader.Read(out character3Skill3Uid);
            
            reader.Read(out character3Id);
            reader.Read(out character3Slot0Id);
            reader.Read(out character3Slot1Id);
            reader.Read(out character3Slot2Id);
            reader.Read(out character3Slot3Id);
            
            reader.Read(out character3Id);
            reader.Read(out character3Relic0Uid);
            reader.Read(out character3Relic1Uid);
            reader.Read(out character3Relic2Uid);
            reader.Read(out character3Relic3Uid);

            return this;
        }
    }

    public class SharedSkillInfo : CorgiSharedObject
    {
        public string characterId;
        public ulong baseUid;
        public uint level;
        public uint grade;
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(characterId);
            writer.Write(baseUid);
            writer.Write(level);
            writer.Write(grade);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out characterId);
            reader.Read(out baseUid);
            reader.Read(out level);
            reader.Read(out grade);

            return this;
        }
    }

    public class SharedSkillSlot : CorgiSharedObject
    {
        public string dbId;
        public uint mastery;

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }

            writer.Write(dbId);
            writer.Write(mastery);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }

            reader.Read(out dbId);
            reader.Read(out mastery);

            return this;
        }
    }

    public class SharedBindingStone : CorgiSharedObject
    {
        public uint level;

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(level);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out level);

            return this;
        }
    }
}