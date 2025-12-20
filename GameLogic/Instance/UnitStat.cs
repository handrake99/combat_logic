using System;
using System.Collections;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.GameLog;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    /// <summary>
    /// unit stat 관리.
    /// </summary>
    public class UnitStat
    {
        private StatType _statType;
        private long _origStat;       // unit original stat
        private long _addStat;        // additional stat
        
        private long _finalStat;      // adjust something(equip or buff/debuff)

        public StatType StatType
        {
            get { return _statType; }
        }
        
        public long OrigStat
        {
            get { return _origStat; }
            set
            {
                _origStat = value;
                _finalStat = BaseStat;
            }
        }

        public long AddStat
        {
            get => _addStat;
            set
            {
                _addStat = value;
                _finalStat = BaseStat;
            }
        }

        private long BaseStat
        {
            get { return _origStat + _addStat; }
        }

        public long Stat
        {
            get { return _finalStat; }
        }

        public UnitStat(StatType statType)
        {
            _statType = statType;
        }

        public long GetStat()
        {
            return _finalStat;
        }

        public long GetBaseStat()
        {
            return BaseStat;
        }

        public void OnUpdateStat(List<PassiveSkillComp> passiveList, CombatLogNode logNode)
        {
            var preStat = BaseStat;
            
            var abs = 0L;
            var percent = 1.0f;
            
            bool isUpdated = false;
            
            // adjust passive 
            foreach (var passiveComp in passiveList)
            {
                var statPassive = passiveComp as StatPassiveComp;
                if (statPassive == null)
                {
                    continue;
                }
                if (_statType == statPassive.StatType)
                {
                    isUpdated = true;
                    var count = passiveComp.StackCount;
                    
                    for (var i = 0; i < count; i++)
                    {
                        abs += statPassive.GetBaseAbsolute(logNode);
                        percent += statPassive.GetBasePercent(logNode);
                    }
                }
            }

            if (isUpdated)
            {
                _finalStat = (long)(abs + preStat * percent);
            }
            else
            {
                _finalStat = preStat;
            }

            // if (_statType == StatType.StAttackPower)
            // {
            //     CorgiLog.LogWarning("[Stat] AttackPower Updated {0}", _finalStat);
            // }

            if (_finalStat < 0)
            {
                _finalStat = 0;
            }

            if (_finalStat > GameLogicSetting.GameLogicMaxValue)
            {
                _finalStat = GameLogicSetting.GameLogicMaxValue;
            }
        }
    }

    public static class UnitStatHelper
    {
        private static float _hitConst = 0.000033f;
        
        public static float GetHitProb(uint casterLevel, long hitRate)
        {
            if (casterLevel == 0)
            {
                return 0f;
            }
            var hitProb = (float)((_hitConst * hitRate) / (Math.Pow(casterLevel, 1.05)));

            return hitProb + 0.95f;
        }
        public static float GetEvasionProb(uint targetLevel, long evasionRate)
        {
            if (targetLevel == 0)
            {
                return 0.05f;
            }
            var evasionProb = (float)((_hitConst * evasionRate) / (Math.Pow(targetLevel, 1.05)));
            
            return evasionProb + 0.05f;
        }
        
        private static float _critConst = 0.00027f;
        
        public static float GetCritProb(uint casterLevel, long critRate)
        {
            if (casterLevel == 0)
            {
                return 0f;
            }
            var critProb = (float)((_critConst * critRate) / (Math.Pow(casterLevel, 1.05)));

            return critProb + 0.05f;
        }
        
        public static float GetResilienceProb(uint targetLevel, long resilienceRate)
        {
            if (targetLevel == 0)
            {
                return 0f;
            }
            var resilienceProb = (float)((_critConst * resilienceRate) / (Math.Pow(targetLevel, 1.05)));
            
            return resilienceProb;
        }
        
        private static float _critDmgConst = 0.00027f;
        public static float GetCritDmgPercent(uint casterLevel, long critDmgStat)
        {
            var critDmgPercent = (float)((_critDmgConst * critDmgStat) / (Math.Pow(casterLevel, 1.05)));

            return critDmgPercent + 0.5f;
        }

        public static string GetFormattedProb(float prob)
        {
            var newProb = Math.Round(prob, 1);
            var probStr = string.Format("{0:0.#}", newProb);
            
            return probStr;
        }
        
    }
}

