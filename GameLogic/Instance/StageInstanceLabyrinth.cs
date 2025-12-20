using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageInstanceLabyrinth : Stage, ICorgiInterface<InstanceLabyrinthInfoSpec>
    {
	    private InstanceLabyrinthInfoSpec _spec;

	    public InstanceLabyrinthInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public StageInstanceLabyrinth(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceLabyrinthInfoSpec>(uid);
		    var dungeon = Dungeon as DungeonInstance;

		    if (sheetData == null || dungeon == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;

		    var index = 0;
		    foreach (var monUid in _spec.BossUids)
		    {
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    monster.SetLevel(_spec.BossLevels[index]);
			    
			    AddMonster(monster);

			    index++;
		    }

		    foreach (var curUnit in MonsterList)
		    {
			    if (curUnit != null)
			    {
				    CorgiCombatLog.Log(CombatLogCategory.Dungeon,"Monster : {0} ({1})" , curUnit.Name, curUnit.ObjectId);
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