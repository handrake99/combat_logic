using System.Collections.Generic;
using IdleCs.GameLogic.SharedInstance;
using IdleCs.Utils;


namespace IdleCs.GameLogic
{
    public class Party 
    {
        private List<SharedMemberInfo> _memberInfos = null;
        private SharedPartyInfo _coPartyDeck;
       
        public List<SharedMemberInfo> MemberInfos => _memberInfos;

        public SharedPartyInfo CoPartyDeck => _coPartyDeck;

        private Dictionary<string, ulong> _instantJoinTimes;//-게임중 join한 멤버들의 join time.
        
        public ulong DungeonUid { get; private set; }
        public ulong StageUid { get; private set; }

        private bool _isPartyUpdated;

        public bool IsPartyUpdated => _isPartyUpdated;


        public Party()
        {
            _memberInfos = new List<SharedMemberInfo>();
            _instantJoinTimes = new Dictionary<string, ulong>();
        }
        
        public SharedMemberInfo GetMemberInfo(string charDbId)
        {
            var findMember = _memberInfos.Find(element => element.character.dbId == charDbId);
            return findMember;
        }

        public bool AddOrUpdate(SharedMemberInfo memberInfo)
        {
            if ((null == memberInfo) || (null == memberInfo.character))
            {
                CorgiCombatLog.LogError(CombatLogCategory.Party, "Party member info or character info is null when AddOrUpdate");
                return false;
            }
            
            if (null == GetMemberInfo(memberInfo.character.dbId))
            {
                _memberInfos.Add(memberInfo);
                _isPartyUpdated = true;
            }
            else
            {
                _memberInfos.RemoveAll(element => memberInfo.character.dbId == element.character.dbId);
                _memberInfos.Add(memberInfo);
            }


            return true;
        }
        
        public void RemoveMember(string charDbId)
        {
            _memberInfos.RemoveAll(element => charDbId == element.character.dbId);
            _isPartyUpdated = true;
        }

        public void OnPartyUpdated()
        {
            _isPartyUpdated = false;
        }

        public int MemberCount()
        {
            return _memberInfos.Count;
        }

        public void SetCoPartyDeck(SharedPartyInfo partyInfo)
        {
            _coPartyDeck = partyInfo;
        }

        public void SetDungeonState(ulong dungeonUid, ulong stageUid)
        {
            DungeonUid = dungeonUid;
            StageUid = stageUid;
        }

        public void SetJoinTime(string characterDbId, ulong timeStamp)
        {
            _instantJoinTimes[characterDbId] = timeStamp;
        }

        public ulong GetJoinTime(string characterDbId)
        {
            ulong time = 0;
            if (_instantJoinTimes.TryGetValue(characterDbId, out time))
            {
                return time;
            }

            return 0;
        }
    }
}