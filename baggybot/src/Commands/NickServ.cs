namespace BaggyBot.Commands
{
	[DisabledCommand]
	internal class NickServ : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "nickserv";
		public override string Usage => "[-f]|[update]|[set <username>]";
		public override string Description => "Performs a NickServ lookup for your username against the database. Use `-f` to query the NickServ service instead, or use `set` to store a user's current NickServ username in the database.";
		
		public override void Use(CommandArgs command)
		{
			// TODO: Reimplement this
		}
	}
}
