using System;

namespace BaggyBot.MessagingInterface
{
	public class ChatMessage
	{
		public ChatChannel Channel { get; }
		public ChatUser Sender { get; }
		public string Body { get; }
		public bool Action { get; }
		public DateTime SentAt { get; }
		public object State { get; }

		public ChatMessage(DateTime sentAt, ChatUser sender, ChatChannel channel, string body, bool action = false, object state = null)
		{
			SentAt = sentAt;
			Sender = sender;
			Channel = channel;
			Body = body;
			Action = action;
			State = state;
		}

		public ChatMessage Edit(string body)
		{
			return new ChatMessage(SentAt, Sender, Channel, body, Action);
		}

		public override string ToString() => $"[{Channel.Name}] {Sender.AddressableName}: {Body}";
	}
}
