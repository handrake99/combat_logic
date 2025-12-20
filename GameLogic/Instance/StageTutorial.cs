using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageTutorial : Stage, ICorgiInterface<TutorialStageInfoSpec>
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private TutorialStageInfoSpec _spec;

	    public TutorialStageInfoSpec GetSpec()
	    {
		    return _spec;}
	    
	    /// <summary>
	    /// dynamic data
	    /// </summary>
	    public StageTutorial(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<TutorialStageInfoSpec>(uid);

		    if (sheetData == null)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid stage uid : {0}", uid);
			    return false;
		    }

		    _spec = sheetData;

		    if (sheetData.MonsterUids.Count != sheetData.MonsterLevels.Count)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Tutorial,"Invalid Minion Setting : {0}", uid);
			    return false;
		    }
		    
		    // todo read monsters;
		    var index = 0;
		    foreach (var monUid in sheetData.MonsterUids)
		    {
			    // set monster
			    var monster = new MonsterTutorial(Dungeon);

			    if (monster.Load(monUid) == false)
			    {
				    continue;
			    }

			    monster.SetLevel(sheetData.MonsterLevels[index]);
			    //monster.SetModelTest(index);
			    
			    AddMonster(monster);
			    
			    index++;
		    }
		    

		    foreach (var curUnit in MonsterList)
		    {
			    if (curUnit != null)
			    {
				    CorgiCombatLog.Log(CombatLogCategory.Tutorial,"Monster : {0} ({1})" , curUnit.Name, curUnit.ObjectId);
			    }
		    }
		    

		    return base.LoadInternal(uid);
	    }
	    public override ulong GetBossSceneTime()
	    {
		    if (_spec.IsBossScene)
		    {
				return CorgiLogicConst.BossSceneDelay;
		    }
		    
		    return 0;
	    }
    }
}