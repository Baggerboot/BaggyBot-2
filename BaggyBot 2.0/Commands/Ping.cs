using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Ping : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Ping(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, "Pong!");
		}
	}
}
