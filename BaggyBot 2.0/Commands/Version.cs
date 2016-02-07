namespace BaggyBot.Commands
{
	internal class Version : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;

		public void Use(CommandArgs command)
		{
			command.Reply("I am currently running version {0}, last updated {1} UTC.", Bot.Version, Bot.LastUpdate.ToUniversalTime().ToString("MMM d, yyyy a\\t HH:mm"));
		}
	}
}
