namespace BaggyBot.MessagingInterface
{
	public class IrcUser : IRCSharp.IRC.IrcUser
	{
		public IrcClientWrapper Client { get; }

		public IrcUser(IrcClientWrapper client, string nick, string ident, string hostmask) :base(nick, ident, hostmask)
		{
			Client = client;
		}

		public IrcUser(IrcClientWrapper client, IRCSharp.IRC.IrcUser user) :base(user.Nick, user.Ident, user.Hostmask)
		{
			Client = client;
		}
	}
}
