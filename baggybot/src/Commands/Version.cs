namespace BaggyBot.Commands
{
	internal class Version : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "version";
		public override string Usage => "";
		public override string Description => "Prints version information about me.";

		public override void Use(CommandArgs command)
		{
			command.Reply($"I am currently running version {Bot.Version}, last updated {Bot.LastUpdate.ToUniversalTime():MMM d, yyyy a\t HH: mm} UTC.");
		}
	}
}
