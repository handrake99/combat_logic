using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using IdleCs;
using IdleCs.GameLogic;
using IdleCs.Library;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json;

namespace IdleCs.GameLog
{
	public abstract class CombatLogNode : LogNode
	{
		[JsonIgnore]
		private CombatLogNode _parent = null;
		[JsonIgnore]
		private CombatLogNode _lastLog = null;
		
		// for local mode
		[JsonIgnore] public DungeonLogNode DungeonLogNode { get; set; }

		public List<CombatLogNode> Childs;

		public int ChildCount => Childs.Count;

		public int TargetCount;

		[JsonIgnore]
		public CombatLogNode Parent
		{
			get => _parent;
			set => _parent = value;
		}

		[JsonIgnore]
		public CombatLogNode LastLog => _lastLog;

		[JsonIgnore] private List<string> _detailLogList;

//		[JsonIgnore]
//		protected bool _isLogged = false;
//		public bool IsLogged => _isLogged;

//		protected DungeonLogType _logType;
		public CombatLogNode()
		{
			_lastLog = this;
			Childs = new List<CombatLogNode>();
			_detailLogList = new List<string>();
		}
		
		public void GetCombatLogNodesInChildren<T>(List<T> nodes, int depth = 0) where T : CombatLogNode
		{
			if (nodes == null || depth > 10)
				return;

			if (this is T)
			{
				nodes.Add(this as T);
				return;
			}

			if (Childs == null) return;
			
			foreach (var node in Childs)
			{
				if (node == null || node == this)
					continue;
				
				node.GetCombatLogNodesInChildren<T>(nodes, depth + 1);
			}
		}
		
		public void GetCombatLogNodesInParents<T>(List<T> nodes) where T : CombatLogNode
		{
			if (nodes == null) { return; }

			if (this is T)
			{
				nodes.Add(this as T);
				return;
			}

			Parent?.GetCombatLogNodesInParents<T>(nodes);
		}
		
		public void AddChild(CombatLogNode child)
		{
			Childs.Add(child);
			child.Parent = this;
			_lastLog = child;
		}

		
		public void Clear()
		{
			Childs.Clear();
		}
		
		public override bool Serialize(IPacketWriter writer)
		{
	        
			if (base.Serialize(writer) == false)
			{
				return false;
			}
			
	        var count = Childs.Count;
	        writer.Write(count);

	        foreach(var curValue in Childs)
	        {
		        if (curValue == null)
		        {
			        continue;
		        }

		        curValue.Serialize(writer);
	        }

			return true;
		}

		public override void OnSerialize(IPacketWriter writer)
		{
			var classType = GetClassType();
	        writer.Write(classType);
		}

		public override ICorgiSerializable DeSerialize(IPacketReader reader)
		{
			if (base.DeSerialize(reader) == null)
			{
				return null;
			}
			
			int count = 0;

			reader.Read(out count);

			for (int i = 0; i < count; i++)
			{
				var instance = (CombatLogNode)CombatLogNode.DeSerializeStatic(reader);

				if (instance != null)
				{
					Childs.Add(instance);
				}
			}

			return this;
		}

		public static ICorgiSerializable DeSerializeStatic(IPacketReader reader)
		{
			 CombatLogNodeType classType;
			 reader.ReadEnum(out classType);
			 
			 var instance = Create(classType);

			 if (instance == null)
			 {
				 if (classType != CombatLogNodeType.None)
				 {
					CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid CombatLogNode : {0}", (int)classType);
				 }
				 
				 return null;
			 }
			 if (instance.DeSerialize(reader) != null)
			 {
				 return instance;
			 }

			 return null;
		}

