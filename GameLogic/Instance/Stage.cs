using System.Collections.Generic;
using System.Linq;
using Corgi.GameData;

using IdleCs.Utils;
using IdleCs.Managers;
using IdleCs.GameLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public abstract class Stage : CorgiObject
    {
	    /// <summary>
	    /// static data
	    /// </summary>
	    private Dungeon _dungeon;
	    
	    protected Dungeon Dungeon{ get{ return _dungeon; }}
	    
		static float[,] _summonPositions = new float[,] {{1f, 3f}, {-1f, 3f}, {3f, 1.5f}, {-3f, 1.5f}
			                                           , {1f, 5.5f}, {-1f, 5.5f}, {3f, 4f}, {-3f, 4f}};
	    
	    /// <summary>
	    /// dynamic data
	    /// </summary>
	    protected readonly List<Unit> _monsterList = new List<Unit>();

	    public List<Unit> MonsterList { get { return _monsterList; }}

	    private Monster[] _summonedMonsters = new Monster[8];

	    public Stage(Dungeon dungeon)
	    {
		    _dungeon = dungeon;
	    }

	    public virtual ulong GetBossSceneTime()
	    {
		    return 0;
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
		    SetMonsterPositions(MonsterList);
		    return true;
	    }

	    protected void AddMonster(Monster monster)
	    {
		    if (monster != null)
		    {
			    _monsterList.Add(monster);
		    }
	    }
	    
	    protected virtual void SetMonsterPositions(List<Unit> monsterList)
	    {
		    if (monsterList == null)
		    {
			    return;
		    }
		    
		    var count = monsterList.Count;
		    string uidStr = string.Format("art_formation.base.{0}", count);
		    
		    SetMonsterPositions(monsterList, uidStr);
	    }

	    protected void SetMonsterPositions(List<Unit> monsterList, string uidStr)
	    {
		    var count = monsterList.Count;
		    
		    var formationSheet = Dungeon.GameData.GetData<ArtFormationInfoSpec>(uidStr);
		    if (formationSheet == null)
		    {
			    throw new CorgiException("invalid formation info {0}", uidStr);
		    }

		    var jsonArray = JArray.Parse(formationSheet.MonsterPositions);
		    if (jsonArray.Count < count)
		    {
			    throw new CorgiException("invalid formation info count {0}", uidStr);
		    }

		    for (var i = 0; i < count; i++)
		    {
			    var posArray = jsonArray[i] as JArray;
			    if (posArray == null || posArray.Count != 2)
			    {
				    throw new CorgiException("invalid formation info position info{0}", uidStr);
			    }

			    var positions = CorgiJson.ParseArrayFloat(posArray);
			    if (positions.Count < 2)
			    {
				    throw new CorgiException("invalid formation info position info{0}", uidStr);
			    }

			    var monsterInst = monsterList[i];
			    var monsterPos = new CorgiPosition(positions[0], positions[1], 0, -1);
			    monsterInst.InitStartPosition(monsterPos );
		    }
	    }

	    public bool SummonMonster(Unit owner, List<ulong> monsterUids, CombatLogNode logNode)
	    {
		    if (monsterUids == null)
		    {
			    return false;
		    }

		    var summonLogNode = logNode as SummonLogNode;
		    if (summonLogNode == null)
		    {
			    return false;
		    }
		    
		    var ownerPosition = owner.Position;
		    
		    foreach (var monsterUid in monsterUids)
		    {
			    // set monster
			    var monster = new Monster(Dungeon);

			    if (monster.Load(monsterUid) == false)
			    {
				    continue;
			    }

			    monster.SetLevel(owner.Level);
			    monster.SetSummoner(owner);

			    var isAdded = false;
			    for (var i = 0; i < 8; i++)
			    {
				    var unit = _summonedMonsters[i];
					var posX = _summonPositions[i, 0];
					var posY = _summonPositions[i, 1];
					
				    if (unit == null || unit.IsLive() == false)
				    {
						var newPosition = new CorgiPosition(ownerPosition.X + posX, ownerPosition.Y + posY
							, ownerPosition.DirX, ownerPosition.DirY);
						
						monster.ResetPosition(newPosition, newPosition, newPosition);
						  
						AddMonster(monster);
						_summonedMonsters[i] = monster;
						summonLogNode.SummonedMonsters.Add(monster.ObjectId);
						  
						monster.OnEnterCombat(logNode);
						isAdded = true;
						
						break;
				    }
			    }

			    if (isAdded == false)
			    {
				    CorgiCombatLog.Log(CombatLogCategory.Skill, "Cant summon monster {0}", monster.Name);
			    }
		    }
		    
		    return true;
	    }
    }
}
