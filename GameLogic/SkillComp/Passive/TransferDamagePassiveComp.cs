using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    [System.Serializable]
	public class TransferDamagePassiveComp : PassiveSkillComp
    {
	    private long _transferValue;
	    private float _transferRate;
	    private float _reduceRate;

	    private SkillCompInfo _skillCompInfo;

		public TransferDamagePassiveComp()
        {
			EventManager.Register(CombatEventType.OnBeingPreHit, OnBeingHit);
        }
		
        protected override bool LoadInternal(ulong uid)
		{
	        if (base.LoadInternal(uid) == false)
	        {
		        return false;
	        }

	        var spec = GetSpec();
	        
	        try
	        {
		        var parameter = JObject.Parse(spec.Params);
		        var targetStr = CorgiJson.ParseString(parameter, "target");
		        var skillCompUidStr = CorgiJson.ParseString(parameter, "skillCompUid");
		        var skillCompUid = GameDataManager.GetUidByString(skillCompUidStr);
			        
		        _skillCompInfo = SkillCompInfo.CreateOnActive(Owner, ParentActor, targetStr, skillCompUid, 0L);
		        if (_skillCompInfo == null)
		        {
			        return false;
		        }

		        _reduceRate = CorgiJson.ParseFloat(parameter, "reduceRate");
		        if (_reduceRate < 0)
		        {
			        _reduceRate = 0;
		        }
		        _transferValue = BaseAbsolute;
		        _transferRate = BasePercent;
	        }
	        catch (Exception e)
	        {
		        CorgiCombatLog.LogFatal(CombatLogCategory.Skill,"invalid parameter {0}", spec.Name);
		        return false;
	        }

	        return true;
		}
	    
		bool OnBeingHit(EventParam eventParam, CombatLogNode logNode)
		{
			var dmgLogNode = logNode.Parent as DamageSkillCompLogNode;

			if (dmgLogNode == null || _skillCompInfo == null)
			{
				return false;
			}
			
			var passiveNode = new TransferDamagePassiveSkillCompLogNode(this.Owner, Owner, this);
			
			var reduceDamage = (long)dmgLogNode.TransferOutput(_transferValue , _transferRate, _reduceRate);

			passiveNode.TotalDamage = (int)dmgLogNode.Damage;
			passiveNode.TransferDamge = reduceDamage;

			if (_skillCompInfo.Invoke(passiveNode) == false)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill,"can't transfer damage.");
				return false;
			}

			logNode.AddChild(passiveNode);

			return true;
			
		}
	}
}
