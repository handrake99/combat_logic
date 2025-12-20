using System.Collections.Generic;

using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    /// <summary>
    /// test mode 용 던전.
    /// 모든 것을 세팅 할 수 있게 만든다. (ex. cooltime 제거, action delay 제거 등등)
    /// </summary>
    public class DungeonDummy : Dungeon
    {

        public DungeonDummy(ICombatBridge bridge)
            : base(bridge, DungeonType.Dummy)
        {
        }

        public override DungeonLogNode EnterDungeon(List<Unit> charList, ulong stageUid)
        {
            if (charList != null)
            {
                return base.EnterDungeon(charList, stageUid);
            }

            return null;
        }

    }
}