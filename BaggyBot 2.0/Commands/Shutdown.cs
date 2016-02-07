namespace BaggyBot.Commands
{
	internal class Shutdown : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;
		public string Usage => "";
		public string Description => "Makes me shut down.";

		private readonly Bot program;

		public Shutdown(Bot prg)
		{
			program = prg;
		}

		public void Use(CommandArgs command)
		{
			program.Shutdown();
		}
	}
}
