namespace BaggyBot.Commands
{
	class Shutdown : ICommand
	{
		private readonly Bot program;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

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
