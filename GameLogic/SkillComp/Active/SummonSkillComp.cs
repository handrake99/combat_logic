using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class SummonSkillComp : ActiveSkillComp
    {
	    private List<ulong> _monsterUids = new List<ulong>();
		
		public SummonSkillComp()
		{
		}
		
        protected override bool LoadInternal(ulong uid)
        {
            var spec = Owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);
            if (spec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DamageAtkFactorSkillComp uid : {0}", uid);
                return false;
            }

	        if (string.IsNullOrEmpty(spec.Params) == false)
	        {
				try
				{
					var parameter = JArray.Parse(spec.Params);

					foreach (var curValue in parameter)
					{
						var monUidStr = (string)curValue;

						if (string.IsNullOrEmpty(monUidStr))
						{
							continue;
						}

						var monUid = GameDataManager.GetUidByString(monUidStr);
						_monsterUids.Add(monUid);
					}
				}
				catch (Exception e)
				{
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
					return false;
				}
	        }

            return base.LoadInternal(uid);
        }
		
		public override SkillCompLogNode CreateLogNode(Unit target)
		{
            var curLog = new SummonLogNode(this.Owner, target, this);
            
	        return curLog;
		}

        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
	        var thisLogNode = logNode as SummonLogNode;
	        if (thisLogNode == null )
	        {
		        return false;
	        }

	        if (target.CombatSideType == CombatSideType.Player)
	        {
		        // monster만 소환 가능
		        return false;
	        }

	        // check self
	        if (target.ObjectId != Owner.ObjectId)
	        {
		        return false;
	        }

	        var retResult = false;

			if (Owner.Dungeon.SummonMonster(Owner, _monsterUids, logNode) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"Summon Skill Comp Failed");
			}

	        return true;

        }
        
    }
}
