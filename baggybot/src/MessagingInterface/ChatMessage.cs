using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public class ChatMessage
	{
		public Plugin Client { get; }
		public ChatChannel Channel { get; }
		public ChatUser Sender { get; }
		public string Message { get; set; }
		public bool Action { get; }


		public ChatMessage(Plugin client, ChatUser sender, ChatChannel channel, string message, bool action = false)
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
