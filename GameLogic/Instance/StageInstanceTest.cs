using Corgi.GameData;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class StageInstanceTest : StageInstanceChapter, ICorgiInterface<InstanceChapterInfoSpec>
    {
	    public StageInstanceTest(Dungeon dungeon)
			:base(dungeon)
	    {
	    }
	    
		public override ulong GetBossSceneTime()
		{
			return 0;
		}
    }
}
