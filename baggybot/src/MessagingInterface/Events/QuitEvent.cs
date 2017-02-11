namespace BaggyBot.MessagingInterface.Events
{
	public class QuitEvent
	{
		public ChatUser User { get; }
		public string Reason { get; }

		public QuitEvent(ChatUser user, string reason)
		{
			User = user;
			Reason = reason;
		}
	}
}