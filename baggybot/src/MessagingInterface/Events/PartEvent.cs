namespace BaggyBot.MessagingInterface.Events
{
	public class PartEvent
	{
		public PartEvent(ChatClient client, ChatUser user, ChatChannel channel)
		{
			Client = client;
			User = user;
			Channel = channel;
		}

		public ChatClient Client { get; }

		public ChatUser User { get; }

		public ChatChannel Channel { get; }
	}
}