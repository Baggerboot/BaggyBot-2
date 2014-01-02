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
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			command.Reply("I am currently running version {0}, last updated {1} UTC.", Bot.Version, Bot.LastUpdate.ToUniversalTime().ToString("MMM d, yyyy a\\t HH:mm"));
		}
	}
}
