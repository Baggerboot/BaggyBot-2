using System;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

namespace BaggyBot.Handlers.ChatClientEvents
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