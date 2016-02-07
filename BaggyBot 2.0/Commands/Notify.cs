namespace BaggyBot.Commands
{
	internal class Notify : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		private string message;
		public Notify(string message)
		{
			this.message = message;
		}

		public void Use(CommandArgs command)
		{
			command.Reply(message);
		}
	}
}
