namespace BaggyBot.Commands
{
	internal class Sql : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<SQL code>";
		public override string Description => "Execute arbitrary SQL code and return its result.";

		public override void Use(CommandArgs command)
		{
			// TODO: Implement the SQL command again
		}
	}
}
