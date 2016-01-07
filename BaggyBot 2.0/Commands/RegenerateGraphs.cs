using System;
using System.Diagnostics;

namespace BaggyBot.Commands
{
	class RegenerateGraphs : ICommand
	{
				public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private DateTime lastUsage;
		private const double MinWaitTime = 10; // In seconds

		public RegenerateGraphs()
		{
			lastUsage = DateTime.Now.AddSeconds(MinWaitTime * -1);
		}

		public void Use(CommandArgs command)
		{
			var diff = DateTime.Now - lastUsage;
			if (diff.TotalSeconds < MinWaitTime) {
				command.ReturnMessage("This command may not be used more than once every {0} seconds. Please try again in {1} seconds.", MinWaitTime, (int)(MinWaitTime - diff.TotalSeconds));
				return;
			}
			const string args = "regenerate_graphs.sh";
			Process.Start("sh", args);
			lastUsage = DateTime.Now;
			command.ReturnMessage("I have regenerated the graphs on the stats page.");
		}
	}
}
