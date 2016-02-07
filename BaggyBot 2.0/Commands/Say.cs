namespace BaggyBot.Commands
{
	internal class Say : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;

		public void Use(CommandArgs command)
		{
			command.ReturnMessage(command.FullArgument);
		}
	}
}
