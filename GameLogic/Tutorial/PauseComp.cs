using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public enum PauseTargetType
    {
        PauseTarget_None = 0
        , PauseTarget_Self = 1       // 0001
        
        , PauseTarget_Knight = 2     // 0001 0
        , PauseTarget_Rogue = 4      // 0010 0
        , PauseTarget_Mage = 8       // 0100 0
        , PauseTarget_Druid = 16      // 1000 0
        
        , PauseTarget_Player = 32     // 01 00000
        , PauseTarget_Monster = 64    // 10 00000
    }
    public enum PauseCommandType
    {
        None = 0
        , PauseCommand_Dungeon = 1
        , PauseCommand_UnitFSM = 2
    }
    
    public class PauseComp : CorgiObject
    {
        // static data
        private GuidePauseInfoSpec _spec;
        private Unit _owner;
        
        private PauseCommandType _command;
        private int _paramCount;
        
        private int _stepCount;
        protected PauseTargetType _pauseTarget;
        
        // dynamic Data
        private int _curCount;
        
        //get method
        protected int ParamCount {get{
            return _paramCount;
        }}

        protected GuidePauseInfoSpec GetSpec()
        {
            return _spec;}

        public PauseCommandType Command => _command;
        public ulong NextUid => _spec.NextGuideUid;

        public PauseComp(PauseCommandType command, int paramCount)
        {
            _command = command;
            _paramCount = paramCount;
        }

        public void SetOwner(Unit owner)
        {
            _owner = owner;
        }

        protected override bool LoadInternal(ulong uid)
        {
            var thisSpec = _owner.Dungeon.GameData.GetData<GuidePauseInfoSpec>(uid);

            if (thisSpec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial," Invalid pause uid : {0}\n", uid);
                return false;
            }

            _spec = thisSpec;

            _stepCount = (int)thisSpec.StepCount;
            _pauseTarget = CorgiEnum.ParseEnum<PauseTargetType>(thisSpec.PauseTargetType);
            
            return true;
        }

        public virtual bool CheckCondition(Unit unit, params int[] args)
        {
            if (_curCount >= _stepCount)
            {
                return false;
            }

            var pauseTarget = 0;
            if (unit != null)
            {
                pauseTarget = GetTargetCode(unit);
            }
            
            if (((int)_pauseTarget != 0) && (pauseTarget & (int)_pauseTarget) == 0)
            {
                return false;
            }
            
            _curCount++;

            if (_stepCount != _curCount)
            {
                return false;
            }

            return true;
        }

        public PauseTargetType GetPauseTargetType()
        {
            switch (_pauseTarget)
            {
                case PauseTargetType.PauseTarget_Knight:
                case PauseTargetType.PauseTarget_Rogue:
                case PauseTargetType.PauseTarget_Mage:
                case PauseTargetType.PauseTarget_Druid:
                case PauseTargetType.PauseTarget_Player:
                case PauseTargetType.PauseTarget_Monster:
                    return _pauseTarget;
                default:
                    return PauseTargetType.PauseTarget_None;
                    break;
            }
        }
        
        int GetTargetCode(Unit unit)
        {
            var ret = 0;
            
            if (unit == null)
            {
                return 0;
            }

            if (_owner !=null && unit.DBId == _owner.DBId)
            {
                ret += 1;
            }

            if (unit.ClassType != ClassType.CtNone)
            {
                var classInt = (int)unit.ClassType;
                ret += 1 << classInt;
            }

            var sideTypeInt = (int) unit.CombatSideType;
            ret += sideTypeInt * 32;

            return ret;

        }
    }
}