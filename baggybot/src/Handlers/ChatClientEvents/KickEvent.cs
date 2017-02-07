using BaggyBot.MessagingInterface;

namespace BaggyBot.Handlers.ChatClientEvents
{
	public class KickEvent
	{
		public KickEvent(ChatClient client, ChatUser kickee, ChatChannel channel, ChatUser kicker, string reason)
		{
			Client = client;
			Kickee = kickee;
			Channel = channel;
			Kicker = kicker;
			Reason = reason;
		}

		public ChatClient Client { get; }

		public ChatUser Kickee { get; }

		public ChatChannel Channel { get; }

		public ChatUser Kicker { get; }

		public string Reason { get; }
	}
}