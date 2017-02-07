namespace BaggyBot.Commands
{
	internal class Shutdown : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "shutdown";
		public override string Usage => "";
		public override string Description => "Makes me shut down.";

		public override void Use(CommandArgs command)
		{
			Bot.Shutdown();
		}
	}
}
