using System.Collections;
using System.Collections.Generic;
using System;
using IdleCs.Utils;
using IdleCs.GameLogic;
using IdleCs.Managers;

namespace IdleCs.GameLog
{
//	public struct SctLog
//	{
//		public static readonly SctLog Null = new SctLog(null, SctMessageInfo.Null, 0);
//
//		public string TargetId;
//		public int StatusEffectNumber;
//		public SctMessageInfo SctMsgInfo;
//		
//		public bool IsNull
//		{
//			get { return SctMsgInfo.IsNull || String.IsNullOrEmpty(TargetId); }
//		}
//
//		public SctMessageType SctMsgType
//		{
//			get { return SctMsgInfo.SctMessageType; }
//		}
//		
//		public SctLog(string targetId, SctMessageInfo info, int statusEffectNumber)
//		{
//			TargetId = targetId;
//			SctMsgInfo = info;
//			StatusEffectNumber = statusEffectNumber;
//		}
//		
//		public override string ToString()
//		{
//			return CorgiString.Format("SctLog : {0}, {1}", TargetId, SctMsgInfo);
//		}
//	}
	
	public struct SctMessageInfo
	{
		public static readonly SctMessageInfo Null = new SctMessageInfo(SctMessageType.None);
		
		
		public SctMessageType SctMessageType { get; private set; }
		public string MainMessage { get; private set; }
//		public float LifeTime { get; private set; }
//		public int FontSize { get; private set; }
//		private string CasterId { get; set; }
		public string IconName { get; set; }
		public string RelicIconName { get; set; }
		
		
		public bool IsNull => SctMessageType == SctMessageType.None;

		
		public SctMessageInfo(SctMessageType messageType, bool isMiss = false, bool isImmune = false, string mainMessage = null, string subMessage = null) : this()
		{
			if (isImmune)
			{
				messageType = SctMessageType.Immune;
				mainMessage = "";
				subMessage = "";
			}
			else if (isMiss)
			{
				messageType = SctMessageType.Miss;
				mainMessage = "";
				subMessage = "";
			}
			
			SctMessageType = messageType;
//			CasterId = null;
			IconName = null;
			RelicIconName = null;
			MainMessage = string.IsNullOrEmpty(mainMessage) ? GetMainMessage(SctMessageType) : mainMessage;
			MainMessage += subMessage;
//			LifeTime = 2f;
		}
		
		public SctMessageInfo(MezType mezType) : this()
		{
			switch (mezType)
			{
				case MezType.Stun:
					SctMessageType = SctMessageType.Stun;
					break;
				case MezType.Sleep:
					SctMessageType = SctMessageType.Sleep;
					break;
				case MezType.Silence:
					SctMessageType = SctMessageType.Silence;
					break;
				
			    case MezType.Fortify:
					SctMessageType = SctMessageType.Fortify;
				    break;
			    case MezType.Exausted:
					SctMessageType = SctMessageType.Exausted;
				    break;
				default:
					SctMessageType = SctMessageType.Info;
					break;
			}
//			CasterId = null;
			IconName = null;
			RelicIconName = null;
			MainMessage = GetMainMessage(SctMessageType);
//			LifeTime = 2f;
		}

		private string GetMainMessage(SctMessageType sctMessageType)
		{
			var messageKey = $"lang.sct.text.{sctMessageType}.name";
			var defaultValue = sctMessageType.ToString();
			
			return TextDataManager.Instance.GetText(messageKey, defaultValue);;
		}

		public override string ToString()
		{
			return CorgiString.Format("Sct : {0}, {1}", SctMessageType, MainMessage);
		}
	}

}