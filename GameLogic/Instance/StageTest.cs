using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Utils;
using IdleCs.Managers;
using IdleCs.GameLog;

namespace IdleCs.GameLogic
{
	public class StageTest : StageInstanceChapter
	{
		public StageTest(Dungeon dungeon)
			: base(dungeon)
		{
		}

		// protected override bool LoadMonsters()
		// {
		// 	var sheetData = GetSpec();
		// 	var index = 0;
		// 	foreach (var monUid in sheetData.BossUids)
		// 	{
		// 		// set monster
		// 		var monster = new MonsterTest(Dungeon);
		//
		// 		if (monster.Load(monUid) == false)
		// 		{
		// 			return false;
		// 		}
		//
		// 		monster.SetLevel(sheetData.BossLevels[index]);
		// 		//monster.SetModelTest(index);
		//
		// 		AddMonster(monster);
		//
		// 		index++;
		// 	}
		//
		// 	foreach (var curUnit in MonsterList)
		// 	{
		// 		if (curUnit != null)
		// 		{
		// 			CorgiLog.LogLine("Monster : {0} ({1})", curUnit.Name, curUnit.ObjectId);
		// 		}
		// 	}
		//
		// 	return true;
		//
		// }

	}
}
