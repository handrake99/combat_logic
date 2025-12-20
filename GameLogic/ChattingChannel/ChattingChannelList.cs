using System;
using System.Collections.Generic;
using System.Linq;
using IdleCs.Network.NetLib;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    [Serializable]
    public class ChattingChannelList : CorgiSerializable
    {
        private int _maxUserCountPerChannel = 0;
        private ChattingType _channelType;
        
        private CorgiList<ChattingChannel> _channels = new CorgiList<ChattingChannel>();
        
        public CorgiList<ChattingChannel> Channels => _channels;
        
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
        
            if (null == _channels)
            {
                return false;
            }

            writer.WriteEnum(_channelType);
            _channels.Serialize(writer);
            return true;
        }
        
        
        public ICorgiSerializable DeSerialize(IPacketReader reader)
        {
            if (base.DeSerialize(reader) == null)
            {
                return null;
            }
        
            if (null == _channels)
            {
                return null;
            }

            reader.ReadEnum(out _channelType);
            _channels.DeSerialize(reader);
            return this;
        }

        public bool Initialize(ChattingType channelType, int maxChannelCount, int maxUserCountPerChannel, int maxMessageCount)
        {
            _channelType = channelType;
            _maxUserCountPerChannel = maxUserCountPerChannel;

            if (channelType == ChattingType.General)
            {
                // initialize channels
                for (int id = 0; id < maxChannelCount; ++id)
                {
                    var channel = new ChattingChannel();
                    var channelKey = $"channel_general_{id}";
                    
                    channel.Initialize(channelType, channelKey, id, maxMessageCount);
                    _channels.Add(channel);
                }
            }
            else
            {
                var channel = new ChattingChannel();
                var channelKey = $"channel_league_{maxChannelCount}";
                
                channel.Initialize(channelType, channelKey, maxChannelCount, maxMessageCount);
                _channels.Add(channel);
                
            }
            
            return (0 < _channels.Count);
        }

        public ChattingChannel FindInitialChannel()
        {
            // 본섭 캐릭터
            var find = _channels.FirstOrDefault(channel => {
                if (_maxUserCountPerChannel == -1 || _maxUserCountPerChannel > channel.CurrentUser())
                {
                    return true;
                }
                return false;
            });

            return find;
        }

        public ChattingChannel GetChannel(int channelIndex)
        {
            foreach (var thisChannel in _channels)
            {
                if (thisChannel != null && thisChannel.Index == channelIndex)
                {
                    return thisChannel;
                }
            }

            return null;
        }
        
        public void TestLog()
        {
            _channels.ForEach((element) => {
                CorgiCombatLog.Log(CombatLogCategory.Chatting,"channel id : {0}, user count : {1}", element.Index, element.CurrentUser());
            });
            
        }

    }
}