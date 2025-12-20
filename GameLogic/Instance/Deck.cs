using System.Collections.Generic;
using Google.Protobuf;
using IdleCs.GameLogic.SharedInstance;

namespace IdleCs.GameLogic
{
    public class Deck 
    {
        public string CharacterId { get; set; }
        public List<SkillActive> ActiveSkills { get; private set; }
        public List<ulong> Relics { get; private set; }
        public List<SkillActive> RelicSkills { get; private set; }
        public List<SkillSlot> SkillSlots { get; private set; }

        public Deck(string characterId)
        {
            CharacterId = characterId;
            ActiveSkills = new List<SkillActive>();
            Relics = new List<ulong>();
            RelicSkills = new List<SkillActive>();
            SkillSlots = new List<SkillSlot>();
        }
    }
}