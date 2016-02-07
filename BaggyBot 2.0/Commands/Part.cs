namespace BaggyBot.Commands
{
	internal class Part : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;
		public string Usage => "<channel>";
		public string Description => "Makes me leave an IRC channel.";

		private readonly IrcInterface ircInterface;
		public Part(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0 || command.Args.Length > 2)
			{
				command.ReturnMessage("usage: -part <channel> [reason]");
			}
			else {
				ircInterface.Part(command.Args[0], command.Args.Length == 2 ? command.Args[1] : null);
			}
		}
	}
}
