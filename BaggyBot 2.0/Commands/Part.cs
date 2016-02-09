using BaggyBot.DataProcessors.IO;

namespace BaggyBot.Commands
{
	internal class Part : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "<channel>";
		public override string Description => "Makes me leave an IRC channel.";

		private readonly IrcInterface ircInterface;
		public Part(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 0 || command.Args.Length > 2)
			{
				command.ReturnMessage("usage: -part <channel> [reason]");
			}
			else {
				ircInterface.Part(command.Args[0], command.Args.Length == 2 ? command.Args[1] : null);
			}
		}
	}
}
