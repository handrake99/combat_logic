using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLogic;
using IdleCs.Managers;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{

	[Serializable]
	public class AbsorbContinuousSkillCompLogNode : ActiveSkillCompLogNode
	{
		public CorgiStringList ContinuousList;
		public CorgiUlongList ContinuousUidList;
		public uint AbsorbCount;
		public uint Result;

		private SkillEffectInst _resultInst = null;
		
		public AbsorbContinuousSkillCompLogNode()
		{}
		public AbsorbContinuousSkillCompLogNode(Unit caster, Unit target, ActiveSkillComp skillComp)
			: base(caster, target, skillComp)
		{
			ContinuousList = new CorgiStringList();
			ContinuousUidList = new CorgiUlongList();
			AbsorbCount = (uint)skillComp.BaseAmount;
			Result = 0;
		}

		public uint DoAbsorb(Unit caster, Unit target, SkillEffectInst inst)
		{
			if (inst.IsDispel == false)
			{
				return 0;
			}
			if (AbsorbCount <= 0 || inst.StackCount <= 0)
			{
				return 0;
			}
			
            AddDetailLog($"Try Absorb {inst.Target.Name}'s {inst.Name}, stackCount {AbsorbCount}");

            var result = inst.DoOver(AbsorbCount);

            if (result <= 0)
            {
	            return result;
            }
            
			ContinuousList.Add(inst.ObjectId);
			ContinuousUidList.Add(inst.Uid);

			if (AbsorbCount < result)
			{
				result = AbsorbCount;
				
				Result += AbsorbCount;
				AbsorbCount = 0;
			}
			else
			{
				AbsorbCount -= result;
				Result += result;
			}
			
			// clone & move
			var curList = caster.ContinuousList;
			var isInsert = true;

			// 이전 결과물이 있을때 거기에 업데이트함
			if (_resultInst != null)
			{
				if (_resultInst.Stackable)
				{
					_resultInst.DoStack((int)result);
				}

				_resultInst.ResetDuration();
				AddDetailLog($"Apply to {caster.Name} with {_resultInst.Name}, stackCount {_resultInst.StackCount} : {_resultInst.CurDuration}");

				isInsert = false;
			}
			else
			{
				// 이미 가지고 있는 것의 경우
				foreach (var curInst in curList)
				{
					if (curInst.IsUniqueInUnit==false && curInst.Uid == inst.Uid)
					{
						  if (curInst.Stackable)
						  {
							  curInst.DoStack((int)result);
						  }

						  curInst.ResetDuration();

						  isInsert = false;

						  _resultInst = curInst;
						  AddDetailLog($"Apply to {caster.Name} with {_resultInst.Name}, stackCount {_resultInst.StackCount} : {_resultInst.CurDuration}");
						  break;
					}
				}
			}
			
			if (isInsert)
			{
				_resultInst = inst.Clone(caster, result);

	            if (_resultInst == null)
	            {
		            CorgiCombatLog.Log(CombatLogCategory.Skill,"invalid uid for skilleffect. {0}", inst.Uid);
		            return result;
	            }

                _resultInst.ResetDuration();
                
                caster.ApplySkillEffect(_resultInst, this);
				AddDetailLog($"Apply to {caster.Name} with {_resultInst.Name}, stackCount {_resultInst.StackCount} : {_resultInst.CurDuration}");
				_resultInst.OnDoApply(_resultInst, this);
            }

            AddDetailLog($"Absorb {inst.Name}, stackCount {result}");
			
			return result;
			
		}
		
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.AbsorbContinuous;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
			if (base.Serialize(writer) == false)
			{
				return false;
			}
			
			ContinuousList.Serialize(writer);
			ContinuousUidList.Serialize(writer);
			
			writer.Write(AbsorbCount);
			writer.Write(Result);

			return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}

			ContinuousList = new CorgiStringList();
			ContinuousList.DeSerialize(reader);
			ContinuousUidList = new CorgiUlongList();
			ContinuousUidList.DeSerialize(reader);

			reader.Read(out AbsorbCount);
			reader.Read(out Result);

			return this;
		}
		
	    protected override void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
            var caster = DungeonLogNode.SharedInstance.GetUnit(CasterId);
            var target = DungeonLogNode.SharedInstance.GetUnit(TargetId);

            if (caster == null || target == null)
            {
	            throw new CorgiException("invalid dispel skillcomp log node");
            }

            var casterName = caster.GetName(bridge);
            var targetName = target.GetName(bridge);
            
            var combatLogData = new CombatLogData(CombatLogType.ActiveSkillComp, index, this);
            index++;

            
			combatLogData.Message = string.Format("{0} Absorb Count({1}) to {2}", casterName, Result, targetName);
			
			logDataList.Add(combatLogData);
			
	    }
	}
}
