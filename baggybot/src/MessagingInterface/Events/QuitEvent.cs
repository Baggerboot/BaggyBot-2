namespace BaggyBot.MessagingInterface.Events
{
	public class QuitEvent
	{
		public QuitEvent(ChatClient client, ChatUser user, string reason)
		{
			Client = client;
			User = user;
			Reason = reason;
		}

		public ChatClient Client { get; }

		public ChatUser User { get; }

		public string Reason { get; }
	}
}