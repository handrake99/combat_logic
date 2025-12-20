using System.Collections.Generic;
using Corgi.DBSchema;
using Corgi.GameData;
using Corgi.Protocol;
using Google.Protobuf;
using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public interface ICombatBridge
    {
        string Id();

        void DoDestroy(int reason);

        List<Unit> CreateUnitList(Dungeon dungeon);
        void UpdateUnitList(Dungeon dungeon);
        //Character CreateCharacter(Dungeon dungeon, string charDbId);
        Skill CreateSkill(Character owner, ulong skillBaseUid);
        Skill CreateSkillFromItem(Character owner, ulong skillItemBaseUid);
        
        //List<Unit> GetCharacters();
        

        Deck GetPersonalDeck(Unit owner);
        Deck GetCoPartyDeck(Unit owner);
        Deck GetSoloPartyDeck(string ownerId, Unit unit);
        Deck GetSoloOffenseDeck(string ownerId, Unit unit);
        Deck GetSoloDefenceDeck(string ownerId, string targetId, Unit unit);
        
        Equip CreateEquip(Character owner, string equipDbId);
        List<BindingStone> GetBindingStones(Character owner);
        SharedAlmanacStat GetAlmanacStat(Character owner);
        SharedCharInfo GetCharInfo(string characterId);

        void StageFinish(string characterId, ulong stageUid, bool stageResult);
        void ChallengeFinish(string characterId, ulong stageUid, bool stageResult);
        void InstanceDungeonFinish(string characterId, DungeonCriteriaType dungeonType, string dungeonId, ulong dungeonUid, ulong stageUid, bool stageResult);
        void WorldBossFinish(string bossKey, string characterId, long totalDamage, bool stageResult);
        void WorldBossDamage(string dungeonKey, string characterId, long damage);
        void WorldBossDead(string dayNum);
        
        void RiftFinish(string dungeonKey, string dungeonId, string characterId, long totalDamage, bool stageResult);
        void RiftDead(string dungeonId, string characterId);

        void ArenaFinish(string dungeonKey, string characterId, string targetId, string winnerId);
        
        // load data
        bool RequestData(string characterId, RequestParam param, string callbackName);
        bool RequestData(string characterId, List<RequestParam> paramList, string callbackName);
        bool IsRequestCompleted();
        //bool IsUpdatedParty();
        bool UpdateUnit();
        void CachedUpdateUnit(Unit joinUnit, string leaveUnitObjectId);
        void DungeonStateWaitLog(string dungeonState);
        void CheckNoConnectionHunting(DungeonState dungeonState, bool isChallenging);
        
        bool Test_BoolValue();
        int Test_IntValue();
    }

    public interface IGameDataBridge
    {
        T GetSpec<T>(ulong uid) where T : class, IMessage, new();
        T GetSpec<T>(string uidStr) where T : class, IMessage, new();
        string GetString(string uidStr);
    }
}
