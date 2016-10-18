using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

namespace BaggyBot.InternalPlugins.Irc
{
	class IrcChatUser : ChatUser
	{
		private IrcChatUser(Plugin client, string nickname, string uniqueId, string ident, string hostmask, string nickServ) : base(client, nickname, uniqueId, false)
		{

		}
	}
}
