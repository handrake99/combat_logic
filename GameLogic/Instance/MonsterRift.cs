
using System;
using System.Collections.Generic;
using IdleCs.Library;
using IdleCs.Utils;

using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class MonsterRift : Monster
    {
	    private long _initMaxHP;
	    private long _initCurHP;
	    
        public MonsterRift(Dungeon dungeon)
            : base(dungeon)
        {
        }

        public override void SetLevel(uint level)
        {
	        Level = level;
	        
	        var dungeon = Dungeon as DungeonRift;
	        var gradeMod = 1f;
	        if (dungeon != null)
	        {
		        gradeMod += dungeon.MonsterMod;
	        }
	        
	        var sheet = GetSpec();
	        
	        //var constValueMaxHP = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.maxHP", 500);
	        var constValueAttackPower = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.attackPower", 500);
	        var constValueDefence = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.defence", 500);
	        var constValueHit = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.hit", 500);
	        var constValueEvasion = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.evasion", 500);
	        var constValueCrit = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.crit", 500);
	        var constValueResilience = Dungeon.GameData.GetConfigNumber("config.combat.factor.stat.resilience", 500);

	        StatMap[StatType.StMaxHp].OrigStat = _initMaxHP;
	        StatMap[StatType.StAttackSpeed].OrigStat = GetStat(StatType.StAttackSpeed, sheet, Level, 0);
		    StatMap[StatType.StAttackPower].OrigStat = (long)(GetStat(StatType.StAttackPower, sheet, Level, constValueAttackPower) * gradeMod);
		    StatMap[StatType.StDefence].OrigStat = (long)(GetStat(StatType.StDefence, sheet, Level, constValueDefence) * gradeMod);
		    StatMap[StatType.StHit].OrigStat = (long)(GetStat(StatType.StHit, sheet, Level, constValueHit) * gradeMod);
		    StatMap[StatType.StEvasion].OrigStat = (long)(GetStat(StatType.StEvasion, sheet, Level, constValueEvasion) * gradeMod);
		    StatMap[StatType.StCrit].OrigStat = (long)(GetStat(StatType.StCrit, sheet, Level, constValueCrit) * gradeMod);
		    StatMap[StatType.StResilience].OrigStat =  (long)(GetStat(StatType.StResilience, sheet, Level, constValueResilience) * gradeMod);

	        CurHP = _initCurHP;
	        if (CurHP > MaxHP)
	        {
		        CurHP = MaxHP;
	        }
        }
        
        public void InitBossHP(long maxHp, long curHp)
        {
	        _initMaxHP = maxHp;
	        _initCurHP = curHp;
        }

        public void OnUpdateCurHP(long curHP)
        {
	        //ResetHP(curHP);
        }
        
        public override void OnEnterCombat(CombatLogNode logNode)
        {
	        base.OnEnterCombat(logNode);
	        CurHP = _initCurHP;
        }
    }
}
