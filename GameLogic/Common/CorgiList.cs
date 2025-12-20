using System;
using System.Collections.Generic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public class CorgiList<T> : List<T> where T : ICorgiSerializable, new()
    {

        public void Serialize(IPacketWriter writer)
        {
	        var count = this.Count;
	        writer.Write(count);
	        
	        foreach (var curValue in this)
	        {
		        if (curValue == null)
		        {
			        continue;
		        }
		        curValue.Serialize(writer);
	        }
        }

        public CorgiList<T> DeSerialize(IPacketReader reader)
        {
	        int count = 0;

	        reader.Read(out count);
	        
	        for (var i = 0; i<count; i++)
	        {
				var newInstance = new T();
				if (newInstance.DeSerialize(reader) == null)
				{
					continue;
				}
				Add(newInstance);
	        }

	        return this;
        }
    }
    
    public class CorgiNumericList<T> : List<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {

        public void Serialize(IPacketWriter writer)
        {
	        var count = this.Count;
	        writer.Write(count);
	        
	        foreach (var curValue in this)
	        {
		        WriteInner(writer, curValue);
	        }
        }

        public void DeSerialize(IPacketReader reader)
        {
	        int count = 0;

	        reader.Read(out count);
	        
	        for (var i = 0; i<count; i++)
	        {
		        T value;
		        ReadInner(reader, out value);
		        Add(value);
	        }
        }
        
        protected virtual void WriteInner(IPacketWriter writer, T value)
        {
	        throw new CorgiException("not implemented method");
        }
        protected virtual void ReadInner(IPacketReader reader, out T value)
        {
	        throw new CorgiException("not implemented method");
        }
    }

    public class CorgiIntList : CorgiNumericList<int>
    {
        protected override void WriteInner(IPacketWriter writer, int value)
        {
	        writer.Write(value);
        }
        protected override void ReadInner(IPacketReader reader, out int value)
        {
	        reader.Read(out value);
        }
    }
    
    public class CorgiStringList : CorgiNumericList<string>
    {
        protected override void WriteInner(IPacketWriter writer, string value)
        {
	        writer.Write(value);
        }
        protected override void ReadInner(IPacketReader reader, out string value)
        {
	        reader.Read(out value);
        }
    }
    
    public class CorgiUlongList : CorgiNumericList<ulong>
    {
        protected override void WriteInner(IPacketWriter writer, ulong value)
        {
	        writer.Write(value);
        }
        protected override void ReadInner(IPacketReader reader, out ulong value)
        {
	        reader.Read(out value);
        }
    }
    
}