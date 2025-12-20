using Corgi.GameData;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class StageInstanceRift : Stage, ICorgiInterface<InstanceRiftInfoSpec>
    {
	    private InstanceRiftInfoSpec _spec;
	    
	    private long _initMaxHP;
	    private long _initCurHP;
	    
	    public InstanceRiftInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    public StageInstanceRift(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
	    protected override bool LoadInternal(ulong uid)
	    {
		    var sheetData = Dungeon.GameData.GetData<InstanceRiftInfoSpec>(uid);
		    var dungeon = Dungeon as DungeonRift;

		    if (sheetData == null || dungeon == null)
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
			    var monster = new MonsterRift(Dungeon);
			    
			    monster.InitBossHP(_initMaxHP, _initCurHP);
			    
			    if (monster.Load(monUid) == false)
			    {
				    return false;
			    }

			    monster.SetLevel(dungeon.MonsterLevel);
			    
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
	        _initMaxHP = maxHP;
	        _initCurHP = curHP;
        }
	    
		public override ulong GetBossSceneTime()
		{
			return CorgiLogicConst.WorldBossSceneDelay;
		}
    }
}
