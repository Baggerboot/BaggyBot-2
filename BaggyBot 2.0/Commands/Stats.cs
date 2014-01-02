using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Stats : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			command.Reply("Statistics can be found at http://www.jgeluk.net/stats/. For now, please use the -regen command to regenerate the graph on the stats page.");
		}
	}
}
