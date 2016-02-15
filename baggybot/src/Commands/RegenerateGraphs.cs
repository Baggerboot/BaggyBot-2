using System;

namespace BaggyBot.Commands
{
	internal class RegenerateGraphs : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Regenerates the graphs on the stats page.";

		//private const double MinWaitTime = 10; // In seconds
		//private DateTime lastUsage;
		
		public RegenerateGraphs()
		{
			//lastUsage = DateTime.Now.AddSeconds(MinWaitTime * -1);
		}

		public override void Use(CommandArgs command)
		{
			// TODO: Reimplement regeneration of graphs
			throw new NotImplementedException("Regenerating the graphs on the stats page is currently not possible.");
			/*var diff = DateTime.Now - lastUsage;
			if (diff.TotalSeconds < MinWaitTime)
			{
				command.ReturnMessage("This command may not be used more than once every {0} seconds. Please try again in {1} seconds.", MinWaitTime, (int)(MinWaitTime - diff.TotalSeconds));
				return;
			}
			const string args = "regenerate_graphs.sh";
			Process.Start("sh", args);
			lastUsage = DateTime.Now;
			command.ReturnMessage("I have regenerated the graphs on the stats page.");*/
		}
	}
}
