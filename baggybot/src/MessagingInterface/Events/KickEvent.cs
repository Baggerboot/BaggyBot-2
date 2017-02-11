namespace BaggyBot.MessagingInterface.Events
{
	public class KickEvent
	{
		public ChatUser Kickee { get; }
		public ChatChannel Channel { get; }
		public ChatUser Kicker { get; }
		public string Reason { get; }

		public KickEvent(ChatUser kickee, ChatChannel channel, ChatUser kicker, string reason)
		{
			Kickee = kickee;
			Channel = channel;
			Kicker = kicker;
			Reason = reason;
		}
	}
}