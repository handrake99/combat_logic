
using System;
using System.Collections.Generic;

using IdleCs.Utils;
using IdleCs.GameLog;
using IdleCs.Library;

namespace IdleCs.GameLogic
{
    /// <summary>
    /// corgi object 의 event manager
    /// </summary>
    [System.Serializable]
    public class EventManager
    {
        public delegate bool OnEventDelegate(EventParam eventParam, CombatLogNode logNode);

        private readonly CombatEventCategory _targetType;

        private Dictionary<CombatEventType, List<OnEventDelegate>> _eventMap;

        public EventManager(CombatEventCategory type)
        {
            _targetType = type;
        }

        public void Register(CombatEventType eventType, OnEventDelegate callback)
        {
            var categoryType = GetCombatEventCategory(eventType);

            if (CheckTarget(categoryType) == false)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Unit,"Invalid Register Event : <0>", eventType.ToString());
                return;
            }
            
            if (_eventMap == null)
            {
                _eventMap = new Dictionary<CombatEventType, List<OnEventDelegate>>();
            }

            if (_eventMap.ContainsKey(eventType) == false)
            {
                _eventMap[eventType] = new List<OnEventDelegate>();
            }

            _eventMap[eventType].Add(callback);
            
        }
        
        // public void Register(CorgiObject childObject)
        // {
        //     if (childObject == null)
        //     {
        //         return;
        //     }
        //     
        //     if (_eventChilds.Contains(childObject) == false)
        //     {
        //         _eventChilds.Add(childObject);
        //     }
        // }
        
        public static CombatEventCategory GetCombatEventCategory(CombatEventType eventType)
        {
            var enumValue = (int) eventType;

            if (enumValue > 0 && enumValue < (int) CombatEventType.EventUnitStart)
            {
                return CombatEventCategory.Rule;
            }else if (enumValue > (int) CombatEventType.EventUnitStart
                      && enumValue < (int) CombatEventType.EventActionStart)
            {
                return CombatEventCategory.Unit;
                
            }else if (enumValue > (int) CombatEventType.EventActionStart
                      && enumValue < (int) CombatEventType.EventEffectStart)
            {
                return CombatEventCategory.Action;
                
            }else if (enumValue > (int) CombatEventType.EventEffectStart)
            {
                return CombatEventCategory.Effect;
            }

            return CombatEventCategory.None;
        }

        private bool CheckTarget(CombatEventCategory eventCategory)
        {
            return true;
//            if (eventCategory == CombatEventCategory.Rule
//                && (_targetType == CombatEventCategory.Unit 
//                    || _targetType == CombatEventCategory.Rule))
//            {
//                return true;
//                
//            }else if (eventCategory == CombatEventCategory.Unit
//                && (_targetType == CombatEventCategory.Unit 
//                    || _targetType == CombatEventCategory.Action
//                    || _targetType == CombatEventCategory.Effect))
//            {
//                return true;
//            }else if (eventCategory == CombatEventCategory.Action
//                && (_targetType == CombatEventCategory.Unit 
//                    || _targetType == CombatEventCategory.Action
//                    || _targetType == CombatEventCategory.Effect))
//            {
//                return true;
//                
//            }else if (eventCategory == CombatEventCategory.Effect
//                  && (_targetType == CombatEventCategory.Effect))
//            {
//                return true;
//            }
//
//            return false;
        }

        public bool OnEvent(CombatEventType eventType, EventParam eventParam, CombatLogNode logNode)
        {
            if (CheckTarget(eventParam.EventCategory) == false)
            {
                return false;
            }
            
            var ret = false;
            

            if (_eventMap != null && _eventMap.ContainsKey(eventType)) 
            {
                var delegateList = _eventMap[eventType];

                foreach(var curDelegate in delegateList)
                {
                    if (curDelegate(eventParam, logNode) == true)
                    {
                        ret = true;
                    }
                }
            }

            return ret; 
            
        }

        public void OnDestroy()
        {
            if (_eventMap != null)
            {
                _eventMap.Clear();
            }
        }
    }
}
