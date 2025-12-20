using System;
using IdleCs.GameLogic;
using IdleCs.Network.NetLib;

namespace IdleCs.GameLog
{
    [Serializable]
	public class AddManaSkillCompLogNode : ActiveSkillCompLogNode
	{

		/// <summary>
		/// Result
		/// </summary>
		public int AddedMana;

	    public AddManaSkillCompLogNode()
			: base()
	    {
	    }

	    public AddManaSkillCompLogNode(Unit caster, Unit target, SkillComp skillComp)
			: base(caster, target, skillComp)
		{
		}

	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.AddMana;
	    }

	    public override bool Serialize(IPacketWriter writer)
        {
	        if (base.Serialize(writer) == false)
	        {
		        return false;
	        }
	        
	        writer.Write(AddedMana);

	        return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        if (base.DeSerialize(reader) == null)
	        {
		        return null;
	        }

	        reader.Read(out AddedMana);
	        
	        return this;
        }
	}
}