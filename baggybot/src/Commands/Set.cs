using System;
using BaggyBot.CommandParsing;
using BaggyBot.Database;
using BaggyBot.Database.Model;

namespace BaggyBot.Commands
{
	internal class Set : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "set";
		public override string Usage => "<property> [key] <value>";
		public override string Description => "Sets the value of a property, or the value of a key belonging to that property.";

		public override void Use(CommandArgs command)
		{
			var parser = new CommandParser(new Operation())
				.AddOperation("name", new Operation()
					.AddFlag("uid", 'u')
					.AddArgument("match"))
				.AddOperation("cfg", new Operation()
					.AddArgument("key")
					.AddRestArgument("value"));

			var cmd = parser.Parse(command.FullArgument);

			switch (cmd.OperationName)
			{
				case "name":
					SetName(cmd.Arguments["match"], cmd.Flags["uid"], cmd.RestArgument);
					command.Reply("Done");
					break;
				case "cfg":
					SetCfg();
					break;
				default:
					InformUsage(command);
					break;
			}
		}
		private void SetName(string match, bool matchIsUid, string name)
		{
			User user;
			if (matchIsUid)
			{
				user = StatsDatabase.GetUserById(int.Parse(match));
			}
			else
			{
				user = StatsDatabase.GetUserByNickname(match);
			}
			user.AddressableNameOverride = name.Trim();
			StatsDatabase.UpdateUser(user);
		}

		private void SetCfg()
		{
			
		}
	}
}
