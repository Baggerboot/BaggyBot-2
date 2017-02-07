using System.Linq;
using BaggyBot.Database;

namespace BaggyBot.Commands
{
	internal class Alias : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "alias";
		public override string Usage => "<key> <command> [parameters ...]";
		public override string Description => "Creates an alias for a command.";
		
		public override void Use(CommandArgs command)
		{
			if (command.Args.Length > 1)
			{
				var key = command.Args[0];
				var value = string.Join(" ", command.Args.Skip(1));
				if (value.StartsWith("-"))
				{
					value = value.Substring(1);
				}
				StatsDatabase.UpsertMiscData("alias", key, value);
				command.Reply($"I've aliased {key} to \"{value}\"");
			}
			else
			{
				InformUsage(command);
			}
		}

		public string GetAlias(StatsDatabaseManager db, string key)
		{
			return db.GetMiscData("alias", key);
		}

		public bool ContainsKey(StatsDatabaseManager db, string key)
		{
			return db.MiscDataContainsKey("alias", key);
		}
	}
}
