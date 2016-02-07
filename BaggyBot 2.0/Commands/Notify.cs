namespace BaggyBot.Commands
{
	internal class Notify : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "";
		public string Description => "Returns a pre-defined message to the user of the command.";

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
