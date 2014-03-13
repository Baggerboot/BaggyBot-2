using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class NewCommand : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			command.ReturnMessage("it works");
		}
	}
}
