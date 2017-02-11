namespace BaggyBot.MessagingInterface.Events
{
	public class PartEvent
	{
		public ChatUser User { get; }
		public ChatChannel Channel { get; }

		public PartEvent(ChatUser user, ChatChannel channel)
		{
			User = user;
			Channel = channel;
		}
	}
}