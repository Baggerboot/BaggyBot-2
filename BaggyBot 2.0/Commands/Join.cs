using BaggyBot.DataProcessors.IO;

namespace BaggyBot.Commands
{
	internal class Join : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "<channel>";
		public override string Description => "Makes me join an IRC channel.";

		private readonly IrcInterface ircInterface;

		public Join(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 1)
			{
				command.Reply("Joining {0}", command.Args[0]);
				if (!ircInterface.JoinChannel(command.Args[0]))
				{
					command.ReturnMessage("Failed to join the channel");
				}
			}
			else {
				command.Reply("Usage: -join <channel>");
			}
		}
	}
}
