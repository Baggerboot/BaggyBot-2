using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.Internal.Irc
{
	class IrcChatUser : ChatUser
	{
		private IrcChatUser(Plugin client, string nickname, string uniqueId, string ident, string hostmask, string nickServ) : base(nickname, uniqueId, false)
		{

		}
	}
}
