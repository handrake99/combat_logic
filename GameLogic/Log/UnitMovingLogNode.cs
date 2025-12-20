using IdleCs.GameLogic;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLog
{
    public class UnitMovingLogNode : CombatLogNode
    {
	    public string ActorId;
	    public string TargetId;
	    public CorgiPosition Position;

	    public UnitMovingLogNode()
	    {
		    Position = new CorgiPosition();
	    }
	    

	    public UnitMovingLogNode(Unit actor, Unit target)
	    {
		    ActorId = actor.ObjectId;
		    if (target != null)
		    {
				TargetId = target.ObjectId;
		    }
		    else
		    {
			    TargetId = string.Empty;
		    }
		    Position = new CorgiPosition(actor.Position);
	    }
	    
	    public override int GetClassType()
	    {
		    return (int) CombatLogNodeType.Moving;
	    }
		
		public override bool Serialize(IPacketWriter writer)
		{
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(ActorId);
            writer.Write(TargetId);
            Position.Serialize(writer);

            return true;
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out ActorId);
            reader.Read(out TargetId);
            Position.DeSerialize(reader);

            return this;
		}
	    
	    public override void LogDebug(IGameDataBridge bridge)
	    {
		    //CorgiLog.LogLine($"{this} / {ActorId} => {Position}");
		    
		    base.LogDebug(bridge);
	    }
    }
}