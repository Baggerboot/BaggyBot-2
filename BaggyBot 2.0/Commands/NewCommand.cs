namespace BaggyBot.Commands
{
	class NewCommand : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			command.ReturnMessage("it works");
		}
	}
}
