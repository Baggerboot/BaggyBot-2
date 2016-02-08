namespace BaggyBot.Commands
{
	internal abstract class Command
	{
		public abstract PermissionLevel Permissions { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }
		// TODO: Create an Example field as well

		public abstract void Use(CommandArgs c);

		public void InformUsage(CommandArgs cmd)
		{
			cmd.Reply($"usage: {Bot.CommandIdentifier}{cmd.Command} {Usage}");
		}
	}
}
