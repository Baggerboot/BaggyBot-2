using BaggyBot.DataProcessors;


namespace BaggyBot.Commands
{
	class Join : ICommand
	{
		private readonly IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Join(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 1) {
				command.Reply("Joining {0}", command.Args[0]);
				if (!ircInterface.JoinChannel(command.Args[0])) {
					command.ReturnMessage("Failed to join the channel");
				}
			} else {
				command.Reply("Usage: -join <channel>");
			}
		}
	}
}
