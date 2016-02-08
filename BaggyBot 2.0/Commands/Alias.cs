using System.Linq;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Alias : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<key> <command> [parameters ...]";
		public override string Description => "Creates an alias for a command.";

		private readonly DataFunctionSet dataFunctionSet;

		public Alias(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

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
				dataFunctionSet.UpsertMiscData("alias", key, value);
				command.Reply("I've aliased {0} to \"{1}\"", key, value);
			}
			else
			{
				command.Reply("usage: -alias <key> <command> [parameters ...]");
			}
		}

		public string GetAlias(string key)
		{
			return dataFunctionSet.GetMiscData("alias", key);
		}

		public bool ContainsKey(string key)
		{
			return dataFunctionSet.MiscDataContainsKey("alias", key);
		}
	}
}
