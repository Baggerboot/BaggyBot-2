namespace BaggyBot.Commands
{
	internal class Reconnect : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "";
		public override string Description => "Simulates a ping timeout, causing me to attempt to reconnect to the IRC server.";

		public override void Use(CommandArgs command)
		{
			command.Client.Reconnect();
		}
	}
}
