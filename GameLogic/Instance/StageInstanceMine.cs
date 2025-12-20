using Corgi.GameData;
using IdleCs.Utils;



namespace IdleCs.GameLogic
{
    public class StageInstanceMine : Stage, ICorgiInterface<InstanceMineInfoSpec>
    {
	    private InstanceMineInfoSpec _spec;

	    public InstanceMineInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public StageInstanceMine(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceMineInfoSpec>(uid);
		    var dungeon = Dungeon as DungeonInstance;

		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;
		    if (_spec.BossUids.Count != _spec.BossLevels.Count)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}, Boss Count Error", uid);
			    return false;
		    }

		    for(var i=0 ; i<_spec.BossUids.Count; i++)
		    {
			    var monUid = _spec.BossUids[i];
			    var monLevel = _spec.BossLevels[i];
			    
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    monster.SetLevel(monLevel);
			    
			    AddMonster(monster);
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
	    
		public override ulong GetBossSceneTime()
		{
			return CorgiLogicConst.BossSceneDelay;
		}
    }
}
