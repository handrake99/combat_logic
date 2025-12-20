using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using IdleCs.GameLogic;
using IdleCs.Library;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;

namespace IdleCs.GameLog
{
	public class SkillEventLogNode : SkillActorLogNode
	{
		public CombatEventType EventType;
	    
	    [JsonIgnore] private string _ownerName;
	    
		public SkillEventLogNode()
			: base()
		{}
		
		public SkillEventLogNode(CombatEventType eventType, EventParam eventParam, Unit owner)
			: base(owner,  eventParam.GetCaster(), eventParam.GetTarget())
		{
			EventType = eventType;
			_ownerName = owner.Name;
			
			SetSkillActorInfo(eventParam.GetSkillActor());
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillEvent;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.WriteEnum(EventType);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.ReadEnum(out EventType);

            return this;
		}
		
		public override string GetName()
		{
			return string.Format("{0}({1})", _ownerName, EventType.ToString());
		}
		
		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			var owner = DungeonLogNode.SharedInstance.GetUnit(OwnerId);

			var ownerName = owner.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillActive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} OnEvent({1})" , ownerName, EventType.ToString());
			logDataList.Add(combatLogData);

		}
    }
}