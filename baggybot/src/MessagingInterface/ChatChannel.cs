using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		}
		public ChatChannel(string identifier,string name, bool isPrivateMessage = false)
		{
			IsPrivateMessage = isPrivateMessage;
			Name = name;
			Identifier = identifier;
		}
	}
}