		public static CombatLogNode Create(CombatLogNodeType logNodeType)
		{
			//CorgiCombatLog.Log(CombatLogCategory.Skill,"[Serialize] Create CombatLogNode : {0}", logNodeType);
	        switch (logNodeType)
	        {
		        case CombatLogNodeType.Stage:
				    return new StageLogNode();
		        case CombatLogNodeType.Tick:
				    return new TickLogNode();
		        case CombatLogNodeType.Moving:
				    return new UnitMovingLogNode();
		        
		        case CombatLogNodeType.PartyMemberDungeonEnter:
			        return new PartyMemberDungeonEnterLogNode();
		        case CombatLogNodeType.CumulativeDamage:
			        return new CumulativeDamageLogNode();
		        
		        case CombatLogNodeType.SkillAction:
				    return new SkillActionLogNode();
		        case CombatLogNodeType.SkillEvent:
				    return new SkillEventLogNode();
		        case CombatLogNodeType.SkillPassive:
				    return new SkillPassiveLogNode();
		        case CombatLogNodeType.SkillInvokeAction:
				    return new SkillInvokeActionLogNode();
		        case CombatLogNodeType.SkillActionCancelCasting:
				    return new CancelCastingLogNode();
		        case CombatLogNodeType.SkillActionCompleteCasting:
				    return new CompleteCastingLogNode();
		        case CombatLogNodeType.SkillActionChannelingTick:
				    return new ChannelingTickLogNode();
		        
		        case CombatLogNodeType.RelicSkillAction:
				    return new RelicActionLogNode();
		        
		        case CombatLogNodeType.Damage:
				    return new DamageSkillCompLogNode();
		        case CombatLogNodeType.Heal:
				    return new HealSkillCompLogNode();
			    case CombatLogNodeType.Dispel:
				    return new DispelSkillCompLogNode();
			    case CombatLogNodeType.Interrupt:
				    return new InterruptSkillCompLogNode();
			    case CombatLogNodeType.AddMana:
				    return new AddManaSkillCompLogNode();
			    case CombatLogNodeType.RestoreMana:
				    return new RestoreManaSkillCompLogNode();
			    case CombatLogNodeType.ConsumeMana:
				    return new ConsumeManaSkillCompLogNode();
			    case CombatLogNodeType.AbsorbMana:
				    return new AbsorbManaSkillCompLogNode();
			    case CombatLogNodeType.RestoreNextMana:
				    return new RestoreNextManaSkillCompLogNode();
			    case CombatLogNodeType.DecreaseManaCost:
				    return new DecreaseManaCostSkillCompLogNode();
			    
			    case CombatLogNodeType.ChangeCurHPSelf:
				    return new ChangeCurHPPercentSelfLogNode();
			    
			    case CombatLogNodeType.Summon:
				    return new SummonLogNode();
			    
			    case CombatLogNodeType.AddContinuous:
				    return new AddContinuousLogNode();
			    case CombatLogNodeType.RemoveContinuous:
				    return new RemoveContinuousLogNode();
			    case CombatLogNodeType.AbsorbContinuous:
				    return new AbsorbContinuousSkillCompLogNode();
			    
			    case CombatLogNodeType.PassiveSkillComp:
				    return new PassiveSkillCompLogNode();
			    case CombatLogNodeType.AbsorbDamage:
				    return new AbsorbDamageLogNode();
			    case CombatLogNodeType.MezPassive:
				    return new MezPassiveSkillCompLogNode();
			    case CombatLogNodeType.SaveFromDeath:
				    return new SaveFromDeathPassiveSkillCompLogNode();
			    case CombatLogNodeType.TransferDamage:
				    return new TransferDamagePassiveSkillCompLogNode();
			    case CombatLogNodeType.ConvertToTrueDamage:
				    return new ConvertToTrueDamagePassiveSkillCompLogNode();
			    case CombatLogNodeType.Vampiric:
				    return new VampiricPassiveSkillCompLogNode();
			    case CombatLogNodeType.ReflectDamage:
				    return new ReflectDamagePassiveSkillCompLogNode();
			    case CombatLogNodeType.BarrierPassive:
				    return new BarrierPassiveSkillCompLogNode();
			    case CombatLogNodeType.Die:
				    return new DieLogNode();
			    case CombatLogNodeType.None:
				    return null;
	        }
	        return null;
		}

		public override void LogDebug(IGameDataBridge bridge)
	    {
		    foreach (var curChild in Childs)
		    {
			    curChild?.LogDebug(bridge);
		    }
	    }
	    
	    public List<CombatLogData> GetLogs(IGameDataBridge bridge, ref int index)
	    {
		    var logDataList = new List<CombatLogData>();
		    this.GetLog(bridge, ref index, logDataList);

		    return logDataList;
	    }

	    public virtual void GetLog(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
		    this.GetLogInner(bridge, ref index, logDataList);
		    
		    foreach (var curLogNode in Childs)
		    {
			    curLogNode?.GetLog(bridge, ref index, logDataList);
		    }
	    }

	    protected virtual void GetLogInner(IGameDataBridge bridge, ref int index, List<CombatLogData> logDataList)
	    {
	    }

	    public string GetDetailLog()
	    {
		    if (_detailLogList.Count == 0)
		    {
			    return string.Empty;
		    }


		    return String.Concat(_detailLogList);
	    }

	    public void AddDetailLog(string log)
	    {
		    _detailLogList.Add(log + "\n");
	    }

	    public virtual void SetRoot(DungeonLogNode dungeonLogNode)
	    {
		    DungeonLogNode = dungeonLogNode;
		    foreach (var child in Childs)
		    {
			    if (child == null)
			    {
				    continue;
			    }
			    child.SetParent(this);
			    child.SetRoot(dungeonLogNode);
		    }
	    }
	    
	    public void SetParent(CombatLogNode parentLogNode)
	    {
		    Parent = parentLogNode;
	    }
	}
}