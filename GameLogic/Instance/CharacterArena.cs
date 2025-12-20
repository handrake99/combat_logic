using Corgi.GameData;

namespace IdleCs.GameLogic
{
    public class CharacterArena : Character
    {
        public CharacterArena(Dungeon dungeon)
			: base(dungeon)
        {
        }

        public override string Name
        {
            get
            {
                if (CombatSideType == CombatSideType.Player)
                {
                    return base.Name + "_A";

                }
                else
                {
                    return base.Name + "_D";
                }
            }
        }

        protected override bool LoadInternal(ulong uid)
        {
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }

            var preMaxHP = StatMap[StatType.StMaxHp].OrigStat;
            var maxHPFactor = Dungeon.GameData.GetConfigFloat("config.arena.factor.stat.maxHP", 1.0f);
		    StatMap[StatType.StMaxHp].OrigStat = (long)(preMaxHP * maxHPFactor);

            return true;
        }

        public void SetCombatSide(CombatSideType sideType)
        {
            CombatSideType = sideType;
        }

        public override float MoveSpeed
        {
            get
            {
                if (UnitState == UnitState.Exploration) return (float)(1.5 * base.MoveSpeed);
                return base.MoveSpeed;
            }
        }
    }
}