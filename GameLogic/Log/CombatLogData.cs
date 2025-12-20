using Google.Protobuf;
using IdleCs.GameLogic;
using IdleCs.Managers;
using UnityEngine;

namespace IdleCs.GameLog
{
    public enum CombatLogType
    {
        None = 0
        , Attack
        , SkillActive
        , SkillPassive
        , ActiveSkillComp
        , ContinuousSkillComp
        , PassiveSkillComp
    }
    public class CombatLogData 
    {
        public ulong CurTick { get; private set; }
        public string Message { get; set; }
        public int Index { get; }
        public int Height { get; set; }
        public string ActorId { get; set; }
        public CombatLogNode Node { get; }
        public CombatLogType LogType { get; private set; }

        public CombatLogData(CombatLogType logType, int i, CombatLogNode node)
        {
            LogType = logType;
            Index = i;
            Node = node;
            if (node != null)
            {
                //Message = node.GetLog(this);
                CurTick = node.CurTick;
                ActorId = null;
                //Height = 150;
                if (node is SkillActionLogNode)
                {
                    var actionLog = (SkillActionLogNode) node;
                    ActorId = actionLog?.CasterId;
                }
            }
        }
    }
}