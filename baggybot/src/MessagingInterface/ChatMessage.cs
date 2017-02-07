using System;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public class ChatMessage
	{
		public ChatChannel Channel { get; }
		public ChatUser Sender { get; }
		// TODO: rename to MessageText/Text/Body
		public string Message { get; set; }
		public bool Action { get; }
		internal Func<string, MessageSendResult> ReplyCallback { get; set; }
		internal Func<string, MessageSendResult> ReturnMessageCallback { get; set; }

		public ChatMessage(ChatUser sender, ChatChannel channel, string message, bool action = false)
		{
			Sender = sender;
			Channel = channel;
			Message = message;
			Action = action;
		}
	}
}
