using System;
using Corgi.DBSchema;
using Corgi.GameData;
using Google.Protobuf;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using IdleCs.Utils;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic
{
    public class BindingStone : CorgiObject, ICorgiInterface<BindingStoneInfoSpec>
    {
        private BindingStoneInfoSpec _spec;
        private Unit _owner;

        public uint Level;
        public StatType StatType;

        // conditinos
        private DungeonType _dungeonType;
        private AreaType _areaType;
        private ulong _monsterGroupUid;
        private ulong _monsterUid;
        private ulong _passiveSkillUid;

        public BindingStoneInfoSpec GetSpec()
        {
            return _spec;
        }

        protected override bool LoadInternal(IMessage iMessage)
        {
            var dbObject = iMessage as DBBindingStone;
            if (dbObject == null)
            {
                return false;
            }

            if (!LoadInternal(dbObject.Uid))
            {
                return false;
            }

            Level = (uint)dbObject.Level;

            return base.LoadInternal(dbObject);
        }

        protected override bool LoadInternal(CorgiSharedObject sharedObject)
        {
            var sObject = sharedObject as SharedBindingStone;
            if (sObject == null)
            {
                return false;
            }

            if (!LoadInternal(sObject.uid))
            {
                return false;
            }

            Level = sObject.level;

            return base.LoadInternal(sObject);
        }

        protected override bool LoadInternal(ulong uid)
        {
            if (!base.LoadInternal(uid))
            {
                return false;
            }

            var spec = _owner.Dungeon.GameData.GetData<BindingStoneInfoSpec>(uid);
            if (spec == null)
            {
                return false;
            }

            _spec = spec;

            StatType = _spec.StatType;

            _passiveSkillUid = _spec.PassiveSkillUid;
            
            // parsing conditions
            var passiveConditions = _spec.PassiveConditions;
            try
            {
                if (string.IsNullOrEmpty(passiveConditions) == false)
                {
                     var conditionJson = JObject.Parse(passiveConditions);

                     var dungeonTypeStr = CorgiJson.ParseString(conditionJson, "dungeonType");
                     if (string.IsNullOrEmpty(dungeonTypeStr) == false)
                     {
                          _dungeonType = CorgiEnum.ParseEnum<DungeonType>(dungeonTypeStr);
                     }
                     var areaTypeStr = CorgiJson.ParseString(conditionJson, "areaType");
                     if (string.IsNullOrEmpty(areaTypeStr) == false)
                     {
                          _areaType = CorgiEnum.ParseEnum<AreaType>(areaTypeStr);
                     }
                     var groupUidStr = CorgiJson.ParseString(conditionJson, "groupUid");
                     if (string.IsNullOrEmpty(groupUidStr) == false)
                     {
                         _monsterGroupUid = GameDataManager.GetUidByString(groupUidStr);
                     }
                     var monsterUidStr = CorgiJson.ParseString(conditionJson, "monsterUid");
                     if (string.IsNullOrEmpty(monsterUidStr) == false)
                     {
                         _monsterUid = GameDataManager.GetUidByString(monsterUidStr);
                     }
                }

            }
            catch (Exception e)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Equip, e);
                CorgiCombatLog.LogError(CombatLogCategory.Equip,"Invalid BindingStone Condition Json : [{0}]", _spec.Uid);
                return true;
            }
            
            
            
            return true;
        }

        private void Init(Unit owner)
        {
            _owner = owner;
        }

        public static BindingStone Create(DBBindingStone dbBindingStone, Unit owner)
        {
            if (dbBindingStone == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Equip, "error in create BindingStone: dbBindingStone is null");
                return null;
            }

            var inst = new BindingStone();

            inst.Init(owner);

            if (!inst.Load(dbBindingStone))
            {
                CorgiCombatLog.LogError(CombatLogCategory.Equip, "failed to load dbBindingStone");
                return null;
            }

            return inst;
        }
        
        public static BindingStone Create(SharedBindingStone sBindingStone, Unit owner)
        {
            if (sBindingStone == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Equip, "error in create BindingStone: sBindingStone is null");
                return null;
            }

            var inst = new BindingStone();

            inst.Init(owner);

            if (!inst.Load(sBindingStone))
            {
                CorgiCombatLog.LogError(CombatLogCategory.Equip, "failed to load sBindingStone");
                return null;
            }

            return inst;
        }

        public long GetStat()
        {
            return _spec.StatValue + (Level == 0 ? 0 : Level - 1) * _spec.StatPerLevel;
        }

        public ulong GetPassiveUid(Dungeon dungeon)
        {
            var isFailed = false;
            
            if (_dungeonType != DungeonType.None)
            {
                if (_dungeonType == dungeon.DungeonType)
                {
                    return _passiveSkillUid;
                }

                isFailed = true;
            }
            if (_areaType != AreaType.AreaNone)
            {
                if (_areaType == dungeon.AreaType)
                {
                    return _passiveSkillUid;
                }

                isFailed = true;
            }
            
            var curStage = dungeon.CurStage;
            var bossMonster = curStage?.MonsterList[0] as Monster ;

            if (bossMonster != null)
            {
                if (_monsterGroupUid != 0)
                {
                    if (_monsterGroupUid == bossMonster.GroupUid)
                    {
                        return _passiveSkillUid;
                    }

                    isFailed = true;
                }
                if (_monsterUid != 0)
                {
                    if (_monsterUid == bossMonster.Uid)
                    {
                        return _passiveSkillUid;
                    }

                    isFailed = true;
                }
            }
            
            if (isFailed)
            {
                // failed all
                return 0;
            }

            return _passiveSkillUid;
        }
    }
}