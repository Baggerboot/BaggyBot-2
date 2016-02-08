namespace BaggyBot.Commands
{
	internal class Reconnect : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "";
		public override string Description => "Simulates a ping timeout, causing me to attempt to reconnect to the IRC server.";

		private readonly IrcInterface ircInterface;

		public Reconnect(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;
		}

		public override void Use(CommandArgs command)
		{
			ircInterface.Reconnect();
		}
	}
}
