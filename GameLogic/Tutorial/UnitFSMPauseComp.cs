
using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class UnitFSMPauseComp : PauseComp
    {
        private UnitState _unitState;
        private UnitStateEvent _unitStateEvent;
        private SkillActionType _skillActionType;
            
        public UnitFSMPauseComp()
            : base(PauseCommandType.PauseCommand_UnitFSM, 3)
        {
        }
        
        protected override bool LoadInternal(ulong uid)
        {
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }

            var thisSpec = GetSpec();

            if (thisSpec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid pause uid : {0}\n", uid);
                return false;
            }

			_unitState = CorgiEnum.ParseEnum<UnitState>(thisSpec.Param0);
			_unitStateEvent = CorgiEnum.ParseEnum<UnitStateEvent>(thisSpec.Param1);
			_skillActionType = CorgiEnum.ParseEnum<SkillActionType>(thisSpec.Param2);
            
            return true;
        }

        // public override bool Initialize(Unit owner, params int[] args)
        // {
        //     if (args.Length != ParamCount)
        //     {
        //         CorgiLog.Log(CorgiLogType.Error, "[Tutorial] Initialize Invalid Pause Args ({0})", Command);
        //         return false;
        //     }
        //
        //     _unitState = (UnitState)args[2];
        //     _unitStateEvent = (UnitStateEvent)args[3];
        //     
        //     return base.Initialize(owner, args);
        // }
        
        public override bool CheckCondition(Unit unit, params int[] args)
        {
            if (args.Length != ParamCount) // except count/target
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial, "CheckCondition Invalid Pause Args ({0})", Command);
                return false;
            }

            var unitState = (UnitState)args[0];
            var unitStateEvent = (UnitStateEvent)args[1];
            var skillActionType = (SkillActionType)args[2];
            
            if (unitState != _unitState || unitStateEvent != _unitStateEvent)
            {
                return false;
            }

            if (_skillActionType != SkillActionType.None && _skillActionType != skillActionType)
            {
                return false;
            }

            var ret= base.CheckCondition(unit, args);
            if (ret)
            {
                // if true
                CorgiCombatLog.Log(CombatLogCategory.Tutorial, "UnitFSM Pause is running {0}:{1}:{2}:{3}"
                    , _pauseTarget, unitState, unitStateEvent, NextUid);
            }
            return ret;
        }
    }
}
