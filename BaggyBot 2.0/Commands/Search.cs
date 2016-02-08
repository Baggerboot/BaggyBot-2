using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Search : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Search for what?";

		public Search(DataFunctionSet df)
		{
			// TODO: Implement the Search command again
			//dataFunctionSet = df;
		}

		public override void Use(CommandArgs command)
		{
		}
	}
}
