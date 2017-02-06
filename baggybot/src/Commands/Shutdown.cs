namespace BaggyBot.Commands
{
	internal class Shutdown : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "";
		public override string Description => "Makes me shut down.";

		private readonly Bot bot;

		public Shutdown(Bot prg)
		{
			bot = prg;
		}

		public override void Use(CommandArgs command)
		{
			bot.Shutdown();
		}
	}
}
