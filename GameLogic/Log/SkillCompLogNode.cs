using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;

namespace IdleCs.GameLog
{
	public abstract class SkillCompLogNode : SkillActorLogNode
	{
		public bool IsImmune;
		public int ActiveCount;

		public ulong SkillCompUid;

	    private SkillComp _skillComp;

	    private Dictionary<EnhanceType, EnhanceCacheStruct> _enhanceMap = new Dictionary<EnhanceType, EnhanceCacheStruct>();
	    
	    public virtual SctMessageInfo SctMessageInfo => SctMessageInfo.Null;
	    
		public SkillCompLogNode()
		{}
		public SkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, caster, target)
		{
			if (skillComp != null)
			{
				SkillCompUid = skillComp.Uid;
			}

			ActiveCount = 1;
			_skillComp = skillComp;
		}

		public ulong GetSkillBaseUid()
		{
			if (_skillComp == null)
			{
				return 0;
			}

			var parent = this.Parent as SkillActorLogNode;
			if (parent == null)
			{
				return 0;
			}

			return parent.SkillBaseUid;
			
		}

		public SkillType GetSkillType()
		{
			if (_skillComp == null)
			{
				return SkillType.None;
			}

			var parent = this.Parent as SkillActorLogNode;
			if (parent == null)
			{
				return SkillType.None;
			}

			return parent.SkillType;
		}
		
		public SkillAreaType GetSkillAreaType()
		{
			if (_skillComp == null)
			{
				return SkillAreaType.None;
			}

			
			var parent = this.Parent as SkillActorLogNode;
			if (parent == null)
			{
				return SkillAreaType.None;
			}

			return parent.SkillAreaType;
		}

		public SkillActorType GetSkillActorType()
		{
			if (_skillComp == null)
			{
				return SkillActorType.None;
			}

			
			var parent = this.Parent as SkillActorLogNode;
			if (parent == null)
			{
				return SkillActorType.None;
			}

			return parent.ActorType;
			
		}
		

		public SkillAttributeType GetAttributeType()
		{
			if (_skillComp == null)
			{
				return SkillAttributeType.SatNone;
			}

			return _skillComp.AttributeType;
		}

		public virtual SkillCompResultType GetLogType()
		{
			return SkillCompResultType.None;
		}

		public void AddEnhanceValue(EnhanceType enhanceType, float absoluteValue, float percentPlusValue,
			float percentMinusValue)
		{
			EnhanceCacheStruct enhanceStruct;
			if (_enhanceMap.ContainsKey(enhanceType) == false)
			{
				enhanceStruct = new EnhanceCacheStruct();
				_enhanceMap.Add(enhanceType, enhanceStruct);
			}
			else
			{
				enhanceStruct = _enhanceMap[enhanceType];
			}
			enhanceStruct.AddEnhance(absoluteValue, percentPlusValue, percentMinusValue);
		}

		public void GetEnhanceValue(EnhanceType enhanceType, ref float absoluteValue, ref float percentPlusValue,
			ref float percentMinusValue)
		{
			if (_enhanceMap.ContainsKey(enhanceType) )
			{
				var enhanceStruct = _enhanceMap[enhanceType];
				
				absoluteValue += enhanceStruct.AbsoloteValue;
				percentPlusValue += enhanceStruct.PercentPlusValue;
				percentMinusValue *= enhanceStruct.PercentMinusValue;
			}
		}

		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(IsImmune);
            writer.Write(ActiveCount);
            writer.Write(SkillCompUid);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out IsImmune);
            reader.Read(out ActiveCount);
            reader.Read(out SkillCompUid);

            return this;
		}

		public override string GetName()
		{
			return _skillComp.GetName();
		}
	}

	public struct SkillCompResultStruct
	{
		public SkillCompResultType ResultType;
		public string ResultName;
		public string ResultDesc;

		public SkillCompResultStruct(SkillCompResultType resultType)
		{
			ResultType = resultType;
			ResultName = String.Empty;
			ResultDesc = String.Empty;
		}
	}
}
