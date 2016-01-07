namespace BaggyBot.Commands
{
	class Reconnect : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }
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
