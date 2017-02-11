namespace BaggyBot.MessagingInterface.Events
{
	public class KickedEvent
	{
		public ChatChannel Channel { get; }
		public ChatUser Kicker { get; }
		public string Reason { get; }

		public KickedEvent(ChatChannel channel, ChatUser kicker, string reason)
		{
			Channel = channel;
			Kicker = kicker;
			Reason = reason;
		}
	}
}