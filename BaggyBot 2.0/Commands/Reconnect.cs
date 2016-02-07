namespace BaggyBot.Commands
{
	internal class Reconnect : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.BotOperator;
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
