using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using Corgi.DBSchema;
using IdleCs.Library;
using Corgi.GameData;
using Google.Protobuf;
using IdleCs.Utils;
using IdleCs.GameLog;
using IdleCs.GameLogic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Managers;
using Newtonsoft.Json.Linq;

//using HutongGames.PlayMaker;

//
namespace IdleCs.GameLogic
{
	/// <summary>
	/// Skill 의 Base Class
	/// </summary>
    public class Skill : CorgiCombatObject,ISkillActor, ICorgiInterface<SkillInfoSpec>
    {
	    /// <summary>
	    /// Static
	    /// </summary>
        private Unit _owner;

	    private SkillInfoSpec _spec;

	    public Unit Owner
        {
            get { return _owner; }
        }
	    public uint Level { get; protected set; }

	    public ulong SkillBaseUid
	    {
		    get {
			    if (_spec.BaseUid == 0)
			    {
				    return _spec.Uid;
			    }

			    return _spec.BaseUid;
		    }
	    }

	    public ulong SkillUid
	    {
		    get { return _spec.Uid; }
	    }
	    
	    
	    public SkillActorType SkillActorType
	    {
		    get { return SkillActorType.Skill; }
	    }

	    public virtual SkillActionType GetSkillActionType()
	    {
		    return SkillActionType.None;
	    }
	    
	    public uint StackCount
	    {
		    get { return 1; }
	    }

	    private uint _mastery;
	    public uint Mastery
	    {
		    get => _mastery;
	    }
	    
	    public SkillInfoSpec GetSpec()
	    {
		    return _spec;
	    }
	    
	    /// <summary>
	    /// Active Components
	    /// </summary>
        protected List<ArraySkillCompInfo> ArrayList = new List<ArraySkillCompInfo>();
        protected List<ActiveSkillCompInfo> ActiveList = new List<ActiveSkillCompInfo>();
        protected List<EffectSkillCompInfo> EffectList = new List<EffectSkillCompInfo>();
        
        /// <summary>
        /// target component
        /// </summary>
		public TargetSkillTargetComp SkillTargetComp { get; protected set; }

	    /// <summary>
	    /// Skill Data
	    /// </summary>
	    
	    public string Name
	    {
		    get { return _spec.Name; }
	    }

	    //public string IconName { get; protected set; }
	    
	    
	    public SkillType SkillType { get; protected set; }
	    public EquipGradeType GradeType { get; protected set; }
	    public SkillAttributeType AttributeType { get; protected set; }

	    public ulong CastingTime { get; protected set; }
	    public ulong ChannelingTick { get; protected set; }
	    //public int ManaCost { get; protected set; }
	    private int _manaCost;
		private int _chanagedManaCost;

	    public int GetManaCost()
	    {
		    return _manaCost + _chanagedManaCost;
	    }
	    
	    public void SetChanagedManaCost(int changedManaCost)
	    {
		    _chanagedManaCost = changedManaCost;
	    }

	    /// <summary>
	    /// Dynamic Data
	    /// </summary>
	    ///
	    ///
	    
	    public Skill() 
	    {
		    EventManager = new EventManager(CombatEventCategory.Action);
	    }

	    public void Init(Unit owner)
	    {
		    _owner = owner;
	    }

	    protected override bool LoadInternal(IMessage dbObject)
	    {
		    var db = dbObject as DBSkill;
		    if (db == null)
		    {
			    return false;
		    }

		    DBId = db.Dbid;
		    
			var skillItemInfo = Owner.Dungeon.GameData.GetData<SkillItemSpec>(db.Uid);

			if (skillItemInfo == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid skill item : {0}", db.Uid);
				return false;
			}
		    Uid = skillItemInfo.SkillUid;
		    Level = db.Level;
		    
		    return LoadInternal(Uid);
	    }
	    
	    protected override bool LoadInternal(CorgiSharedObject sObject)
	    {
		    var sharedObject = sObject as SharedSkillInfo;
		    
		    if (sharedObject == null)
		    {
			    return false;
		    }
		    
			var skillItemInfo = Owner.Dungeon.GameData.GetData<SkillItemSpec>(sObject.uid);

			if (skillItemInfo == null)
			{
				CorgiCombatLog.LogError(CombatLogCategory.Skill, "invalid skill item : {0}", sObject.uid);
				return false;
			}

		    Uid = skillItemInfo.SkillUid;
		    Level = sharedObject.level;
		    
		    return LoadInternal(Uid);
	    }

