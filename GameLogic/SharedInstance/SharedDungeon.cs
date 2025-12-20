using System.Collections.Generic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedDungeon : SharedInstance
    {
        public CorgiList<SharedMonster> SharedMonsters => CurStage?.SharedMonsters;
        public CorgiList<SharedStructure> SharedStructures;

        public CorgiUlongList AffixList;
        public CorgiUlongList PartyBuffList;

        public int CurStageIndex;

        public bool IsChallenging;
        public uint ChallengeCount;

        public override int GetClassType()
        {
            return (int)SharedInstanceCategory.Dungeon;
        }

        // for combat Server
        public override void Init(CorgiObject original)
        {
            var dungeon = original as Dungeon;
            if (dungeon == null)
            {
                throw new CorgiException("failed to initialize SharedDungeon");
            }
            
            base.Init(original);

            SharedStructures = new CorgiList<SharedStructure>();
            foreach (var inst in dungeon.StructureList)
            {
                if (inst == null)
                {
                    continue;
                }
                var unit = new SharedStructure();
                unit.Init(inst);
                SharedStructures.Add(unit);
            }

            AffixList = new CorgiUlongList();
            AffixList.AddRange(dungeon.AffixList);

            PartyBuffList = new CorgiUlongList();
            PartyBuffList.AddRange(dungeon.PartyBuffList);
            
            CurStageIndex = dungeon.CurStageIndex;
            // if (dungeon.CurStage != null)
            // {
            //     CurStage = new SharedStage();
            //     CurStage.Init(dungeon.CurStage);
            // }

            IsChallenging = dungeon.GetIsChallenging();
            ChallengeCount = dungeon.GetChallengeCount();

            //CorgiLog.LogLine("Dungeon Challenge State : {0}, {1}", IsChallenging, ChallengeCount);
        }

        public override SharedUnit GetUnit(string unitId)
        {
            var unit = base.GetUnit(unitId);
            if (unit != null)
            {
                return unit;
            }
            
            foreach (var curUnit in SharedMonsters)
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
            
            foreach (var curUnit in SharedStructures)
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
        
        public override List<SharedMonster> GetMonsters()
        {
            return CurStage?.SharedMonsters ;
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
            
            AffixList.Serialize(writer);
            PartyBuffList.Serialize(writer);
            
            writer.Write(CurStageIndex);

            if (CurStage == null)
            {
                writer.Write(0);
            }
            else
            {
                CurStage.Serialize(writer);
            }
            
            writer.Write(IsChallenging);
            writer.Write(ChallengeCount);

            return true;
        }
        
        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            AffixList = new CorgiUlongList();
            AffixList.DeSerialize(reader);

            PartyBuffList = new CorgiUlongList();
            PartyBuffList.DeSerialize(reader);
            
            reader.Read(out CurStageIndex);
            
            var curStage = new SharedStage();
            CurStage = (SharedStage)curStage.DeSerialize(reader);
            
            reader.Read(out IsChallenging);
            reader.Read(out ChallengeCount);
            
            return this;
        }
    }
}