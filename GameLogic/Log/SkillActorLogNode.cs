using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;


namespace IdleCs.GameLog
{
	public abstract class SkillActorLogNode : CombatLogNode
	{
		public ulong SkillBaseUid;
		public ulong SkillUid;
		
		public SkillActorType ActorType;
		public SkillType SkillType;
		public SkillAreaType SkillAreaType;
		public SkillAttributeType SkillAttributeType;

		public string OwnerId;
		public string CasterId;
		public string TargetId = string.Empty;
		
		
		
		protected SkillActorLogNode()
			: base()
		{
		}

		protected SkillActorLogNode(Unit owner, Unit caster, Unit target)
			: base()
		{
			if (owner == null)
			{
				throw new CorgiException("invalid parameter in skillactorlognode");
			}
			OwnerId = owner.ObjectId;
			CasterId = caster.ObjectId;
			if (target != null)
			{
				TargetId = target.ObjectId;
			}
		}

		protected void SetSkillActorInfo(ISkillActor actor)
		{
			if (actor == null)
			{
				return;
			}

			SkillBaseUid = actor.SkillBaseUid;
			SkillUid = actor.SkillUid;

			ActorType = actor.SkillActorType;
			SkillType = actor.SkillType;
			SkillAttributeType = actor.AttributeType;
		}
		

		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
	        writer.Write(SkillBaseUid);
	        writer.Write(SkillUid);
            writer.WriteEnum(ActorType);
            writer.WriteEnum(SkillType);
            writer.WriteEnum(SkillAreaType);
            writer.WriteEnum(SkillAttributeType);
            
            writer.Write(OwnerId);
            writer.Write(CasterId);
            writer.Write(TargetId);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
	        reader.Read(out SkillBaseUid);
	        reader.Read(out SkillUid);
            reader.ReadEnum(out ActorType);
            reader.ReadEnum(out SkillType);
            reader.ReadEnum(out SkillAreaType);
            reader.ReadEnum(out SkillAttributeType);
            
            reader.Read(out OwnerId);
            reader.Read(out CasterId);
            reader.Read(out TargetId);

            return this;
		}

		public abstract string GetName();
	}
}
