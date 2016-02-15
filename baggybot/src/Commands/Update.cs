using System;

namespace BaggyBot.Commands
{
	internal class Update : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "[--no-dl]";
		public override string Description => "Downloads a new update and makes me restart to apply it.";

		private readonly Bot bot;
		public Update(Bot bot)
		{
			this.bot = bot;
		}

		public override void Use(CommandArgs command)
		{
			// TODO: Implemenent self-updating
			throw new NotImplementedException();
		}
	}
}
