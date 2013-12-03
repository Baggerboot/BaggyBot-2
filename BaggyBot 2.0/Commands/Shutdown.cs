using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Database;

namespace BaggyBot.Commands
{
	class Shutdown : ICommand
	{
		private IrcInterface ircInterface;
		private Bot program;

		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Shutdown(IrcInterface inter, Bot prg)
		{
			ircInterface = inter;
			program = prg;
		}

		public void Use(CommandArgs command)
		{
			program.Shutdown();
		}
	}
}
