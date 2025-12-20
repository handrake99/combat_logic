using Corgi.GameData;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageAdventureMinion : Stage, ICorgiInterface<AdventureStageInfoSpec>
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private AdventureStageInfoSpec _spec;

	    public AdventureStageInfoSpec GetSpec()
	    {
		    return _spec;}
	    
	    /// <summary>
	    /// dynamic data
	    /// </summary>
	    public StageAdventureMinion(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<AdventureStageInfoSpec>(uid);

		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;

		    if (sheetData.MinionUids.Count != sheetData.MinionLevels.Count)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid Minion Setting : {0}", uid);
			    return false;
		    }
		    
		    // todo read monsters;
		    var index = 0;
		    foreach (var monUid in sheetData.MinionUids)
		    {
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    continue;
			    }

			    monster.SetLevel(sheetData.MinionLevels[index]);
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
		    

		    return base.LoadInternal(uid);
	    }
    }
}
