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
		private Bot program;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Shutdown(Bot prg)
		{
			program = prg;
		}

		public void Use(CommandArgs command)
		{
			program.Shutdown();
		}
	}
}
