using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageInstance : Stage, ICorgiInterface<InstanceStageInfoSpec>
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private InstanceStageInfoSpec _spec;

	    public InstanceStageInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    /// <summary>
	    /// dynamic data
	    /// </summary>
	    public StageInstance(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceStageInfoSpec>(uid);

		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", GameDataManager.GetStringByUid(uid));
			    return false;
		    }

		    _spec = sheetData;

		    if (sheetData.BossUids.Count != sheetData.BossLevels.Count)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid Minion Setting : {0}", uid);
			    return false;
		    }

		    if (LoadMonsters() == false)
		    {
			    return false;
		    }

		    return base.LoadInternal(uid);
	    }
	    
	    protected virtual bool LoadMonsters()
	    {
		    var sheetData = _spec;
		    var index = 0;
		    foreach (var monUid in sheetData.BossUids)
		    {
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    monster.SetLevel(sheetData.BossLevels[index]);
			    //monster.SetModelTest(index);
			    
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

		    return true;

	    }
    }
}
