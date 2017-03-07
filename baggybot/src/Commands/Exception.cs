using System;
using BaggyBot.Monitoring;

namespace BaggyBot.Commands
{
	internal class CustomException : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "exception";
		public override string Usage => "";
		public override string Description => "Throw a user-generated exception.";

		public override void Use(CommandArgs command)
		{
			Logger.Log(this, "Throwing user-generated exception", LogLevel.Warning);
			throw new Exception("User-requested exception");
		}
	}
}