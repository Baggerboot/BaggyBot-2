using BaggyBot.MessagingInterface;

namespace BaggyBot.Commands
{
	internal class Join : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "join";
		public override string Usage => "<channel>";
		public override string Description => "Makes me join an IRC channel.";

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 1)
			{
				command.Reply($"Joining {command.Args[0]}");
				Client.JoinChannel(command.Args[0]);
			}
			else {
				command.Reply("Usage: -join <channel>");
			}
		}
	}
}
