using System;
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

		public override bool Equals(object obj)
		{
			var channel = obj as ChatChannel;
			if (channel?.Identifier == null || Identifier == null) return false;
			return string.Equals(Identifier, channel.Identifier, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return Identifier?.GetHashCode() ?? 0;
		}

		public override string ToString() => $"{Name} ({Identifier}) {(IsPrivateMessage ? "(private message)" : "")}";
	}
}
