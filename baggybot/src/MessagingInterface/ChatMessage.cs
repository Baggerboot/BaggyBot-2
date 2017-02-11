using System;
using System.Xml.Serialization.Configuration;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public class ChatMessage
	{
		public ChatChannel Channel { get; }
		public ChatUser Sender { get; }
		// TODO: rename to MessageText/Text/Body
		public string Body { get; }
		public bool Action { get; }
		public DateTime SentAt { get; }

		public ChatMessage(DateTime sentAt, ChatUser sender, ChatChannel channel, string body, bool action = false)
		{
			SentAt = sentAt;
			Sender = sender;
			Channel = channel;
			Body = body;
			Action = action;
		}

		public ChatMessage Edit(string body)
		{
			return new ChatMessage(SentAt, Sender, Channel, body, Action);
		}
	}
}
