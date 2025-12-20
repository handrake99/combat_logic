
using System;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedAlmanacStat : CorgiSharedObject
    {
        public ulong ST_MaxHP;
        public ulong ST_AttackSpeed;
        public ulong ST_AttackPower;
        public ulong ST_Defence;
        public ulong ST_Hit;
        public ulong ST_Evasion;
        public ulong ST_Crit;
        public ulong ST_Resilience;
        public ulong ST_CritDmg;
        
        public override void Init(CorgiObject original)
        {
            base.Init(original);
        }
        
    }
}
