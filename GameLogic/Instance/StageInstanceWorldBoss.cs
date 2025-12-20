using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class StageInstanceWorldBoss : Stage, ICorgiInterface<InstanceWorldBossSpec>
    {
	    private InstanceWorldBossSpec _spec;
	    
	    private long _maxHP;
	    private long _curHP;

	    public InstanceWorldBossSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public StageInstanceWorldBoss(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceWorldBossSpec>(uid);

		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;

		    if (_spec.BossUids.Count <= 0)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}, Boss Count Error", uid);
			    return false;
		    }

		    for(var i=0 ; i<_spec.BossUids.Count; i++)
		    {
			    var monUid = _spec.BossUids[i];
			    
			    // set monster
			    var monster = new MonsterWorldBoss(Dungeon);
			    
			    monster.InitBossHP(_maxHP, _curHP);

			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    uint maxCharLevel = 0;
			    foreach (var charInst in Dungeon.CharList)
			    {
				    if (charInst == null)
				    {
					    continue;
				    }

				    if (maxCharLevel < charInst.Level)
				    {
					    maxCharLevel = charInst.Level;
				    }
			    }

			    monster.SetLevel(maxCharLevel);
			    
			    AddMonster(monster);
			    break; // only one
		    }

		    foreach (var curUnit in MonsterList)
		    {
			    if (curUnit != null)
			    {
				    CorgiCombatLog.Log(CombatLogCategory.Dungeon, "Monster : {0} ({1})" , curUnit.Name, curUnit.ObjectId);
			    }
		    }

		    return base.LoadInternal(uid);
	    }
	    
        public void InitBossHP(long maxHP, long curHP)
        {
	        _maxHP = maxHP;
	        _curHP = curHP;
        }
	    
		public override ulong GetBossSceneTime()
		{
			return CorgiLogicConst.WorldBossSceneDelay;
		}
		
		protected override void SetMonsterPositions(List<Unit> monsterList)
		{
			if (monsterList == null)
			{
				return;
			}

			var charList = Dungeon.CharList;
			if (charList == null)
			{
				return;
			}
			
			int mageNum = 0;

			foreach (var unit in charList)
			{
				if (unit == null)
				{
					continue;
				}
				
				if (unit.ClassType == ClassType.CtMage)
				{
					mageNum++;
				}
			}

			int postfix = 1;

			if (mageNum == 1)
			{
				postfix = 1;
			}
			else if (mageNum == 0)
			{
				postfix = 2;
			}
			else if (mageNum == 2)
			{
				postfix = 3;
			}
			else
			{
				CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid mageNum[{0}]", mageNum);
			}

			string uidStr = $"art_formation.worldboss.{postfix}";

			SetMonsterPositions(monsterList, uidStr);
		}
    }
}
