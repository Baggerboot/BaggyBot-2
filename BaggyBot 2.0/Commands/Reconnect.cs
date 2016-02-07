namespace BaggyBot.Commands
{
	internal class Reconnect : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;
		public string Usage => "";
		public string Description => "Simulates a ping timeout, causing me to attempt to reconnect to the IRC server.";

		private readonly IrcInterface ircInterface;

		public Reconnect(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.Reconnect();
		}
	}
}
