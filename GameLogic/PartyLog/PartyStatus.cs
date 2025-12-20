using System;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic
{
    public class PartyStatus : CorgiSharedObject
    {
        public CorgiList<PartyMemberStatus> MemberStatusList = new CorgiList<PartyMemberStatus>();
        
        public PartyMemberStatus OnConnected(string characterId)
        {
            var memberStatus = GetPartyMemberStatus(characterId);
            if (memberStatus == null)
            {
                memberStatus = new PartyMemberStatus();
                
                memberStatus.CharacterId = characterId;
                MemberStatusList.Add(memberStatus);
            }
            memberStatus.Connected = true;

            MemberStatusList.Add(memberStatus);

            return memberStatus;
        }

        public PartyMemberStatus OnClosed(string characterId)
        {
            // remove
            foreach (var thisStatus in MemberStatusList)
            {
                if (thisStatus == null || thisStatus.CharacterId != characterId)
                {
                    continue;
                }

                MemberStatusList.Remove(thisStatus);

                thisStatus.Connected = false;
                return thisStatus;
            }

            return null;
        }

        public PartyMemberStatus GetPartyMemberStatus(string unitId)
        {
            foreach (var thisStatus in MemberStatusList)
            {
                if (thisStatus != null && thisStatus.CharacterId == unitId)
                {
                    return thisStatus;
                }
            }
            return null;
        }

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }

            MemberStatusList.Serialize(writer);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }

            MemberStatusList = new CorgiList<PartyMemberStatus>();
            MemberStatusList.DeSerialize(reader);
            
            return this;
        }
        
    }

    public class PartyMemberStatus : CorgiSerializable
    {
        // characterId 
        public string CharacterId;
        
        // connect 상태
        public bool Connected;
        
        // user action event
        public UserAction UserAction = UserAction.None;
        
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

            writer.Write(CharacterId);
            writer.Write(Connected);
            writer.WriteEnum(UserAction);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }

            reader.Read(out CharacterId);
            reader.Read(out Connected);
            reader.ReadEnum(out UserAction);

            return this;
        }
    }
}