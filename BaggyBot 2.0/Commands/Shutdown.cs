namespace BaggyBot.Commands
{
	internal class Shutdown : ICommand
	{
		private readonly Bot program;
		public PermissionLevel Permissions => PermissionLevel.BotOperator;

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
