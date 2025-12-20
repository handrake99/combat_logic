using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class CheckContinuousOwnerConditionComp : ConditionComp
    {
        private SkillTargetType _targetType;
        
        public CheckContinuousOwnerConditionComp()
        {
        }

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

            int count = 0;

            if (unit == null)
            {
                return 0;
            }

            foreach (var inst in unit.ContinuousList)
            {
                if (inst == null)
                {
                    continue;
                }

                bool isCorrect = true;

                if (inst.Caster.ObjectId == unit.ObjectId)
                {
                    count += (int)inst.StackCount;
                }
            }
            
            return GetRetValue(count);
        }
    }
}
