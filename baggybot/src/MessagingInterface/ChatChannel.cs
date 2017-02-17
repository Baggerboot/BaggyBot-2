using System.Collections.Generic;

namespace BaggyBot.MessagingInterface
{
	public class ChatChannel
	{
		public string Identifier { get; }
		public string Name { get; }
		public bool IsPrivateMessage { get; }

		public List<ChatUser> Users { get; } = new List<ChatUser>();

		public ChatChannel(string identifier, bool isPrivateMessage = false)
		{
			IsPrivateMessage = isPrivateMessage;
			Identifier = identifier;
			Name = identifier;
		}
		public ChatChannel(string identifier,string name, bool isPrivateMessage = false)
		{
			IsPrivateMessage = isPrivateMessage;
			Name = name;
			Identifier = identifier;
		}

		public override string ToString() => $"{Name} ({Identifier}) {(IsPrivateMessage ? "(private message)" : "")}";
	}
}
