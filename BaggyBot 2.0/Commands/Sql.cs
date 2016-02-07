namespace BaggyBot.Commands
{
	internal class Sql : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<SQL code>";
		public string Description => "Execute arbitrary SQL code and return its result.";

		public void Use(CommandArgs command)
		{
			// TODO: Implement the SQL command again
		}
	}
}
