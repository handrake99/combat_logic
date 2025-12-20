using System;
using System.Collections.Generic;
using System.Linq;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
	public enum PartyLogCategory
	{
		None = 0
		, User = 1
		, Battle = 2
		, Acquire = 3
		, Party = 4
		, Rift = 5
	}
	
    public enum PartyLogType
    {
        None = 0
        , Connect = 1
        , PartyJoin = 2
        , PartyLeave = 3
        , PartyExile = 4
        
        , BattleAutoHunting = 10
        , BattleChallenge = 11
        , BattleInstance = 12
        , BattleStageLose = 13
        
        , AcquireSkill = 20
        , AcquireEquip = 21
        
        , PartyChapterRise = 30
        , PartyChapterDrop = 31
        
        , RiftOpen = 40
        , RiftJoin = 41
    }
    
    
    [Serializable]
    public class PartyLog : ICorgiSerializable
    {
		public static int PARTY_LOG_MAX_COUNT = 50;
		
        public List<PartyLogMessage> PartyLogList = new List<PartyLogMessage>();

        private int _maxMessageCount = PARTY_LOG_MAX_COUNT;
        private PartyLogMessage _lastPartyLog = null;

        public PartyLog()
        {
        }

        public void AddLog(PartyLogMessage logMessage)
        {
	        // aggregate
			AggregateLast();
			// remove count
			RemoveMaxCount();
			
			PartyLogList.Add(logMessage);
			
	        // aggregate
			AggregateLast();

			_lastPartyLog = logMessage;
        }

        void RemoveMaxCount()
        {
			var curCount = PartyLogList.Count;
			if (curCount >= _maxMessageCount)
			{
				PartyLogList.RemoveRange(0, curCount - _maxMessageCount + 1);
			}
	        
        }

        public void AggregateLast()
        {
	        var logCount = PartyLogList.Count;
	        if (logCount < 2)
	        {
		        // 2개 미만은 의미 없음.
		        return;
	        }

	        for(int i=logCount-1; i>=1 ; i--)
	        {
		        var thisLog = PartyLogList[i];
		        var preLog = PartyLogList[i - 1];

		        if (thisLog == null || preLog == null)
		        {
			        break;
		        }

		        if (preLog.Aggregate(thisLog) )
		        {
			        // remove thisLog
					PartyLogList.RemoveAt(i);
		        }
	        }
        }

        public PartyLogMessage GetCurLog()
        {
	        if (PartyLogList.Count == 0)
	        {
		        return null;
	        }
	        return _lastPartyLog;
        }

        public void LogDebug(IGameDataBridge bridge)
        {
	        CorgiCombatLog.Log(CombatLogCategory.Party,"PartyLog *** List Start *** ");
	        foreach (var curLog in PartyLogList)
	        {
		        if (curLog == null)
		        {
			        continue;
		        }

		        var logStr = curLog.GetLogStr(bridge);
		        
		        CorgiCombatLog.Log(CombatLogCategory.Party,"PartyLog " + logStr);
	        }
	        CorgiCombatLog.Log(CombatLogCategory.Party,"PartyLog *** List End *** ");
        }
	    public int GetClassType()
	    {
		    throw new NotImplementedException();
	    }

        public bool Serialize(IPacketWriter writer)
        {
	        var count = PartyLogList.Count;
	        writer.Write(count);

	        foreach(var curValue in PartyLogList)
	        {
		        if (curValue == null)
		        {
			        continue;
		        }
	        
		        curValue.Serialize(writer);
	        }

	        return true;
        }
		public void OnSerialize(IPacketWriter writer)
		{
		}
        
        public ICorgiSerializable DeSerialize(IPacketReader reader)
        {
	        try
	        {
		        int count = 0;

		        reader.Read(out count);

		        for (int i = 0; i < count; i++)
		        {
			        PartyLogCategory classType;
			        reader.ReadEnum(out classType);

			        var instance = PartyLogMessage.Create(classType);

			        if (instance == null)
			        {
				        CorgiCombatLog.LogError(CombatLogCategory.Party, "invalid Party Log : {0}", (int)classType);
				        continue;
			        }

			        if (instance.DeSerialize(reader) != null)
			        {
						PartyLogList.Add(instance);
			        }
		        }
	        }
	        catch (Exception e)
	        {
		        CorgiCombatLog.LogError(CombatLogCategory.Party, e);
		        PartyLogList.Clear();
		        return this;
	        }
	        
	        AggregateLast();
	        RemoveMaxCount();
	        
	        return this;
        }
    }
}