
using IRCSharp;

namespace BaggyBot.MessagingInterface
{
	public class IrcMessage : IRCSharp.IRC.IrcMessage
	{
		public IrcClientWrapper Client { get; }

		public new IrcUser Sender => (IrcUser)base.Sender;

		public IrcMessage(IrcClientWrapper clientWrapper, IrcUser sender, string channel, string message, bool action = false)
			: base(sender, channel, message, action)
		{
			Client = clientWrapper;
		}

		public IrcMessage(IrcClientWrapper clientWrapper, IRCSharp.IRC.IrcMessage message)
			:base(new IrcUser(clientWrapper, message.Sender), message.Channel, message.Message, message.Action)
		{
			Client = clientWrapper;
		}
		
		public MessageSendResult Reply(string message)
		{
			message = Sender.Nick + ", " + message;
			return Sender.Client.SendMessage(Channel, message);
		}

		public MessageSendResult ReturnMessage(string message)
		{
			return Sender.Client.SendMessage(Channel, message);
		}
	}
}
