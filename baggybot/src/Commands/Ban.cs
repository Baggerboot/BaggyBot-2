namespace BaggyBot.Commands
{
	internal class Ban : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "ban";
		public override string Usage => "<username>";
		public override string Description => "Ban a user from the current channel/server.";

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length != 1)
			{
				InformUsage(command);
			}
			else
			{
				var username = command.Args[0];
				var user = Client.FindUser(username);
				Client.Ban(user, command.Channel);
			}
		}
	}
}
