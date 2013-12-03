using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace BaggyBot.Commands
{
	class Version : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Version(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, String.Format("I am currently running version {0}, last updated {1} UTC.", Bot.Version, Bot.LastUpdate.ToUniversalTime().ToString("MMM d, yyyy a\\t HH:mm")));
		}
	}
}
