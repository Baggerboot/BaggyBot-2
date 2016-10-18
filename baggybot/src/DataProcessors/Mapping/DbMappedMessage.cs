using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

namespace BaggyBot.DataProcessors.Mapping
{
	class DbMappedMessage
	{
		public Plugin Client { get; }
		public ChatChannel Channel { get; }
		public DbMappedUser Sender { get; }
		public string Message { get; }
		public bool Action { get; }


		public DbMappedMessage(Plugin client, DbMappedUser sender, ChatChannel channel, string message, bool action = false)
		{
			Client = client;
			Sender = sender;
			Channel = channel;
			Message = message;
			Action = action;
		}

		public MessageSendResult Reply(string message)
		{
			message = Sender.Nickname + ", " + message;
			return Client.SendMessage(Channel, message);
		}

		public MessageSendResult ReturnMessage(string message)
		{
			return Client.SendMessage(Channel, message);
		}
	}
}
