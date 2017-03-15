namespace BaggyBot.Commands
{
	internal class Kick : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "kick";
		public override string Usage => "<username>";
		public override string Description => "Kick a user from the current channel.";

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
				Client.Kick(user, command.Channel);
			}
		}
	}
}
