using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Network;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class DungeonTest : DungeonInstance
    {
	    private bool _withParty;
	    private bool _allowExtermination = false;

	    public DungeonTest(ICombatBridge bridge, bool withParty)
			: base(bridge)
	    {
		    _withParty = withParty;
	    }

	    protected override bool LoadInternal(JObject json)
	    {
		    if (base.LoadInternal(json) == false)
		    {
			    return false;
		    }

		    if (CorgiJson.IsValidBool(json, "allowExtermination"))
		    {
				_allowExtermination = CorgiJson.ParseBool(json, "allowExtermination");
		    }
		    
            // load area type
            var spec = GetSpec();
            var areaSheet = GameData.GetData<ArtAreaInfoSpec>(spec.AreaUid);
            if (areaSheet == null)
            {
                AreaType = AreaType.AreaNone;
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Invalid Area Type : {0}\n", GameDataManager.GetStringByUid(spec.AreaUid));
            }
            else
            {
                AreaType = areaSheet.AreaType;
            }

		    return true;
	    }
	    
        public override Stage CreateNewStage(ulong nextStageUid)
        {
	        if (DungeonCriteriaType == DungeonCriteriaType.DctNone || DungeonCriteriaType == DungeonCriteriaType.DctChapter)
	        {
		        var newStage = new StageInstanceTest(this);
		        
				if (newStage.Load(nextStageUid) == false)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"failed Initializing stage instance.");
					return null;
				}
				CorgiCombatLog.LogError(CombatLogCategory.Dungeon,"Create Stage[{0}]", nextStageUid);

				return newStage;
	        }

	        return base.CreateNewStage(nextStageUid);
        }

        public override List<Unit> FillNpc(List<Unit> unitList)
        {
	        if (_withParty)
	        {
		        return base.FillNpc(unitList);
	        }
	        return unitList;
        }
        

        protected override void OnFinishStage(CombatLogNode logNode)
        {
            base.OnFinishStage(logNode);
			_state.Set(DungeonState.Destroy);
        }

        protected override Deck GetCurrentDeck(Unit unit)
        {
			return Bridge.GetCoPartyDeck(unit);
			
        }
        
	    public override bool GetAllowExtermination()
	    {
		    return _allowExtermination;
	    }
    }
}
