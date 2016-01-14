using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Search : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		//private DataFunctionSet dataFunctionSet;

		public Search(DataFunctionSet df)
		{
			//dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{

		}
	}
}
