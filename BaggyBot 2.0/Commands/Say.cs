namespace BaggyBot.Commands
{
	internal class Say : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<message>";
		public string Description => "Makes me say something.";

		public void Use(CommandArgs command)
		{
			command.ReturnMessage(command.FullArgument);
		}
	}
}
