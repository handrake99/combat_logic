using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLogic;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{
    public class AddContinuousLogNode : SkillCompLogNode
    {
	    /// <summary>
	    /// Result
	    /// </summary>
	    //private bool _isImmune = false;
	    //private SkillEffectInst _inst = null;
	    public string InstId;

	    public ContinuousBenefitType BenefitType;

	    public bool IsResist;

		public AddContinuousLogNode()
		{}
		public AddContinuousLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.AddContinuous;
	    }
	    
		public override SkillCompResultType GetLogType()
		{
			if (BenefitType == ContinuousBenefitType.Buff)
			{
				return SkillCompResultType.Buff;
			}
			return SkillCompResultType.Debuff;
		}

		public override bool Serialize(IPacketWriter writer)
		{
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(InstId);
	        writer.WriteEnum(BenefitType);
	        writer.Write(IsResist);

	        return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }

	        reader.Read(out InstId);
	        reader.ReadEnum(out BenefitType);
	        reader.Read(out IsResist);

	        return this;
		}
        

		protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid action log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);

            var spec = GameDataManager.Instance.GetData<SkillContinuousInfoSpec>(SkillCompUid);
            
            var combatLogData = new CombatLogData(CombatLogType.ContinuousSkillComp, index, this);
            index++;
		    
            if (IsImmune)
            {
                combatLogData.Message = string.Format("{0} {1} Immune to {2}", casterName, spec?.Name, targetName);
            }
            else
            {
                combatLogData.Message = string.Format("{0} AddContinuous {1} to {2}", casterName, spec?.Name, targetName);
            }
			
			logDataList.Add(combatLogData);
			
	    }
	    
    }

	public class RemoveContinuousLogNode : SkillCompLogNode
	{
		//private StatusEffectSubType _statusEffectSubType;
		//private string _msgText = null;

		public RemoveContinuousLogNode()
		{
		}
		
		public RemoveContinuousLogNode(SkillEffectInst inst)
			: base(inst.Caster,inst.Target, inst.SkillComp)
		{
		}
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.RemoveContinuous;
	    }
		
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid action log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);

            var spec = GameDataManager.Instance.GetData<SkillContinuousInfoSpec>(SkillCompUid);
            
            var combatLogData = new CombatLogData(CombatLogType.ContinuousSkillComp, index, this);
		    
            combatLogData.Message = string.Format("{0} RemoveContinuous {1} to {2}", casterName, spec?.Name, targetName);
            index++;
			
			logDataList.Add(combatLogData);
	    }
		
	}
}