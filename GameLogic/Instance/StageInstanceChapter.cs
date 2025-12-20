using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageInstanceChapter : Stage, ICorgiInterface<InstanceChapterInfoSpec>
    {
	    private InstanceChapterInfoSpec _spec;

	    public InstanceChapterInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public StageInstanceChapter(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceChapterInfoSpec>(uid);
		    var dungeon = Dungeon as DungeonInstance;

		    if (sheetData == null || dungeon == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;

		    foreach (var monUid in _spec.BossUids)
		    {
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    monster.SetLevel(dungeon.Level);
			    
			    AddMonster(monster);
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