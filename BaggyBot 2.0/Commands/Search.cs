using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Search : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;

		public Search(DataFunctionSet df)
		{
			// TODO: Implement the Search command again
			//dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
		}
	}
}
