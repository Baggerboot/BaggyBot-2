using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Alias : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private readonly DataFunctionSet dataFunctionSet;

		public Alias(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length > 2)
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
