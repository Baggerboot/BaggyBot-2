using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class RegenerateGraphs : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private DateTime lastUsage;
		private const double minWaitTime = 10; // In seconds

		public RegenerateGraphs(IrcInterface inter)
		{
			ircInterface = inter;
			lastUsage = DateTime.Now.AddSeconds(minWaitTime * -1);
		}

		public void Use(CommandArgs command)
		{
			TimeSpan diff = DateTime.Now - lastUsage;
			if (diff.TotalSeconds < minWaitTime) {
				ircInterface.SendMessage(command.Channel, string.Format("This command may not be used more than once every {0} seconds. Please try again in {1} seconds.", minWaitTime, (int)(minWaitTime - diff.TotalSeconds)));
				return;
			}
			var args = "regenerate_graphs.sh";
			System.Diagnostics.Process.Start("sh", args);
			lastUsage = DateTime.Now;
			ircInterface.SendMessage(command.Channel, "I have regenerated the graphs on the stats page.");
		}
	}
}
