namespace BaggyBot.MessagingInterface.Events
{
	public class NameChangeEvent
	{
		public NameChangeEvent(ChatClient client, ChatUser oldName, ChatUser newName)
		{
			Client = client;
			OldName = oldName;
			NewName = newName;
		}

		public ChatClient Client { get; }

		public ChatUser OldName { get; }

		public ChatUser NewName { get; }
	}
}