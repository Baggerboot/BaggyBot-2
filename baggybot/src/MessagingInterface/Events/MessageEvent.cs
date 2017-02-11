using System;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface.Events
{
	public class MessageEvent
	{
		public ChatMessage Message { get; }
		internal Func<string, MessageSendResult> ReplyCallback { get; set; }
		internal Func<string, MessageSendResult> ReturnMessageCallback { get; set; }

		public MessageEvent(ChatMessage message, Func<string, MessageSendResult> replyCallback, Func<string, MessageSendResult> returnMessageCallback)
		{
			Message = message;
			ReplyCallback = replyCallback;
			ReturnMessageCallback = returnMessageCallback;
		}
	}
}