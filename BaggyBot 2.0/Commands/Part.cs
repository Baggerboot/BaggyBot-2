namespace BaggyBot.Commands
{
	class Part : ICommand
	{
		private readonly IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Part(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0 || command.Args.Length > 2) {
				command.ReturnMessage("usage: -part <channel> [reason]");
			} else {
				ircInterface.Part(command.Args[0], (command.Args.Length == 2 ? command.Args[1] : null));
			}
		}
	}
}
