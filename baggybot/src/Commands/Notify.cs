namespace BaggyBot.Commands
{
	internal class Notify : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Returns a pre-defined message to the user of the command.";

		private string message;
		public Notify(string message)
		{
			this.message = message;
		}

		public override void Use(CommandArgs command)
		{
			command.Reply(message);
		}
	}
}
