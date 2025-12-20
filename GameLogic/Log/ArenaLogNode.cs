using System;
using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Network.NetLib;


namespace IdleCs.GameLog
{
    [Serializable]
	public class ArenaLogNode : LogNode
	{
// 		public DungeonState DungeonState;
//
// 		public CombatLogNode CombatLogNode;
//
// 		public SharedArena SharedArena;
//
// 		public string DungeonId => SharedArena?.objectId;
//
// 		public ArenaLogNode()
// 		{
// 		}
// 		
// 		public ArenaLogNode(DungeonState curState)
// 		{
// 			DungeonState = curState;
// 		}
//
// 		public void UpdateDungeonState(DungeonState newState)
// 		{
// 			DungeonState = newState;
// 		}
//
// 		public void SetCombatLog(CombatLogNode logNode)
// 		{
// 			if (logNode == null)
// 				return;
// 			
// 			if (CombatLogNode is TickLogNode && logNode is TickLogNode)
// 			{
// 				if (CombatLogNode.ChildCount > 0)
// 				{
// 					CombatLogNode.AddChild(logNode);
// 					return;
// 				}
// 			}
// 			 
// 			 CombatLogNode = logNode;
// 		}
//
// 		public void SetRoot()
// 		{
// 			if (CombatLogNode == null)
// 			{
// 				return;
// 			}
//
// 			CombatLogNode.SetRoot(this);
// 		}
// 		
//         public override int GetClassType()
//         {
// 	        return (int)CombatLogNodeType.Dungeon;
//         }
// 		
// 		public override bool Serialize(IPacketWriter writer)
// 		{
// 			if (base.Serialize(writer) == false)
// 			{
// 				return false;
// 			}
// 			
// 			writer.WriteEnum(DungeonState);
//
// 			if (CombatLogNode == null)
// 			{
// 				writer.Write(0);
// 			}
// 			else
// 			{
// 				CombatLogNode.Serialize(writer);
// 			}
//
// 			if (SharedDungeon == null)
// 			{
// 				writer.Write(0);
// 			}
// 			else
// 			{
// 				SharedDungeon.Serialize(writer);
// 			}
// 			
// 			writer.Write(IsChallenging);
//
// 			return true;
// 		}
//
// 		public override ICorgiSerializable DeSerialize(IPacketReader reader)
// 		{
// 			if (base.DeSerialize(reader) == null)
// 			{
// 				return null;
// 			}
//
// 			reader.ReadEnum(out DungeonState);
//
// 			CombatLogNodeType logNodeType;
// 			reader.ReadEnum(out logNodeType);
//
// 			CombatLogNode = CombatLogNode.Create(logNodeType);
// 			if (CombatLogNode != null)
// 			{
// 				CombatLogNode.DeSerialize(reader);
// 			}
//
// 			var sharedDungeon = new SharedDungeon();
// 	        SharedDungeon = (SharedDungeon)sharedDungeon.DeSerialize(reader);
// 			
// 			reader.Read(out IsChallenging);
//
// 			return this;
// 		}
// 		
// //        public override void OnSerialize(IPacketWriter writer)
// //        {
// //	        SharedDungeon.Serialize(writer);
// //        }
// //        
// //        public override void OnDeSerialize(IPacketReader reader)
// //        {
// //	        var sharedDungeon = new SharedDungeon();
// //	        SharedDungeon = (SharedDungeon)sharedDungeon.DeSerialize(reader);
// //        }
//
// 	    public override void LogDebug(IGameDataBridge bridge)
// 		{
// 			CorgiCombatLog.Log(CombatLogCategory.Skill,"Dungeon State : {0}", DungeonState.ToString());
// 			CombatLogNode?.LogDebug(bridge);
// 		}
	}
}
