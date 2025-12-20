using System;
using System.Collections.Generic;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{
    [Serializable]
	public class HealSkillCompLogNode : ActiveSkillCompLogNode
    {
	    /// <summary>
	    /// Result
	    /// </summary>
	    public ulong FinalHeal
	    {
		    get { return (ulong)(Heal); }
	    }

	    public float Heal;
	    public long AppliedHeal;
	    
		
	    public override SctMessageInfo SctMessageInfo
	    {
		    get
		    {
			    var messageType = (IsCritical) ? SctMessageType.HealCritical : SctMessageType.Heal;
			    return new SctMessageInfo(messageType, IsMiss, IsImmune, $"+{CorgiUtil.GetNumberText(FinalHeal, true, false, 0)}"); 
		    }
	    }
	    
		public HealSkillCompLogNode()
		{}
		public HealSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
	    
	    // call by feature
		public override void AmplifyOutput(float mod)
		{
			Heal += Heal * mod;
		}
		
		public override void ApplyEnhance(float absoluteValue, float percentPlusValue, float percentMinusValue)
		{
			var preHeal = Heal;
			Heal = Heal * percentPlusValue * percentMinusValue + absoluteValue;
			AddDetailLog($"ApplyEnhance {preHeal} to {Heal}, {absoluteValue}, {percentPlusValue}, {percentMinusValue}");
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Heal;
	    }
	    
		public override SkillCompResultType GetLogType()
		{
			return SkillCompResultType.Heal;
		}

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(Heal);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }
	        
	        reader.Read(out Heal);
	        
	        return this;
        }

	    public override void LogDebug(IGameDataBridge bridge)
		{
			CorgiCombatLog.Log(CombatLogCategory.Skill,"{0} Heal {1} to {2}", CasterId, Heal, TargetId);
			
			base.LogDebug(bridge);
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
            
            var combatLogData = new CombatLogData(CombatLogType.ActiveSkillComp, index, this);
            index++;
		    
			combatLogData.Message = string.Format("{0} Heal {1} to {2}", casterName, FinalHeal, targetName);
			
			logDataList.Add(combatLogData);
			
	    }
	}
}