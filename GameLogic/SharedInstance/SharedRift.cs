using System.Collections.Generic;
using IdleCs.Network.NetLib;
using Newtonsoft.Json.Linq;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedRift : CorgiSharedObject
    {
        public string dungeonId;
        public ulong dungeonUid;
        public ulong stageUid;
        public uint level;
        public uint grade;
        public ulong startTimestamp;
        public ulong endTimestamp;
        
        public long maxHp;
        public long curHp;

        public Dictionary<string/*owner charaterId*/, Dictionary<string/*dmaged characterId*/, long>> _damageMap;
        
        public CorgiList<SharedRiftCharacter> characters;
            
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            writer.Write(dungeonId);
            writer.Write(dungeonUid);
            writer.Write(stageUid);
            writer.Write(level);
            writer.Write(grade);
            writer.Write(startTimestamp);
            writer.Write(endTimestamp);
            
            writer.Write(maxHp);
            writer.Write(curHp);
            
            characters.Serialize(writer);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out dungeonId);
            reader.Read(out dungeonUid);
            reader.Read(out stageUid);
            reader.Read(out level);
            reader.Read(out grade);
            reader.Read(out startTimestamp);
            reader.Read(out endTimestamp);

            reader.Read(out maxHp);
            reader.Read(out curHp);

            var newCharacters = new CorgiList<SharedRiftCharacter>();
            characters = newCharacters.DeSerialize(reader);

            return this;
        }

        public SharedRift Clone()
        {
            var shared = new SharedRift();
            
            shared.dungeonId = dungeonId;
            shared.dungeonUid = dungeonUid;
            shared.stageUid = stageUid;
            shared.level = level;
            shared.grade = grade;
            shared.startTimestamp = startTimestamp;
            shared.endTimestamp = endTimestamp;
            shared.maxHp = maxHp;
            shared.curHp = curHp;
            
            shared.characters = new CorgiList<SharedRiftCharacter>();

            if (characters != null)
            {
                foreach (var character in characters)
                {
                    if (character == null)
                    {
                        continue;
                    }
                    shared.characters.Add(character.Clone());
                }
                
            }
            
            return shared;
        }
        
        public JObject ToJson()
        {
            var ret = new JObject();
            
            ret.Add("dungeonId", dungeonId);
            ret.Add("dungeonUid", dungeonUid);
            ret.Add("stageUid", stageUid);
            ret.Add("level", level);
            ret.Add("grade", grade);
            ret.Add("startTimestamp", startTimestamp);
            ret.Add("endTimestamp", endTimestamp);
            ret.Add("maxHp", maxHp);
            ret.Add("curHp", curHp);
            
            var retArray = new JArray();

            if (characters != null)
            {
                
                foreach (var character in characters)
                {
                    if (character == null)
                    {
                        continue;
                    }

                    retArray.Add(character.ToJson());
                }
                
            }

            ret.Add("characters", retArray);
            
            return ret;
        }

        public void RecordDamage(string characterId, long damage)
        {
            foreach (var sharedChar in characters)
            {
                if (sharedChar == null)
                {
                    continue;
                }

                if (sharedChar.characterId == characterId)
                {
                    sharedChar.damage += (ulong)damage;
                    curHp -= damage;
                    if (curHp < 0)
                    {
                        curHp = 0;
                    }
                    break;
                }
            }
        }

        public void RecordHeal(long heal)
        {
            curHp += heal;
            if (curHp > maxHp)
            {
                curHp = maxHp;
            }
        }

        public void JoinRiftChallenge(string characterId)
        {
            var check = false;
            
            foreach (var sharedRiftCharacter in characters)
            {
                if (sharedRiftCharacter != null && sharedRiftCharacter.characterId == characterId)
                {
                    if (sharedRiftCharacter.isChallenging)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid isChallenging[{0}] for character[{1}] in JoinRiftChallenge", sharedRiftCharacter.isChallenging, characterId);
                    }
                    sharedRiftCharacter.isChallenging = true;
                    sharedRiftCharacter.tryCount++;
                    check = true;
                    break;
                }
            }

            if (!check)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "no character[{0}] matching in StartRift", characterId);
            }
        }

        public void CompleteRiftChallenge(string characterId)
        {
            var check = false;
            
            foreach (var sharedRiftCharacter in characters)
            {
                if (sharedRiftCharacter != null && sharedRiftCharacter.characterId == characterId)
                {
                    if (!sharedRiftCharacter.isChallenging)
                    {
                        CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "invalid isChallenging[{0}] for character[{1}] in CompleteRiftChallenge", sharedRiftCharacter.isChallenging, characterId);
                    }
                    sharedRiftCharacter.isChallenging = false;
                    check = true;
                    break;
                }
            }

            if (!check)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Dungeon, "no character[{0}] matching in EndRift", characterId);
            }
        }
    }
}
