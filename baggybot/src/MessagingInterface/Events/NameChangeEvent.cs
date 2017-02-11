namespace BaggyBot.MessagingInterface.Events
{
	public class NameChangeEvent
	{
		public ChatUser OldName { get; }
		public ChatUser NewName { get; }

		public NameChangeEvent(ChatUser oldName, ChatUser newName)
		{
			OldName = oldName;
			NewName = newName;
		}
	}
}