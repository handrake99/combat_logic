using System;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Managers;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class DamageTransferedDOTSkillComp : DamageSkillComp
    {
        private int _totalDamage;
        
        public DamageTransferedDOTSkillComp()
        {
        }
        
        protected override bool LoadInternal(ulong uid)
        {
            var parentSkillInst = ParentActor as SkillEffectInst;
            // should involed by skill continuous
            if (parentSkillInst == null)
            {
                return false;
            }

            if (base.LoadInternal(uid) == false)
            {
                return false;
            }
            
            var spec = Owner.Dungeon.GameData.GetData<SkillActiveInfoSpec>(uid);
            if (spec == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"Invalid DamageAtkFactorSkillComp uid : {0}", uid);
                return false;
            }

            // Damage = spec.BaseAmount;
            // AtkFactor = spec.ApFactor;
            var atkFactor = spec.ApFactor;
            
            var ownerLevel = Owner.Level;
            var dmgLvMod = 5 * ownerLevel * ownerLevel + 500;
            var target = Owner;
            
            // 기본 피해량
            var baseDamage = Owner.AttackPower * dmgLvMod / (dmgLvMod + target.Defence);
            // 계수 피해량
            _totalDamage = (int)(baseDamage * atkFactor + BaseAmount);
            
            ForceHit = true;
            IgnoreDefence = true;
            IgnoreCritical = true;
            IgnoreEvent = true;
            
            parentSkillInst.RegisterEvent(CombatEventType.OnSkillEffectEnter, OnUpdated);

            return true;
        }
        
        public override SkillCompLogNode CreateLogNode(Unit target)
        {
            var curLog = new DamageSkillCompLogNode(Owner, target, this);

            curLog.ForceHit = ForceHit;
	        curLog.IgnoreDefence = IgnoreDefence;
	        curLog.IgnoreCritical = IgnoreCritical;
	        curLog.IgnoreEvent = IgnoreEvent;
            
            return curLog;
        }
        
        // TransferDamage Passive 로부터 올라온것만 처리된다.
        protected override bool DoApplyInner(Unit target, CombatLogNode logNode)
        {
            var dmgLogNode = logNode as DamageSkillCompLogNode;
            var skillInst = ParentActor as SkillEffectInst;
            if (dmgLogNode == null || skillInst == null)
            {
                return false;
            }

            var remainCount = 1UL;
            if (skillInst.TickDuration > 0)
            {
                remainCount = skillInst.CurDuration / skillInst.TickDuration;
            }

            if (_totalDamage == 0 || remainCount == 0)
            {
                return false;
            }
            
            var curDamage = _totalDamage / (int)remainCount;

            dmgLogNode.Damage = curDamage;

            if (DoApplyDamage(dmgLogNode) == false)
            {
                return false;
            }

            _totalDamage -= curDamage;
	        
            return true;
        }

        bool OnUpdated(EventParam eventParam, CombatLogNode logNode)
        {
			var curLogNode = logNode.Parent as TransferDamagePassiveSkillCompLogNode;
			if (curLogNode == null)
			{
				return false;
			}

			var addedDamage = (int)(curLogNode.TransferDamge / curLogNode.TargetCount);

			_totalDamage += addedDamage;

			return true;
            
        }
    }
}
