using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Notify : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private string message;
		public Notify(string message)
		{
			this.message = message;
		}

		public void Use(CommandArgs command)
		{
			command.Reply(message);
		}
	}
}
