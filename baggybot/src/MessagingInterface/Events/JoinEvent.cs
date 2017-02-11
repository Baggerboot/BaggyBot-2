namespace BaggyBot.MessagingInterface.Events
{
	public class JoinEvent
	{
		public ChatUser User { get; }
		public ChatChannel Channel { get; }

		public JoinEvent(ChatUser user, ChatChannel channel)
		{
			User = user;
			Channel = channel;
		}
	}
}