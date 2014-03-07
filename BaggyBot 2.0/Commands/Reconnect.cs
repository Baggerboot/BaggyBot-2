using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Reconnect : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }
		private IrcInterface ircInterface;

		public Reconnect(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.Reconnect();
		}
	}
}
