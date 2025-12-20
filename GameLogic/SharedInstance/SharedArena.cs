using System.Collections.Generic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using IdleCs.GameLogic;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedArena : SharedInstance
    {
        public CorgiList<SharedMonster> SharedEnemies;
        public ulong CurCombatTick;
        public uint SuddenDeathCount;

        public override int GetClassType()
        {
            return (int)SharedInstanceCategory.Arena;
        }
        
        // for combat Server
        public override void Init(CorgiObject original)
        {
            var dungeon = original as DungeonArena;
            if (dungeon == null)
            {
                throw new CorgiException("failed to initialize SharedDungeon");
            }
            
            base.Init(original);
            
            if (dungeon.CurStage != null)
            {
                CurStage = new SharedStage();
                CurStage.Init(dungeon.CurStage);
            }
            
            SharedEnemies = new CorgiList<SharedMonster>();
            foreach (var inst in dungeon.EnemyList)
            {
                if (inst == null)
                {
                    continue;
                }
                var unit = new SharedCharacterArena();
                unit.Init(inst);
                SharedEnemies.Add(unit);
            }

            CurCombatTick = dungeon.CurCombatTick;
            SuddenDeathCount = dungeon.SuddenDeathCount;
        }

        public override SharedUnit GetUnit(string unitId)
        {
            var unit = base.GetUnit(unitId);
            if (unit != null)
            {
                return unit;
            }
            
            foreach (var curUnit in SharedEnemies)
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
        
        
        public override string GetStageId()
        {
            return CurStage?.objectId;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            if (CurStage == null)
            {
                writer.Write(0);
            }
            else
            {
                CurStage.Serialize(writer);
            }

            SharedEnemies.Serialize(writer);

            writer.Write(CurCombatTick);
            writer.Write(SuddenDeathCount);
            
            return true;
        }
        
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            var curStage = new SharedStage();
            CurStage = (SharedStage)curStage.DeSerialize(reader);

            SharedEnemies = new CorgiList<SharedMonster>();
            reader.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                var inst = new SharedCharacterArena();
                if (inst.DeSerialize(reader) == null)
                {
                    continue;
                }
                SharedEnemies.Add(inst);
            }

            reader.Read(out CurCombatTick);
            reader.Read(out SuddenDeathCount);
            
            return this;
        }
    }
}
