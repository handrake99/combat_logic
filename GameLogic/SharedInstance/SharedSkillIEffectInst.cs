using System;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json.Converters;

namespace IdleCs.GameLogic.SharedInstance
{
    public class SharedSkillEffectInst : CorgiSharedObject
    {
        public ulong DurationRemain;
        public uint VisiblePriority;
        public uint StackCount;

        public override void Init(CorgiObject original)
        {
            var effectInst = original as SkillEffectInst;
            if (effectInst == null)
            {
                throw new CorgiException("failed to initailize SharedSkillEffectInst");
            }
            
            base.Init(original);

            DurationRemain = effectInst.CurDuration;
            VisiblePriority = effectInst.VisiblePriority;
            StackCount = effectInst.StackCount;

        }
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(DurationRemain);
            writer.Write(StackCount);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out DurationRemain);
            reader.Read(out StackCount);

            return this;
        }
    }
}