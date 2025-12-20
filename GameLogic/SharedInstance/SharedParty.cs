using System;
using System.Collections.Generic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;

namespace IdleCs.GameLogic.SharedInstance
{
    
    public class SharedPartyInfo : CorgiSharedObject
    {
        public SharedPartySettingInfo characterCoPartySetting;
        public CorgiList<SharedSkillInfo> skills;
        public CorgiList<SharedRelicInfo> relics;
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            characterCoPartySetting.Serialize(writer);
            skills.Serialize(writer);
            relics.Serialize(writer);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            characterCoPartySetting = new SharedPartySettingInfo();
            characterCoPartySetting = (SharedPartySettingInfo)characterCoPartySetting.DeSerialize(reader);
            skills = new CorgiList<SharedSkillInfo>();
            skills = skills.DeSerialize(reader);
            relics = new CorgiList<SharedRelicInfo>();
            relics = relics.DeSerialize(reader);

            return this;
        }
    }
}