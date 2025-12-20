
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class DungeonPauseComp : PauseComp
    {
        private DungeonState _dungeonState;
        
        public DungeonPauseComp()
            : base(PauseCommandType.PauseCommand_Dungeon, 3)
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
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial," Invalid pause uid : {0}\n", uid);
                return false;
            }

            _dungeonState = CorgiEnum.ParseEnum<DungeonState>(thisSpec.Param0);
            
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
        //     _dungeonState = (DungeonState)args[2];
        //     
        //     return base.Initialize(owner, args);
        // }
        
        public override bool CheckCondition(Unit unit, params int[] args)
        {
            if (args.Length != ParamCount-2) // except count/target
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial, " CheckCondition Invalid Pause Args ({0})", Command);
                return false;
            }

            var dungeonState = (DungeonState) args[0];
            if (_dungeonState != dungeonState)
            {
                return false;
            }

            // if true
            var ret= base.CheckCondition(unit, args);
            if (ret)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Tutorial, " Dungoen Pause is running {0}:{1}", dungeonState, NextUid);
            }
            
            return ret;
        }
    }
}