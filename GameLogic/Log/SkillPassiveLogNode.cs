using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using UnityEngine;

namespace IdleCs.GameLog
{
	[System.Serializable]
	public class SkillPassiveLogNode : SkillActorLogNode
	{
		public string SkillId;
		public ulong SkillUid;
		public SkillType SkillType;

		public SkillPassiveLogNode()
		{}
		public SkillPassiveLogNode(Unit owner, Skill skill)
			: base(owner, owner, null)
		{
			SkillId = skill.ObjectId;
			SkillUid = skill.Uid;
			SkillType = skill.SkillType;

			SetSkillActorInfo(skill);
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SkillPassive;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(SkillId);
            writer.Write(SkillUid);
            writer.WriteEnum(SkillType);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out SkillId);
            reader.Read(out SkillUid);
            reader.ReadEnum(out SkillType);

            return this;
		}
		
		public override string GetName()
		{
			var spec = GameDataManager.Instance.GetData<SkillPassiveInfoSpec>(SkillUid);
			if (spec == null)
			{
				return String.Empty;
			}

			return spec.Name;
		}

		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
		{
			var skillSpec = bridge.GetSpec<SkillInfoSpec>(SkillUid);

			var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);

			if (skillSpec == null || caster == null)
			{
				//throw new CorgiException("invalid action log node");
				return;
			}

			var casterName = caster.GetName(bridge);

			var combatLogData = new CombatLogData(CombatLogType.SkillPassive, index, this);
			index++;

			combatLogData.Message = string.Format("{0} SkillPassive OnActive ({1})", casterName, skillSpec?.Name);

			logDataList.Add(combatLogData);

		}

	}
	
}

