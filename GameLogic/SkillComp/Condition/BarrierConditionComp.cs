using System;
using Newtonsoft.Json.Linq;
using IdleCs.GameLog;
using IdleCs.Managers;

namespace IdleCs.GameLogic
{
    public class BarrierConditionComp : ConditionComp
    {
        protected override bool LoadInternal(ulong uid)
        {
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }
            
            return true;
        }

        public override int CheckActive(SkillCompLogNode logNode)
        {
            var dungeon = Owner.Dungeon;
            var target = dungeon.GetUnit(logNode.TargetId);
            var unit = GetUnit(Owner, target, logNode);

            var hasBarrier = unit.ApplyBarrier();

            return hasBarrier ? 1 : 0;
        }
    }
}