	    protected override bool LoadInternal(ulong uid)
	    {
		    if (base.LoadInternal(uid) == false)
		    {
			    return false;
		    }
		    
		    var spec = Owner.Dungeon.GameData.GetData<SkillInfoSpec>(uid);
		    if (spec == null)
		    {
			    return false;
		    }

		    _spec = spec;

		    //IconName = spec.icon;
		    SkillType = CorgiEnum.ParseEnum<SkillType>(spec.SkillType);
		    GradeType = spec.SkillGrade;
		    AttributeType = spec.SkillAttribute;
		    
		    CastingTime = spec.CastingTime;
		    _manaCost = (int)spec.ManaCost;

		    if (Level == 0)
		    {
			    // if no initialize, initialize 1
			    Level = 1;
		    }

		    //CorgiLog.LogLine("Load Skill({0}) for {1}", spec.Name, _owner.Name);
		    
		    return true;
	    }

	    public void SetLevel(uint level)
	    {
		    if (level > 100000)
		    {
			    CorgiCombatLog.LogError(CombatLogCategory.Skill, "[Skill] ({0}) invalid skill level {1}", Uid, level);
			    return;
		    }
		    Level = level;
	    }

	    // called by tallented Skill
	    public void AddLevel()
	    {
		    Level += 1;
	    }

	    protected override void Tick(ulong deltaTime, TickLogNode logNode)
	    {
	    }

	    /// <summary>
	    /// Skill 진입점.
	    /// should override
	    /// </summary>
        public virtual SkillActionLogNode DoSkill(Unit target)
        {
            // todo override

	        return null;
        }
	    
	    /// <summary>
	    /// ArraySkillComp 실행 Method
	    /// target -> doApply
	    /// </summary>
	    protected void InvokeSkillComp(List<ArraySkillCompInfo> skillCompList, SkillActionLogNode logNode)
	    {
		    //int count = 1;
			foreach (var arrayCompInfo in skillCompList)
			{
				if (arrayCompInfo == null)
				{
					continue;
				}

				logNode.AddDetailLog($"Invoke ArraySkillComp : {Owner.Dungeon.GameData.GetStrByUid(arrayCompInfo.ArrayCompUid)}");
				arrayCompInfo.Invoke(logNode);
				//count++;
			}
	    }

	    /// <summary>
	    /// ActiveSkillComp 실행 Method
	    /// target -> doApply
	    /// </summary>
	    protected void InvokeSkillComp(List<ActiveSkillCompInfo> skillCompList, SkillActionLogNode logNode)
	    {
			foreach (var activeCompInfo in skillCompList)
			{
				if (activeCompInfo == null)
				{
					continue;
				}

				logNode.AddDetailLog($"Invoke ActiveSkillComp : {Owner.Dungeon.GameData.GetStrByUid(activeCompInfo.SkillComp.Uid)}");
				activeCompInfo.Invoke(logNode);
			}

			foreach (var curLogNode in logNode.Childs)
			{
				var skillCompLogNode = curLogNode as SkillCompLogNode;
				if (skillCompLogNode == null)
				{
					continue;
				}

				if (skillCompLogNode.IsImmune)
				{
					logNode.ResultType = SkillResultType.SkillResultImmune;
				}
			}
	    }
	    
	    /// <summary>
	    /// EffectSkillComp 실행 Method
	    /// target -> doApply
	    /// </summary>
	    protected void InvokeSkillComp(List<EffectSkillCompInfo> skillCompList, SkillActionLogNode logNode)
	    {
		    //int count = 1;
			foreach (var effectCompInfo in skillCompList)
			{
				if (effectCompInfo == null)
				{
					continue;
				}

				logNode.AddDetailLog($"Invoke ContinuousSkillComp : {Owner.Dungeon.GameData.GetStrByUid(effectCompInfo.SkillComp.Uid)}");
				effectCompInfo.Invoke(logNode);
				//count++;
			}
	    }

	    public void ResetManaCost()
	    {
		    _manaCost = 0;
	    }

	    public void SetMastery(uint mastery)
	    {
		    _mastery = mastery;
	    }
    }
}

