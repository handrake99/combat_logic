using Google.Protobuf;
using Corgi.DBSchema;
using IdleCs.GameLogic.SharedInstance;
using UnityEngine.PlayerLoop;


namespace IdleCs.GameLogic
{
    public class SkillSlot : CorgiObject
    {
        private Unit _owner;
        public uint Mastery;
        
        protected override bool LoadInternal(IMessage dbObject)
        {
            var db = dbObject as DBSkillSlot;
            if (db == null)
            {
                return false;
            }

            DBId = db.Dbid;
            Mastery = db.Mastery;

            return true;
        }
        
        protected override bool LoadInternal(CorgiSharedObject sObject)
        {
            var sharedObject = sObject as SharedSkillSlot;
            if (sharedObject == null)
            {
                return false;
            }

            DBId = sharedObject.dbId;
            Mastery = sharedObject.mastery;

            return true;
        }

        private void Init(Unit owner)
        {
            _owner = owner;
        }
        
        public static SkillSlot Create(DBSkillSlot dbSkill, Unit owner)
        {
            if (dbSkill == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid db skillSlot : null");
                return null;
            }

            var inst = new SkillSlot();
			
            inst.Init(owner);

            if (inst.Load(dbSkill) == false)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"failed to load skillSlot");
                return null;
            }

            return inst;
        }
        
        public static SkillSlot Create(SharedSkillSlot skillSlot, Unit owner)
        {
            if (skillSlot == null)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"invalid db skillSlot : null");
                return null;
            }

            var inst = new SkillSlot();
			
            inst.Init(owner);

            if (inst.Load(skillSlot) == false)
            {
                CorgiCombatLog.LogError(CombatLogCategory.Skill,"failed to load skillSlot");
                return null;
            }

            return inst;
        }
    }
}