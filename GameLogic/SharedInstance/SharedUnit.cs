using System;
using System.Collections.Generic;
using Corgi.GameData;
using IdleCs.Network.NetLib;
using IdleCs.Utils;
using Newtonsoft.Json.Converters;

namespace IdleCs.GameLogic.SharedInstance
{
    public abstract class SharedUnit : CorgiSharedObject
    {
        public UnitState UnitState;

        public uint Level;
        public CorgiPosition InitialPosition;
        public CorgiPosition Position;
        public CorgiPosition Direction;
        public long CurHP;
        public long MaxHP;
        public long CurBarrier;
        public long CurMana;
        public long MaxMana;

        public CorgiList<SharedSkill> Skills;
        public int CurSkillIndex;

        public CorgiList<SharedSkillEffectInst> ContinuousList;

        public SharedUnit()
        {
        }
        
        public bool IsLive()
        {
            return (MaxHP > 0 && CurHP > 0);
        }

        public override void Init(CorgiObject original)
        {
            var owner = original as Unit;
            if (owner == null)
            {
                throw new CorgiException("failed to initailize SharedUnit");
            }
            base.Init(owner);

            UnitState = owner.UnitState;
            
            Level = owner.Level;
            InitialPosition = new CorgiPosition(owner.InitialPosition);
            Position = new CorgiPosition(owner.Position);
            CurHP = owner.CurHP;
            MaxHP = owner.MaxHP;

            CurBarrier = owner.CurBarrier;

            CurMana = owner.CurMana;
            MaxMana = owner.MaxMana;

            Skills = new CorgiList<SharedSkill>();
            foreach (var curSkill in owner.ActiveSkills)
            {
                if (curSkill == null)
                {
                    continue;
                }
                
                var sharedSkill = new SharedSkill();
                sharedSkill.Init(curSkill);
                Skills.Add(sharedSkill);
            }

            CurSkillIndex = owner.CurSkillIndex;
            
            ContinuousList = new CorgiList<SharedSkillEffectInst>();
            
            foreach (var curInst in owner.ContinuousList)
            {
                if (curInst == null)
                {
                    continue;
                }
                
                var sharedInst = new SharedSkillEffectInst();
                sharedInst.Init(curInst);
                ContinuousList.Add(sharedInst);
            }
        }

        public virtual string GetName(IGameDataBridge bridge)
        {
            return string.Empty;
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.WriteEnum(UnitState);
            
            InitialPosition.Serialize(writer);
            Position.Serialize(writer);
            
            writer.Write(Level);
            writer.Write(CurHP);
            writer.Write(MaxHP);
            
            writer.Write(CurBarrier);
            
            writer.Write(CurMana);
            writer.Write(MaxMana);
            
            writer.Write(CurSkillIndex);
            
            Skills.Serialize(writer);
            ContinuousList.Serialize(writer);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }

            reader.ReadEnum(out UnitState);

            InitialPosition = new CorgiPosition();
            InitialPosition.DeSerialize(reader);
            Position = new CorgiPosition();
            Position.DeSerialize(reader);

            reader.Read(out Level);
            reader.Read(out CurHP);
            reader.Read(out MaxHP);
            
            reader.Read(out CurBarrier);
            
            reader.Read(out CurMana);
            reader.Read(out MaxMana);
            
            reader.Read(out CurSkillIndex);
            
            Skills = new CorgiList<SharedSkill>();
            Skills.DeSerialize(reader);
            ContinuousList = new CorgiList<SharedSkillEffectInst>();
            ContinuousList.DeSerialize(reader);
            
            return this;
        }
    }

    public class SharedCharacter : SharedUnit
    {
        public int Grade;
        public bool IsNpc;
        
        public SharedCharacter()
        {
        }
        
        public override string GetName(IGameDataBridge bridge)
        {
            var spec = bridge.GetSpec<CharacterInfoSpec>(uid);
            if (spec == null)
            {
                return string.Empty;
            }

            return spec.Name;
        }

        public override void Init(CorgiObject original)
        {
            if (original is Character)
            {
                var owner = original as Character;
                Grade = owner.Grade;
                IsNpc = false;

            }else if (original is Npc)
            {
                var owner = original as Npc;
                Grade = 1;
                IsNpc = true;
            }
            else
            {
                throw new CorgiException("failed to initailize SharedCharacter");
            }
            base.Init(original);
        }

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(Grade);
            writer.Write(IsNpc);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out Grade);
            reader.Read(out IsNpc);

            return this;
        }
        
    }
    
    public class SharedMonster : SharedUnit
    {
        public int Grade;
        public bool IsNpc;
        
        public SharedMonster()
        {
        }
        public override string GetName(IGameDataBridge bridge)
        {
            var spec = bridge.GetSpec<MonsterInfoSpec>(uid);
            if (spec == null)
            {
                return string.Empty;
            }

            return spec.Name;
        }
    }
    
    public class SharedCharacterArena : SharedMonster
    {
        public SharedCharacterArena()
        {
        }
        
        public override void Init(CorgiObject original)
        {
            if (original is CharacterArena)
            {
                var owner = original as CharacterArena;
                Grade = owner.Grade;
                IsNpc = false;

            }else if (original is Npc)
            {
                var owner = original as Npc;
                Grade = 1;
                IsNpc = true;
            }
            
            base.Init(original);
        }
        public override string GetName(IGameDataBridge bridge)
        {
            var spec = bridge.GetSpec<CharacterInfoSpec>(uid);
            if (spec == null)
            {
                return string.Empty;
            }

            return spec.Name+"_D";
        }
        
        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }
            
            writer.Write(Grade);
            writer.Write(IsNpc);

            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
            
            reader.Read(out Grade);
            reader.Read(out IsNpc);

            return this;
        }
    }
    
    public class SharedStructure : SharedUnit
    {
        public SharedStructure()
        {
        }
        public override string GetName(IGameDataBridge bridge)
        {
            return "Affix";
        }
    }
    
}