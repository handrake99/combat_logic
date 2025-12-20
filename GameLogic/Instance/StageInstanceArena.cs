using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;


namespace IdleCs.GameLogic
{
    public class StageInstanceArena : Stage
    {
	    public StageInstanceArena(Dungeon dungeon)
			:base(dungeon)
	    {
	    }

	    protected override bool LoadInternal(JObject jObject)
	    {
		    return base.LoadInternal(jObject);
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    return base.LoadInternal(uid);
	    }
	    
	    public void AddEnemy(Unit enemy)
	    {
		    if (enemy != null)
		    {
			    _monsterList.Add(enemy);
		    }
	    }
	    
	    protected override void SetMonsterPositions(List<Unit> monsterList)
	    {
		    if (monsterList == null)
		    {
			    return;
		    }
		    
		    var count = monsterList.Count;
		    string uidStr = string.Format("art_formation.base.{0}", count);
		    
		    //SetMonsterPositions(monsterList, uidStr);
	    }
	    
		public override ulong GetBossSceneTime()
		{
			return CorgiLogicConst.WorldBossSceneDelay;
		}
    }
}
