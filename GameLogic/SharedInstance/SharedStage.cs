using System.Collections.Generic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedStage : CorgiSharedObject
    {
        public CorgiList<SharedMonster> SharedMonsters;
        
        public override void Init(CorgiObject original)
        {
            var stage= original as Stage;
            if (stage == null)
            {
                throw new CorgiException("failed to initialize SharedStage");
            }
            
            base.Init(original);
            
            SharedMonsters = new CorgiList<SharedMonster>();

            if (stage.MonsterList != null)
            {
                foreach (var inst in stage.MonsterList)
                {
                    if (inst == null)
                    {
                        continue;
                    }

                    var unit = new SharedMonster();
                    unit.Init(inst);
                    SharedMonsters.Add(unit);
                }
            }
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            SharedMonsters.Serialize(writer);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            SharedMonsters = new CorgiList<SharedMonster>();
            SharedMonsters.DeSerialize(reader);

            return this;
        }
        
    }
}