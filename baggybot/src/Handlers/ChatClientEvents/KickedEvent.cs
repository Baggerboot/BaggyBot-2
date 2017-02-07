using BaggyBot.MessagingInterface;

namespace BaggyBot.Handlers.ChatClientEvents
{
	public class KickedEvent
	{
		public KickedEvent(ChatClient client, ChatChannel channel, ChatUser kicker, string reason)
		{
			Client = client;
			Channel = channel;
			Kicker = kicker;
			Reason = reason;
		}

		public ChatClient Client { get; }

		public ChatChannel Channel { get; }

		public ChatUser Kicker { get; }

		public string Reason { get; }
	}
}