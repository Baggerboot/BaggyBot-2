namespace BaggyBot.Commands
{
	internal class Snag : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "snag";
		public override string Usage => "[username]";
		public override string Description => "Makes me quote the next message that's written to this channel, or the next message written by the user specified.";

		public override void Use(CommandArgs command)
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
