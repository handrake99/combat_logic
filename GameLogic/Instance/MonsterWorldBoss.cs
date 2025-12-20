
using System;
using System.Collections.Generic;
using IdleCs.Library;
using IdleCs.Utils;

using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
    public class MonsterWorldBoss : Monster
    {
	    private long _initMaxHP;
	    private long _initCurHP;
	    
        public MonsterWorldBoss(Dungeon dungeon)
            : base(dungeon)
        {
        }

        protected override bool LoadInternal(ulong uid)
        {
	        var ret = base.LoadInternal(uid);
	        UnitSize = 30f;
	        
	        return ret;
        }
        
        public override void SetLevel(uint level)
        {
	        Level = level;
	        
	        var sheet = GetSpec();
	        
	        StatMap[StatType.StMaxHp].OrigStat = _initMaxHP;
	        StatMap[StatType.StAttackSpeed].OrigStat = sheet.AttackSpeed;
	        StatMap[StatType.StAttackPower].OrigStat = sheet.AttackPower;
	        StatMap[StatType.StDefence].OrigStat = sheet.Defence;
	        StatMap[StatType.StHit].OrigStat = sheet.Hit;
	        StatMap[StatType.StEvasion].OrigStat = sheet.Evasion;
	        StatMap[StatType.StCrit].OrigStat = sheet.Crit;
	        StatMap[StatType.StResilience].OrigStat = sheet.Resilience;

	        CurHP = _initCurHP;
        }

        public override void OnEnterCombat(CombatLogNode logNode)
        {
	        base.OnEnterCombat(logNode);
	        CurHP = _initCurHP;
        }

        
        public void InitBossHP(long maxHP, long curHP)
        {
	        _initMaxHP = maxHP;
	        _initCurHP = curHP;
        }

        public void OnUpdateCurHP(long curHP)
        {
	        ResetHP(curHP);
        }
        
        protected override string GetCrazySkillUidStr()
        {
	        return "skill.worldboss.crazy"; 
        }
    }
}