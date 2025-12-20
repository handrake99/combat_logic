using IdleCs.Network.NetLib;
using IdleCs.Utils;
using System.Collections.Generic;
using System.Net.Http;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedInstance: CorgiSharedObject
    {
        public DungeonState State;

        public CorgiList<SharedCharacter> SharedCharacters;
        
        public SharedStage CurStage;
        
        // for combat Server
        public override void Init(CorgiObject original)
        {
            var dungeon = original as Dungeon;
            if (dungeon == null)
            {
                throw new CorgiException("failed to initialize SharedDungeon");
            }
            
            base.Init(original);

            State = dungeon.State;
            
            SharedCharacters = new CorgiList<SharedCharacter>();
            foreach (var inst in dungeon.CharList)
            {
                if (inst == null)
                {
                    continue;
                }
                var unit = new SharedCharacter();
                unit.Init(inst);
                SharedCharacters.Add(unit);
            }
            
            if (dungeon.CurStage != null)
            {
                CurStage = new SharedStage();
                CurStage.Init(dungeon.CurStage);
            }
        }

        public virtual SharedUnit GetUnit(string unitId)
        {
            foreach (var curUnit in SharedCharacters)
            {
                if (curUnit == null)
                {
                    continue;
                }

                if (curUnit.objectId == unitId)
                {
                    return curUnit;
                }
            }
            return null;
        }

        public virtual List<SharedMonster> GetMonsters()
        {
            return null;
        }

        public virtual string GetStageId()
        {
            return null;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.WriteEnum<DungeonState>(State);
            SharedCharacters.Serialize(writer);

            return true;
        }
        
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.ReadEnum<DungeonState>(out State);
            SharedCharacters = new CorgiList<SharedCharacter>();
            SharedCharacters.DeSerialize(reader);
            
            return this;
        }

        public static SharedInstance Create(SharedInstanceCategory category)
        {
            switch (category)
            {
                case SharedInstanceCategory.Dungeon:
                    return new SharedDungeon();
                case SharedInstanceCategory.Arena:
                    return new SharedArena();
                default:
                    return null;
            }
        }
    }
}
