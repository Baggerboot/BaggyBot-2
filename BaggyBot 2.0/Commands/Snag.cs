namespace BaggyBot.Commands
{
	internal class Snag : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;

		public void Use(CommandArgs command)
		{
			switch (command.Args.Length)
			{
				case 0:
					ControlVariables.SnagNextLine = true;
					break;
				case 1:
					ControlVariables.SnagNextLineBy = command.Args[0];
					break;
				default:
					command.Reply("Usage: -snag [nickname]");
					break;
			}
		}
	}
}
