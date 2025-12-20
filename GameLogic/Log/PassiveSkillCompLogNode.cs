using System;

using System.Collections.Generic;
using IdleCs;

using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using UnityEditor;

namespace IdleCs.GameLog
{
	public class PassiveSkillCompLogNode : SkillCompLogNode
	{
		public PassiveSkillCompLogNode()
		{}
		public PassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}

		public override int GetClassType()
		{
			return (int) CombatLogNodeType.PassiveSkillComp;
		}
		
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
		    var passiveName = GetName();
		    if (passiveName == null)
		    {
			    // no name
			    return;
		    }
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid action log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);
            
            var combatLogData = new CombatLogData(CombatLogType.PassiveSkillComp, index, this);

			combatLogData.Message = string.Format("{0} {1} is ACTIVE to {2}", casterName, GetName(), targetName);
			
            index++;
			
			logDataList.Add(combatLogData);
	    }

	    protected virtual string GetName()
	    {
		    return null;
	    }
	    
	}
	
	public class MezPassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		//private Skill _skill;
		public MezType MezType;

		public override SctMessageInfo SctMessageInfo => new SctMessageInfo(MezType);
//		public Skill Skill
//		{
//			get { return _skill; }
//		}

		public MezPassiveSkillCompLogNode()
		{}
		public MezPassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
			var passiveComp = skillComp as MezPassiveComp;
			if (passiveComp != null)
			{
				MezType = passiveComp.MezType;
			}
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.MezPassive;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.WriteEnum(MezType);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.ReadEnum(out MezType);
	        
	        return this;
        }
        
	    protected override string GetName()
	    {
		    return string.Format("Mez({0})", MezType.ToString());
	    }
        
		
	}
	
	public class BarrierPassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		private int _maxAmount;

		public BarrierPassiveSkillCompLogNode()
		{}
		public BarrierPassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
			var passiveComp = skillComp as BarrierPassiveComp;
			if (passiveComp != null)
			{
				_maxAmount = passiveComp.GetBarrierAmount();
				var origAmount = passiveComp.GetOrigBarrierAmount();
				var enhancedAmount = passiveComp.GetEnhancedBarrierAmount();
				AddDetailLog($"OrigBarrierAmount : {origAmount}");
				AddDetailLog($"EnhancedBarrierAmount : {enhancedAmount}");
			}
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.BarrierPassive;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(_maxAmount);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out _maxAmount);
	        
	        return this;
        }
	    protected override string GetName()
	    {
		    return string.Format("Barrier({0})", _maxAmount);
	    }
		
	}

	public class SaveFromDeathPassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		public int CurHP;
		
		public SaveFromDeathPassiveSkillCompLogNode()
		{}
		public SaveFromDeathPassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.SaveFromDeath;
	    }
		
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(CurHP);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out CurHP);
	        
	        return this;
        }
        
	    protected override string GetName()
	    {
		    return string.Format("SaveFromDeath({0})", CurHP);
	    }
	}
	
	public class TransferDamagePassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		public long TotalDamage;
		public long TransferDamge;
		
		public TransferDamagePassiveSkillCompLogNode()
		{}
		public TransferDamagePassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.TransferDamage;
	    }
		
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        return this;
        }
        
	    protected override string GetName()
	    {
		    return string.Format("TransferDamage({0}/{1})", TransferDamge, TotalDamage);
	    }
	}
	public class ConvertToTrueDamagePassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		public ConvertToTrueDamagePassiveSkillCompLogNode()
		{}
		public ConvertToTrueDamagePassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.ConvertToTrueDamage;
	    }
		
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        return this;
        }
        
	    protected override string GetName()
	    {
		    return string.Format("ConvertToTrueDamge");
	    }
	}
	public class VampiricPassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		public VampiricPassiveSkillCompLogNode()
		{}
		public VampiricPassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Vampiric;
	    }
		
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        return this;
        }
	    protected override string GetName()
	    {
		    return string.Format("Vampiric");
	    }
	}
	
	public class ReflectDamagePassiveSkillCompLogNode : PassiveSkillCompLogNode
	{
		public ReflectDamagePassiveSkillCompLogNode()
		{}
		public ReflectDamagePassiveSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.ReflectDamage;
	    }
		
	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        return this;
        }
        
	    protected override string GetName()
	    {
		    return string.Format("ReflectDamage");
	    }
	}
}

