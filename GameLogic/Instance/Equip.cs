using System;
using System.Collections.Generic;
using Corgi.DBSchema;
using Corgi.GameData;
using Google.Protobuf;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public struct StatStruct
    {
        public ulong Uid;
        public StatType StatType;
        public uint StatValue;
        public uint StatPerLevel;

        public long StatFinal;

        public void Init(ulong uid, StatType statType, uint statValue, uint statPerLevel)
        {
            Uid = uid;
            StatType = statType;
            StatValue = statValue;
            StatPerLevel = statPerLevel;
            
        }

        public void Calc(GameDataManager gameData, ClassType classType, uint level)
        {
            StatFinal = Equip.CalcStatValue(gameData, this, classType, level);
        }
        
        public void Calc2(GameDataManager gameData, ClassType classType, uint level)
        {
            //StatFinal = StatValue + StatPerLevel * (level-1);
            StatFinal = Equip.CalcStatValue2(gameData, this, classType, level);
        }

        public bool IsSame(StatStruct statStruct)
        {
            if (StatType == statStruct.StatType && StatFinal == statStruct.StatFinal &&
                StatPerLevel == statStruct.StatPerLevel)
            {
                return true;
            }

            return false;
        }
    }

    public class EquipInfo
    {
        // Equip Data
        public StatStruct[] MainStats = new StatStruct[2];
        public StatStruct[] SubStats = new StatStruct[4];

        public ulong MainSkillUid;
        public ulong SubSkillUid;

        ClassType _classType;

        public EquipInfo(ClassType classType)
        {
            _classType = classType;
        }

        public void Calc(GameDataManager gameData, uint level)
        {
            for (var i = 0; i < MainStats.Length; i++)
            {
                MainStats[i].Calc(gameData, _classType, level);
            }

            for (var i = 0; i < SubStats.Length; i++)
            {
                SubStats[i].Calc(gameData, _classType, level);
            }
        }
        
        public void Calc2(GameDataManager gameData, uint level)
        {
            for (var i = 0; i < MainStats.Length; i++)
            {
                MainStats[i].Calc2(gameData, _classType, level);
            }

            for (var i = 0; i < SubStats.Length; i++)
            {
                SubStats[i].Calc2(gameData, _classType, level);
            }
        }

        public bool IsSame(EquipInfo equipInfo)
        {
            for (var i = 0; i < MainStats.Length; i++)
            {
                if (MainStats[i].IsSame(equipInfo.MainStats[i]) == false)
                {
                    return false;
                }
            }

            for (var i = 0; i < SubStats.Length; i++)
            {
                if (SubStats[i].IsSame(equipInfo.SubStats[i]) == false)
                {
                    return false;
                }
            }

            return true;

        }
    }
    
    public class Equip : CorgiObject, ICorgiInterface<EquipSpec>
    {
        private Unit _owner;

        private EquipSpec _spec;

        public EquipSpec GetSpec()
        {
            return _spec;
        }
        
        // static data
        public uint Level { get; protected set; }
        public uint _maxLevel;
        public uint Grade { get; protected set; }

        public bool IsMaxLevel
        {
            get { return Level == _maxLevel; }
        }

        private EquipInfo _equipInfo;
        
        // Equip Data
        public StatStruct[] MainStats
        {
            get { return _equipInfo?.MainStats; }
        }
        public StatStruct[] SubStats
        {
            get { return _equipInfo?.SubStats; }
        }
        public ulong MainSkillUid
        {
            get
            {
                if (_equipInfo == null)
                {
                    return 0;
                }
                return _equipInfo.MainSkillUid;
            }
        }
        public ulong SubSkillUid
        {
            get
            {
                if (_equipInfo == null)
                {
                    return 0;
                }
                return _equipInfo.SubSkillUid;
            }
        }
        
        public Equip(Unit owner)
        {
            _owner = owner;
        }

        protected override bool LoadInternal(IMessage dbObject)
        {
            var db = dbObject as DBEquip;
            if (db == null)
            {
                return false;
            }

            DBId = db.Dbid;
            Uid = db.Uid;
            if (LoadInternal(Uid) == false)
            {
                return false;
            }

            Level = db.Level;
        
            var equipInfo = LoadSouls(_owner.Dungeon.GameData, db);

            if (equipInfo == null)
            {
                return false;
            }

            _equipInfo = equipInfo;
            
            return base.LoadInternal(dbObject);
        }
        
        protected override bool LoadInternal(CorgiSharedObject sObject)
        {
            var sharedObject = sObject as SharedEquipInfo;
            if (sharedObject == null)
            {
                return false;
            }

            DBId = sharedObject.dbId;
            Uid = sharedObject.uid;
            if (LoadInternal(Uid) == false)
            {
                return false;
            }

            Level = sharedObject.level;

            var equipInfo = LoadSouls(_owner.Dungeon.GameData, sharedObject);
            
            if(equipInfo == null)
            {
                return false;
            }

            _equipInfo = equipInfo;
            
            return base.LoadInternal(sObject);
        }

        protected override bool LoadInternal(ulong uid)
        {
            if (base.LoadInternal(uid) == false)
            {
                return false;
            }

            var spec = _owner.Dungeon.GameData.GetData<EquipSpec>(uid);
            if (spec == null)
            {
                return false;
            }

            _spec = spec;
            
            Grade = (uint)_spec.EquipGrade;
            
            var gradeStr = string.Format("equip_grade.{0}", Grade);
            var gradeInfo = GameDataManager.Instance.GetData<EquipGradeSpec>(gradeStr);

            if (gradeInfo == null)
            {
                return false;
            }

            _maxLevel = gradeInfo.MaxLevel;

            return true;
        }
        

        public static EquipInfo LoadSouls(GameDataManager gamedata, DBEquip dbEquip)
        {
            if (gamedata == null || dbEquip == null)
            {
                return null;
            }

            var spec = gamedata.GetData<EquipSpec>(dbEquip.Uid);
            if (spec == null)
            {
                return null;
            }

            var soulUids = new List<ulong>();

            soulUids.Add(spec.SoulUid);
            soulUids.Add(dbEquip.SoulUid1);
            soulUids.Add(dbEquip.SoulUid2);
            soulUids.Add(dbEquip.SoulUid3);
            soulUids.Add(dbEquip.SoulUid4);

            //var calcEquipInfo = LoadSoulsInternal(gamedata, spec, soulUids, dbEquip.Level);

            if (spec.EquipSoulType == EquipSoulType.General || spec.EquipSoulType == EquipSoulType.Soul)
            {
                return LoadSoulsInternalNew(gamedata, dbEquip, soulUids);
            } else if (spec.EquipSoulType == EquipSoulType.InGameSoul)
            {
                return LoadSoulsInternal(gamedata, spec, soulUids, dbEquip.Level);
            }

            return null;
        }

        public static EquipInfo LoadSouls(GameDataManager gamedata, ulong equipUid, uint level)
        {
            var spec = gamedata.GetData<EquipSpec>(equipUid);
            if (spec == null)
            {
                return null;
            }
            
            var soulUids = new List<ulong>();

            soulUids.Add(spec.SoulUid);
            
            return LoadSoulsInternal(gamedata, spec, soulUids, level);
        }
        
        public static EquipInfo LoadSouls(GameDataManager gamedata, SharedEquipInfo sharedEquip)
        {
            if (gamedata == null || sharedEquip == null)
            {
                return null;
            }

            var spec = gamedata.GetData<EquipSpec>(sharedEquip.uid);
            if (spec == null)
            {
                return null;
            }

            var soulUids = new List<ulong>();

            soulUids.Add(spec.SoulUid);
            soulUids.Add(sharedEquip.soulUid1);
            soulUids.Add(sharedEquip.soulUid2);
            soulUids.Add(sharedEquip.soulUid3);
            soulUids.Add(sharedEquip.soulUid4);
            
            if (spec.EquipSoulType == EquipSoulType.General || spec.EquipSoulType == EquipSoulType.Soul)
            {
                return LoadSoulsInternalNew(gamedata, sharedEquip, soulUids);
            } else if (spec.EquipSoulType == EquipSoulType.InGameSoul)
            {
                return LoadSoulsInternal(gamedata, spec, soulUids, sharedEquip.level);
            }

            return null;
        }
        
        // Load EquipInfo from Spec
        private static EquipInfo LoadSoulsInternal(GameDataManager gamedata, EquipSpec spec, List<ulong> soulUids, uint level)
        {
            var equipType = spec.Type;
            var equipSoulType = spec.EquipSoulType;
            
            if (equipSoulType == EquipSoulType.None)
            {
                // do nothing
                return null;
            }

            var equipInfo = new EquipInfo(spec.ClassType);
            if (equipSoulType == EquipSoulType.General)
            {
                for(var i=0; i<soulUids.Count; i++)
                {
                    var soulUid = soulUids[i];
                    var soulSpec = GameDataManager.Instance.GetData<EquipMonsterSoulSpec>(soulUid);

                    if (soulSpec == null)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Equip, "Invalid Monster Soul Uid {0}", soulUid);
                        continue;
                    }

                    if (i == 0)
                    {
                        // main soul
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.WeaponMainStat1Type,
                                    soulSpec.WeaponMainStat1Value, soulSpec.WeaponMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.WeaponMainStat2Type,
                                    soulSpec.WeaponMainStat2Value, soulSpec.WeaponMainStat2PerLevel);
                                break;
                            case EquipType.EtHelmet:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.HelmetMainStat1Type,
                                    soulSpec.HelmetMainStat1Value, soulSpec.HelmetMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.HelmetMainStat2Type,
                                    soulSpec.HelmetMainStat2Value, soulSpec.HelmetMainStat2PerLevel);
                                break;
                            case EquipType.EtArmor:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.ArmorMainStat1Type,
                                    soulSpec.ArmorMainStat1Value, soulSpec.ArmorMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.ArmorMainStat2Type,
                                    soulSpec.ArmorMainStat2Value, soulSpec.ArmorMainStat2PerLevel);
                                break;
                            case EquipType.EtGauntlet:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.GauntletMainStat1Type,
                                    soulSpec.GauntletMainStat1Value, soulSpec.GauntletMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.GauntletMainStat2Type,
                                    soulSpec.GauntletMainStat2Value, soulSpec.GauntletMainStat2PerLevel);
                                break;
                            case EquipType.EtBoots:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.BootsMainStat1Type,
                                    soulSpec.BootsMainStat1Value, soulSpec.BootsMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.BootsMainStat2Type,
                                    soulSpec.BootsMainStat2Value, soulSpec.BootsMainStat2PerLevel);
                                break;
                            case EquipType.EtCloak:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.CloakMainStat1Type,
                                    soulSpec.CloakMainStat1Value, soulSpec.CloakMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.CloakMainStat2Type,
                                    soulSpec.CloakMainStat2Value, soulSpec.CloakMainStat2PerLevel);
                                break;
                        }
                    }
                    else
                    {   // sub soul
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.WeaponSubStatType, soulSpec.WeaponSubStatValue, soulSpec.WeaponSubStatPerLevel);
                                break;
                            case EquipType.EtHelmet:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.HelmetSubStatType, soulSpec.HelmetSubStatValue, soulSpec.HelmetSubStatPerLevel);
                                break;
                            case EquipType.EtArmor:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.ArmorSubStatType, soulSpec.ArmorSubStatValue, soulSpec.ArmorSubStatPerLevel);
                                break;
                            case EquipType.EtGauntlet:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.GauntletSubStatType, soulSpec.GauntletSubStatValue, soulSpec.GauntletSubStatPerLevel);
                                break;
                            case EquipType.EtBoots:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.BootsSubStatType, soulSpec.BootsSubStatValue, soulSpec.BootsSubStatPerLevel);
                                break;
                            case EquipType.EtCloak:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.CloakSubStatType, soulSpec.CloakSubStatValue, soulSpec.CloakSubStatPerLevel);
                                break;
                        }
                    }
                }
            }
            else if (equipSoulType == EquipSoulType.Soul)
            {
                for(var i=0; i<soulUids.Count; i++)
                {
                    var soulUid = soulUids[i];
                    var soulSpec = GameDataManager.Instance.GetData<EquipDragonSoulSpec>(soulUid);

                    if (soulSpec == null)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Equip, "[Equip] Invalid Dragon Soul Uid {0}", soulUid);
                        continue;
                    }
                    
                    if (i == 0)
                    {   // main soul
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.WeaponMainStat1Type,
                                    soulSpec.WeaponMainStat1Value, soulSpec.WeaponMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.WeaponMainStat2Type,
                                    soulSpec.WeaponMainStat2Value, soulSpec.WeaponMainStat2PerLevel);
                                break;
                            case EquipType.EtHelmet:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.HelmetMainStat1Type,
                                    soulSpec.HelmetMainStat1Value, soulSpec.HelmetMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.HelmetMainStat1Type,
                                    soulSpec.HelmetMainStat2Value, soulSpec.HelmetMainStat2PerLevel);
                                break;
                            case EquipType.EtArmor:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.ArmorMainStat1Type,
                                    soulSpec.ArmorMainStat1Value, soulSpec.ArmorMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.ArmorMainStat2Type,
                                    soulSpec.ArmorMainStat2Value, soulSpec.ArmorMainStat2PerLevel);
                                break;
                            case EquipType.EtGauntlet:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.GauntletMainStat1Type,
                                    soulSpec.GauntletMainStat1Value, soulSpec.GauntletMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.GauntletMainStat2Type,
                                    soulSpec.GauntletMainStat2Value, soulSpec.GauntletMainStat2PerLevel);
                                break;
                            case EquipType.EtBoots:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.BootsMainStat1Type,
                                    soulSpec.BootsMainStat1Value, soulSpec.BootsMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.BootsMainStat2Type,
                                    soulSpec.BootsMainStat2Value, soulSpec.BootsMainStat2PerLevel);
                                break;
                            case EquipType.EtCloak:
                                equipInfo.MainStats[0].Init(soulSpec.Uid, soulSpec.CloakMainStat1Type,
                                    soulSpec.CloakMainStat1Value, soulSpec.CloakMainStat1PerLevel);
                                equipInfo.MainStats[1].Init(soulSpec.Uid, soulSpec.CloakMainStat2Type,
                                    soulSpec.CloakMainStat2Value, soulSpec.CloakMainStat2PerLevel);
                                break;
                        }
                        equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                        equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                    }
                    else
                    {   // sub soul
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.WeaponSubStatType, soulSpec.WeaponSubStatValue, soulSpec.WeaponSubStatPerLevel);
                                break;
                            case EquipType.EtHelmet:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.HelmetSubStatType, soulSpec.HelmetSubStatValue, soulSpec.HelmetSubStatPerLevel);
                                break;
                            case EquipType.EtArmor:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.ArmorSubStatType, soulSpec.ArmorSubStatValue, soulSpec.ArmorSubStatPerLevel);
                                break;
                            case EquipType.EtGauntlet:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.GauntletSubStatType, soulSpec.GauntletSubStatValue, soulSpec.GauntletSubStatPerLevel);
                                break;
                            case EquipType.EtBoots:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.BootsSubStatType, soulSpec.BootsSubStatValue, soulSpec.BootsSubStatPerLevel);
                                break;
                            case EquipType.EtCloak:
                                equipInfo.SubStats[i-1].Init(soulSpec.Uid, soulSpec.CloakSubStatType, soulSpec.CloakSubStatValue, soulSpec.CloakSubStatPerLevel);
                                break;
                        }
                    }
                }
            }
            else if (equipSoulType == EquipSoulType.InGameSoul)
            {
                for(var i=0; i<soulUids.Count; i++)
                {
                    var soulUid = soulUids[i];
                    var soulSpec = GameDataManager.Instance.GetData<EquipDragonSoulSpec>(soulUid);

                    if (soulSpec == null)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Equip, "[Equip] Invalid Dragon Soul Uid {0}", soulUid);
                        continue;
                    }
                    var baseSoulUid = soulSpec.BaseUid;
                    var baseSoulSpec= GameDataManager.Instance.GetData<EquipDragonSoulSpec>(baseSoulUid);

                    if (baseSoulSpec == null)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Equip, "[Equip] Invalid Dragon Soul Uid {0}", baseSoulUid);
                        continue;
                    }

                    var valueFactor = 1f;
                    var modFactor = 1f;

                    if (spec.EquipGrade == EquipGradeType.EgtLegendaryPlus)
                    {
                        valueFactor = 1f + GameDataManager.Instance.GetConfigFloat("config.equip.synthesis.valueIncreasePlus", 0f);
                        modFactor = 1f + GameDataManager.Instance.GetConfigFloat("config.equip.synthesis.modIncreasePlus", 0f);
                    }
                    else
                    {
                        valueFactor = 1f + GameDataManager.Instance.GetConfigFloat("config.equip.synthesis.valueIncrease", 0f);
                        modFactor = 1f + GameDataManager.Instance.GetConfigFloat("config.equip.synthesis.modIncrease", 0f);
                    }
                    
                    if (i == 0)
                    {   // main soul
                        StatType mainStat1Type = StatType.StNone;
                        uint mainStat1 = 0;
                        uint mainStat1PerLevel = 0;
                        StatType mainStat2Type = StatType.StNone;
                        uint mainStat2 = 0;
                        uint mainStat2PerLevel = 0;
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                mainStat1Type = baseSoulSpec.WeaponMainStat1Type;
                                mainStat1 = baseSoulSpec.WeaponMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.WeaponMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.WeaponMainStat2Type;
                                mainStat2 = baseSoulSpec.WeaponMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.WeaponMainStat2PerLevel;
                                break;
                            case EquipType.EtHelmet:
                                mainStat1Type = baseSoulSpec.HelmetMainStat1Type;
                                mainStat1 = baseSoulSpec.HelmetMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.HelmetMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.HelmetMainStat2Type;
                                mainStat2 = baseSoulSpec.HelmetMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.HelmetMainStat2PerLevel;
                                break;
                            case EquipType.EtArmor:
                                mainStat1Type = baseSoulSpec.ArmorMainStat1Type;
                                mainStat1 = baseSoulSpec.ArmorMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.ArmorMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.ArmorMainStat2Type;
                                mainStat2 = baseSoulSpec.ArmorMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.ArmorMainStat2PerLevel;
                                break;
                            case EquipType.EtGauntlet:
                                mainStat1Type = baseSoulSpec.GauntletMainStat1Type;
                                mainStat1 = baseSoulSpec.GauntletMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.GauntletMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.GauntletMainStat2Type;
                                mainStat2 = baseSoulSpec.GauntletMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.GauntletMainStat2PerLevel;
                                break;
                            case EquipType.EtBoots:
                                mainStat1Type = baseSoulSpec.BootsMainStat1Type;
                                mainStat1 = baseSoulSpec.BootsMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.BootsMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.BootsMainStat2Type;
                                mainStat2 = baseSoulSpec.BootsMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.BootsMainStat2PerLevel;
                                break;
                            case EquipType.EtCloak:
                                mainStat1Type = baseSoulSpec.CloakMainStat1Type;
                                mainStat1 = baseSoulSpec.CloakMainStat1Value;
                                mainStat1PerLevel = baseSoulSpec.CloakMainStat1PerLevel;
                                mainStat2Type = baseSoulSpec.CloakMainStat2Type;
                                mainStat2 = baseSoulSpec.CloakMainStat2Value;
                                mainStat2PerLevel = baseSoulSpec.CloakMainStat2PerLevel;
                                break;
                        }

                        mainStat1 = (uint) (mainStat1 * valueFactor);
                        mainStat1PerLevel = (uint) (mainStat1PerLevel * modFactor);
                        mainStat2 = (uint) (mainStat2 * valueFactor);
                        mainStat2PerLevel = (uint) (mainStat2PerLevel * modFactor);
                        
                        equipInfo.MainStats[0].Init(soulSpec.Uid, mainStat1Type, mainStat1, mainStat1PerLevel);
                        equipInfo.MainStats[1].Init(soulSpec.Uid, mainStat2Type, mainStat2, mainStat2PerLevel);

                        if (spec.EquipGrade == EquipGradeType.EgtLegendaryPlus) // plus grade
                        {
                            equipInfo.MainSkillUid = soulSpec.MainSkill2Uid;
                            equipInfo.SubSkillUid = soulSpec.SubSkill2Uid;
                        }
                        else
                        {
                            equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                            equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                        }
                    }
                    else
                    {   // sub soul
                        StatType subStatType = StatType.StNone;
                        uint subStat = 0;
                        uint subStatPerLevel = 0;
                        
                        switch (equipType)
                        {
                            case EquipType.EtWeapon:
                                subStatType = soulSpec.WeaponSubStatType;
                                subStat = soulSpec.WeaponSubStatValue;
                                subStatPerLevel = soulSpec.WeaponSubStatPerLevel;
                                break;
                            case EquipType.EtHelmet:
                                subStatType = soulSpec.HelmetSubStatType;
                                subStat = soulSpec.HelmetSubStatValue;
                                subStatPerLevel = soulSpec.HelmetSubStatPerLevel;
                                break;
                            case EquipType.EtArmor:
                                subStatType = soulSpec.ArmorSubStatType;
                                subStat = soulSpec.ArmorSubStatValue;
                                subStatPerLevel = soulSpec.ArmorSubStatPerLevel;
                                break;
                            case EquipType.EtGauntlet:
                                subStatType = soulSpec.GauntletSubStatType;
                                subStat = soulSpec.GauntletSubStatValue;
                                subStatPerLevel = soulSpec.GauntletSubStatPerLevel;
                                break;
                            case EquipType.EtBoots:
                                subStatType = soulSpec.BootsSubStatType;
                                subStat = soulSpec.BootsSubStatValue;
                                subStatPerLevel = soulSpec.BootsSubStatPerLevel;
                                break;
                            case EquipType.EtCloak:
                                subStatType = soulSpec.CloakSubStatType;
                                subStat = soulSpec.CloakSubStatValue;
                                subStatPerLevel = soulSpec.CloakSubStatPerLevel;
                                break;
                        }

                        if (spec.EquipGrade == EquipGradeType.EgtLegendaryPlus)
                        {
                            subStat = (uint) (subStat * valueFactor);
                            subStatPerLevel = (uint) (subStatPerLevel * modFactor);
                        }
                        equipInfo.SubStats[i-1].Init(soulSpec.Uid, subStatType, subStat, subStatPerLevel);
                    }
                }
            }

            
            // stat post process
            equipInfo.Calc(gamedata, level);

            return equipInfo;
        }

        public static long CalcStatValue(GameDataManager gameData, StatStruct statStruct, ClassType classType, uint level)
        {
		    var charUid = GameDataManager.GetUidByString("character.pc." + (int)classType);
            var charSheet = gameData.GetData<CharacterInfoSpec>(charUid);
            var statFactor = 1f;
            
            var constValue = 500;
            var statType = statStruct.StatType;
            if (statType == StatType.StMaxHp)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.maxHP", 500);
                statFactor = charSheet.StatHPFactor;
            }else if (statType == StatType.StAttackPower)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.attackPower", 500);
                statFactor = charSheet.StatAttackPowerFactor;
            }else if (statType == StatType.StDefence)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.defence", 500);
                statFactor = charSheet.StatDefenceFactor;
            }else if (statType == StatType.StHit)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.hit", 500);
                statFactor = charSheet.StatHitFactor;
            }else if (statType == StatType.StEvasion)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.evasion", 500);
                statFactor = charSheet.StatEvasionFactor;
            }else if (statType == StatType.StCrit)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.crit", 500);
                statFactor = charSheet.StatCritFactor;
            }else if (statType == StatType.StResilience)
            {
                constValue = gameData.GetConfigNumber("config.combat.factor.equip.resilience", 500);
                statFactor = charSheet.StatResilienceFactor;
            }
            

            uint levelConst = 0;
            if (level > 0)
            {
                levelConst = level-1;
            }
            
			var constantFactor = (1d + (double)levelConst / constValue);
            
			var calcResult = statStruct.StatValue + (long)Math.Pow((double)(levelConst*statStruct.StatPerLevel), constantFactor);
            
            return (long)(calcResult * statFactor);
        }
        public static long CalcStatValue2(GameDataManager gameData, StatStruct statStruct, ClassType classType, uint level)
        {
		    var charUid = GameDataManager.GetUidByString("character.pc." + (int)classType);
            var charSheet = gameData.GetData<CharacterInfoSpec>(charUid);
            var statFactor = 1f;
            
            var statType = statStruct.StatType;
            if (statType == StatType.StMaxHp)
            {
                statFactor = charSheet.StatHPFactor;
            }else if (statType == StatType.StAttackPower)
            {
                statFactor = charSheet.StatAttackPowerFactor;
            }else if (statType == StatType.StDefence)
            {
                statFactor = charSheet.StatDefenceFactor;
            }else if (statType == StatType.StHit)
            {
                statFactor = charSheet.StatHitFactor;
            }else if (statType == StatType.StEvasion)
            {
                statFactor = charSheet.StatEvasionFactor;
            }else if (statType == StatType.StCrit)
            {
                statFactor = charSheet.StatCritFactor;
            }else if (statType == StatType.StResilience)
            {
                statFactor = charSheet.StatResilienceFactor;
            }
            

            uint levelConst = 0;
            if (level > 0)
            {
                levelConst = level-1;
            }


            var calcResult = statStruct.StatValue + (statStruct.StatPerLevel * statFactor) * levelConst;
            
            return (long)(calcResult);
        }
        
        // General & Soul
        // General & Soul from DBEquip in Client
        private static EquipInfo LoadSoulsInternalNew(GameDataManager gamedata, DBEquip dbEquip, List<ulong> soulUids)
        {
            var spec = gamedata.GetData<EquipSpec>(dbEquip.Uid);
            var level = dbEquip.Level;
            
            var equipType = spec.Type;
            var equipSoulType = spec.EquipSoulType;
            
            if (equipSoulType == EquipSoulType.None)
            {
                // do nothing
                return null;
            }

            var equipInfo = new EquipInfo(spec.ClassType);
            
            var soulMainStat1Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.MainStat1);
            var soulMainStat1Value = (uint)dbEquip.MainStat1Value;
            var soulMainStat1PerLevel = (uint)dbEquip.MainStat1PerLevel;
            var soulMainStat2Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.MainStat2);
            var soulMainStat2Value = (uint)dbEquip.MainStat2Value;
            var soulMainStat2PerLevel = (uint)dbEquip.MainStat2PerLevel;
            
            var soulSubStat1Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.SubStat1);
            var soulSubStat1Value = (uint)dbEquip.SubStat1Value;
            var soulSubStat1PerLevel = (uint)dbEquip.SubStat1PerLevel;
            var soulSubStat2Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.SubStat2);
            var soulSubStat2Value = (uint)dbEquip.SubStat2Value;
            var soulSubStat2PerLevel = (uint)dbEquip.SubStat2PerLevel;
            var soulSubStat3Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.SubStat3);
            var soulSubStat3Value = (uint)dbEquip.SubStat3Value;
            var soulSubStat3PerLevel = (uint)dbEquip.SubStat3PerLevel;
            var soulSubStat4Type = CorgiEnum.ParseEnumPascal<StatType>(dbEquip.SubStat4);
            var soulSubStat4Value = (uint)dbEquip.SubStat4Value;
            var soulSubStat4PerLevel = (uint)dbEquip.SubStat4PerLevel;

            equipInfo.MainStats[0].Init(dbEquip.SoulUidMain, soulMainStat1Type, soulMainStat1Value, soulMainStat1PerLevel);
            equipInfo.MainStats[1].Init(dbEquip.SoulUidMain, soulMainStat2Type, soulMainStat2Value, soulMainStat2PerLevel);
            
            equipInfo.SubStats[0].Init(dbEquip.SoulUid1, soulSubStat1Type, soulSubStat1Value, soulSubStat1PerLevel);
            equipInfo.SubStats[1].Init(dbEquip.SoulUid2, soulSubStat2Type, soulSubStat2Value, soulSubStat2PerLevel);
            equipInfo.SubStats[2].Init(dbEquip.SoulUid3, soulSubStat3Type, soulSubStat3Value, soulSubStat3PerLevel);
            equipInfo.SubStats[3].Init(dbEquip.SoulUid4, soulSubStat4Type, soulSubStat4Value, soulSubStat4PerLevel);
            
            // skill
            if (spec.EquipSoulType == EquipSoulType.Soul)
            {
                var soulUid = dbEquip.SoulUidMain;
                var soulSpec = GameDataManager.Instance.GetData<EquipDragonSoulSpec>(soulUid);

                if (soulSpec != null)
                {
                    equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                    equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                    if (spec.EquipGrade == EquipGradeType.EgtLegendaryPlus)
                    {
                        equipInfo.MainSkillUid = soulSpec.MainSkill2Uid;
                        equipInfo.SubSkillUid = soulSpec.SubSkill2Uid;
                    }
                    else
                    {
                        equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                        equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                    }
                }
            }
            
            // stat post process
            equipInfo.Calc2(gamedata, level);

            return equipInfo;
        }
        
        // load from SharedEquip in CombatServer
        private static EquipInfo LoadSoulsInternalNew(GameDataManager gamedata, SharedEquipInfo sharedEquip, List<ulong> soulUids)
        {
            var spec = gamedata.GetData<EquipSpec>(sharedEquip.uid);
            var level = sharedEquip.level;
            
            var equipType = spec.Type;
            var equipSoulType = spec.EquipSoulType;
            
            if (equipSoulType == EquipSoulType.None)
            {
                // do nothing
                return null;
            }

            var equipInfo = new EquipInfo(spec.ClassType);

            var soulMainStat1Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.mainStat1);
            var soulMainStat1Value = (uint)sharedEquip.mainStat1Value;
            var soulMainStat1PerLevel = (uint)sharedEquip.mainStat1PerLevel;
            var soulMainStat2Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.mainStat2);
            var soulMainStat2Value = (uint)sharedEquip.mainStat2Value;
            var soulMainStat2PerLevel = (uint)sharedEquip.mainStat2PerLevel;
            
            var soulSubStat1Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.subStat1);
            var soulSubStat1Value = (uint)sharedEquip.subStat1Value;
            var soulSubStat1PerLevel = (uint)sharedEquip.subStat1PerLevel;
            var soulSubStat2Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.subStat2);
            var soulSubStat2Value = (uint)sharedEquip.subStat2Value;
            var soulSubStat2PerLevel = (uint)sharedEquip.subStat2PerLevel;
            var soulSubStat3Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.subStat3);
            var soulSubStat3Value = (uint)sharedEquip.subStat3Value;
            var soulSubStat3PerLevel = (uint)sharedEquip.subStat3PerLevel;
            var soulSubStat4Type = CorgiEnum.ParseEnumPascal<StatType>(sharedEquip.subStat4);
            var soulSubStat4Value = (uint)sharedEquip.subStat4Value;
            var soulSubStat4PerLevel = (uint)sharedEquip.subStat4PerLevel;
            
            equipInfo.MainStats[0].Init(sharedEquip.soulUidMain, soulMainStat1Type, soulMainStat1Value, soulMainStat1PerLevel);
            equipInfo.MainStats[1].Init(sharedEquip.soulUidMain, soulMainStat2Type, soulMainStat2Value, soulMainStat2PerLevel);
            
            equipInfo.SubStats[0].Init(sharedEquip.soulUid1, soulSubStat1Type, soulSubStat1Value, soulSubStat1PerLevel);
            equipInfo.SubStats[1].Init(sharedEquip.soulUid2, soulSubStat2Type, soulSubStat2Value, soulSubStat2PerLevel);
            equipInfo.SubStats[2].Init(sharedEquip.soulUid3, soulSubStat3Type, soulSubStat3Value, soulSubStat3PerLevel);
            equipInfo.SubStats[3].Init(sharedEquip.soulUid4, soulSubStat4Type, soulSubStat4Value, soulSubStat4PerLevel);
            
            // skill
            if (spec.EquipSoulType == EquipSoulType.Soul)
            {
                var soulUid = sharedEquip.soulUidMain;
                var soulSpec = GameDataManager.Instance.GetData<EquipDragonSoulSpec>(soulUid);

                if (soulSpec != null)
                {
                    equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                    equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                    if (spec.EquipGrade == EquipGradeType.EgtLegendaryPlus)
                    {
                        equipInfo.MainSkillUid = soulSpec.MainSkill2Uid;
                        equipInfo.SubSkillUid = soulSpec.SubSkill2Uid;
                    }
                    else
                    {
                        equipInfo.MainSkillUid = soulSpec.MainSkillUid;
                        equipInfo.SubSkillUid = soulSpec.SubSkillUid;
                    }
                }
            }
            
            // stat post process
            equipInfo.Calc2(gamedata, level);

            return equipInfo;
        }

        public static bool IsBetterEquip(DBEquip currentEquip, DBEquip equip)
        {
            if (currentEquip == null || equip == null)
            {
                return false;
            }

            if (equip.Grade > currentEquip.Grade)
            {
                return true;
            }else if (equip.Grade < currentEquip.Grade)
            {
                return false;
            }

            if (equip.Level > currentEquip.Level)
            {
                return true;
            }else if (equip.Level < currentEquip.Level)
            {
                return false;
            }
            
            return false;
        }
        
    }

}