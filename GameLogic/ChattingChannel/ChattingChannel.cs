using System;
using System.Threading;

using IdleCs.Network.NetLib;

namespace IdleCs.GameLogic
{
    public class ChattingChannel : CorgiSerializable
    {
        private ChattingType _channelType;
        private string _channelKey;
        private int _index;
        private int _userCount = 0;
        private int _maxMessageCount = 1000;
        
        public CorgiList<ChattingMessage> _messageList = new CorgiList<ChattingMessage>();
        public CorgiList<ChattingMessage> Messages
        {
	        get { return _messageList; }
        }

        public ulong LastMessageTimestamp { get; private set; } = 0UL;

        public ChattingType ChattingType => _channelType;
        public string ChannelKey => _channelKey;

        public int Index => _index;

        public int CurrentUser()
        {
            return _userCount;
        }
        
        public override int GetClassType()
        {
            throw new NotImplementedException();
        }

        public override bool Serialize(IPacketWriter writer)
        {
            if (base.Serialize(writer) == false)
            {
                return false;
            }

            writer.Write(_channelKey);
            writer.Write(_index);
            writer.Write(_userCount);
            writer.Write(_maxMessageCount);
            
	        _messageList?.Serialize(writer);
            
            return true;
        }

        public override ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (null == base.DeSerialize(reader))
            {
                return null;
            }

            reader.Read(out _channelKey);
            reader.Read(out _index);
            reader.Read(out _userCount);
            reader.Read(out _maxMessageCount);
            
	        _messageList?.DeSerialize(reader);

            return this;
        }

        public void Initialize(ChattingType type, string channelKey, int index, int maxCount)
        {
            _channelType = type;
            _channelKey = channelKey;
            _index = index;
            _maxMessageCount = maxCount;
            
            _userCount = 0;
        }

        public void Destroy()
        {
        }

        public void AddUser()
        {
            _userCount++;
        }

        public void RemoveUser()
        {
            if (_userCount > 0)
            {
                _userCount--;
            }
            else
            {
                _userCount = 0;
            }
        }
        
        public void AddChatting(ChattingMessage message)
        {
            if (MessageCount >= _maxMessageCount)
            {
                _messageList.RemoveAt(0);
            }
            _messageList.Add(message);
            LastMessageTimestamp = message.TimeStamp;
        }

        public int MessageCount
        {
            get { return _messageList.Count; }
        }
        
        // for connect to room.
        public ChattingChannel GetPartyChatting(ulong joinTimestamp)
        {
            var clonedPartyChatting = new ChattingChannel();

            foreach (var thisMessage in _messageList)
            {
                if (thisMessage != null && thisMessage.TimeStamp > joinTimestamp)
                {
                    clonedPartyChatting.AddChatting(thisMessage);
                }
            }

            return clonedPartyChatting;
        }
        
    }
}