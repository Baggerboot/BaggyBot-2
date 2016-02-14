using BaggyBot.Database;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Search : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Search for what?";
		
		public override void Use(CommandArgs command)
		{
			// TODO: Reimplement the search command
		}
	}
}
