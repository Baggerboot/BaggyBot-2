using System;

namespace BaggyBot.MessagingInterface
{
	public class ChatMessage
	{
		public ChatUser Sender { get; }
		public ChatChannel Channel { get; }
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

		public override bool Equals(object obj)
		{
			var chatMessage = obj as ChatMessage;
			// Both messages must not be null, and both messages must have a body
			if (chatMessage?.Body == null || Body == null) return false;

			return string.Equals(Body, chatMessage.Body, StringComparison.Ordinal)
				&& Sender.Equals(chatMessage.Sender)
				&& Channel.Equals(chatMessage.Channel)
				&& SentAt == chatMessage.SentAt;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Channel?.GetHashCode() ?? 0;
				hashCode = (hashCode*397) ^ (Sender?.GetHashCode() ?? 0);
				hashCode = (hashCode*397) ^ (Body?.GetHashCode() ?? 0);
				hashCode = (hashCode*397) ^ SentAt.GetHashCode();
				return hashCode;
			}
		}

		public override string ToString() => $"[{Channel.Name}] {Sender.AddressableName}: {Body}";
	}
}
