namespace BaggyBot.Commands
{
	internal class Version : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Prints version information about me.";

		public override void Use(CommandArgs command)
		{
			command.Reply("I am currently running version {0}, last updated {1} UTC.", Bot.Version, Bot.LastUpdate.ToUniversalTime().ToString("MMM d, yyyy a\\t HH:mm"));
		}
	}
}
