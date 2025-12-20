using System;
using System.Collections.Generic;

using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{
    [Serializable]
	public class DamageSkillCompLogNode : ActiveSkillCompLogNode
    {
	    
	    /// <summary>
	    /// Result
	    /// </summary>
	    public ulong FinalDamage => (ulong)(Damage);


	    public float Damage;

	    public int AbsorbAmount;

	    public AttributeRelationType ElementalRelationType;

	    //public int AdvantageRate;

	    public long PreHP; // 적용전 HP
	    public long CurHP; // 적용후 HP
	    
	    
	    public override SctMessageInfo SctMessageInfo
	    {
		    get
		    {
			    var messageType = (IsCritical) ? SctMessageType.DamageCritical : SctMessageType.Damage;
			    return new SctMessageInfo(messageType, IsMiss, IsImmune, $"-{CorgiUtil.GetNumberText(FinalDamage, true, false, 0)}"); 
		    }
	    }

	    public DamageSkillCompLogNode()
			: base()
	    {
	    }

	    public DamageSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}
	    
	    
		public int Absorb(int amount, float absorbRate)
		{
			var preDamage = (int)Damage;
			int absorbAmount = (int)(preDamage * absorbRate);
			
			if(amount < absorbAmount)
			{
				// 흡수 량이 안남았다.
				absorbAmount = amount;
			}

			if(absorbAmount < preDamage)
			{
				// damage 가 남는 경우.
				Damage = preDamage - absorbAmount;
			}else
			{
				// 전부 흡수.
				absorbAmount = (int)Damage;
				Damage = 0;
			}

            AbsorbAmount += absorbAmount;
            
            AddDetailLog($"Damage : {preDamage} / Absorb : {absorbAmount} / Remain : {Damage}");

			return absorbAmount;
		}
	    
	    // call by feature
		public override void AmplifyOutput(float mod)
		{
			Damage += Damage * mod;
		}
	    
	    // call by unit
		public override void ApplyEnhance(float absoluteValue, float percentPlusValue, float percentMinusValue)
		{
			var preDamage = Damage;
			Damage = Damage * percentPlusValue * percentMinusValue + absoluteValue;
			AddDetailLog($"ApplyEnhance {preDamage} to {Damage}, {absoluteValue}, {percentPlusValue}, {percentMinusValue}");
		}
	    
	    // call by skillcomp
	    public override float TransferOutput(long transferValue, float transferRate, float reduceRate)
	    {
			if (reduceRate < 0f || transferRate < 0)
			{
				return 0f;
			}

		    var reduceDamage = transferValue + Damage * transferRate;
		    Damage = Damage - reduceDamage;

		    return reduceDamage * (1f- reduceRate);
	    }
	    
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Damage;
	    }
	    
		public override SkillCompResultType GetLogType()
		{
			return SkillCompResultType.Deal;
		}

	    public override bool Serialize(IPacketWriter writer)
	    {
		    if (base.Serialize(writer) == false)
		    {
			    return false;
		    }
		    
		    writer.Write(Damage);
		    writer.Write(AbsorbAmount);
		    writer.WriteEnum(ElementalRelationType);

		    writer.Write(PreHP);
		    writer.Write(CurHP);

		    return true;
	    }

	    public override ICorgiSerializable DeSerialize(IPacketReader reader)
	    {
		    if (base.DeSerialize(reader) == null)
		    {
			    return null;
		    }

		    reader.Read(out Damage);
		    reader.Read(out AbsorbAmount);
		    reader.ReadEnum(out ElementalRelationType);
		    
		    
		    reader.Read(out PreHP);
		    reader.Read(out CurHP);

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
            
            var combatLogData = new CombatLogData(CombatLogType.ActiveSkillComp, index, this);

            if (IsImmune)
            {
                combatLogData.Message = string.Format("{0} Damage Immune to {1}", casterName, targetName);
            }
            else
            {
                combatLogData.Message = string.Format("{0} Damage {1} to {2}", casterName, FinalDamage, targetName);
            }
            index++;
			
			logDataList.Add(combatLogData);
			
			
	    }
	    
	    public override void LogDebug(IGameDataBridge bridge)
	    {
			CorgiCombatLog.Log(CombatLogCategory.Skill,"{0} Damage {1} to {2}", CasterId, Damage, TargetId);
			
		    base.LogDebug(bridge);
	    }

	    
//	    protected override SctLog GetSctLog()
//	    {
//		    if (FinalDamage <= 0) { return SctLog.Null; }
//		    
//		    SctMessageType messageType = (IsCritical) ? SctMessageType.DamageCritical : SctMessageType.Damage;
//		    return new SctLog(TargetId, new SctMessageInfo(messageType, FinalDamage), StatusEffectNumber);
//	    }
	}
}