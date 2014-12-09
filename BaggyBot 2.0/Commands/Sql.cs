namespace BaggyBot.Commands
{
	class Sql : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{

		}
	}
}
