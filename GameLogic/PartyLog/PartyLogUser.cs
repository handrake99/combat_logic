using SteamB23.KoreanUtility.Grammar;

namespace IdleCs.GameLogic
{
    public class PartyLogUser : PartyLogMessage
    {
        public int TempIndex { get; set; }
        public PartyLogUser(){}
        public PartyLogUser(PartyLogType type, string userName)
            : base(type, userName)
        {
            TempIndex = 1;
        }
        
	    public override int GetClassType()
        {
            return (int)PartyLogCategory.User;
        }
        
        public override bool Aggregate(PartyLogMessage logMessage)
        {
            var thisLog = logMessage as PartyLogUser;
            if (thisLog == null)
            {
                return false;
            }

            if (LogType != thisLog.LogType)
            {
                return false;
            }

            if (UserName != thisLog.UserName)
            {
                return false;
            }
            
            Timestamp = thisLog.Timestamp;
            
            return true;
        }

        public override string GetLogStr(IGameDataBridge bridge)
        {
            string formatStr = null;
            switch (LogType)
            {
                case PartyLogType.Connect:
                    formatStr =bridge.GetString("lang.party.log.connect");
                    break;
                case PartyLogType.PartyJoin:
                    formatStr =bridge.GetString("lang.party.log.party_join");
                    break;
                case PartyLogType.PartyLeave:
                    formatStr =bridge.GetString("lang.party.log.party_leave");
                    break;
                case PartyLogType.PartyExile:
                    formatStr =bridge.GetString("lang.party.log.party_exile");
                    break;
            }

            if (string.IsNullOrEmpty(formatStr))
            {
                return null;
            }

            var userName = UserName;
            var resultStr = string.Format(formatStr, userName);
            resultStr = 조사.문자처리(resultStr);

            if (!string.IsNullOrEmpty(userName))
            {
                resultStr = resultStr.Replace(userName, $"<b><color=#ffffffff>{userName}</color></b>");
            }
            return resultStr;
        }
    }
}