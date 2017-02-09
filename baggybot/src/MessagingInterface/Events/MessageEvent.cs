using System;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface.Events
{
	public class MessageEvent
	{
		public MessageEvent(ChatClient client, ChatMessage message, Func<string, MessageSendResult> replyCallback, Func<string, MessageSendResult> returnMessageCallback)
		{
			Client = client;
			Message = message;
			Message.ReplyCallback = replyCallback;
			Message.ReturnMessageCallback = returnMessageCallback;
		}

		private ChatClient Client { get; }

		public ChatMessage Message { get; }
	}
}