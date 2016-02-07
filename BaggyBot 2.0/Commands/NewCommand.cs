namespace BaggyBot.Commands
{
	internal class NewCommand : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;

		public void Use(CommandArgs command)
		{
			command.ReturnMessage("it works");
		}
	}
}
