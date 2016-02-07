namespace BaggyBot.Commands
{
	internal class Snag : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;
		public string Usage => "[username]";
		public string Description => "Makes me quote the next message that's written to this channel, or the next message written by the user specified.";

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
					command.Reply("Usage: -snag [username]");
					break;
			}
		}
	}
}
