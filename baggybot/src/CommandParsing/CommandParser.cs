using System;
using System.Collections.Generic;
using System.Linq;

namespace BaggyBot.CommandParsing
{
	class CommandParser
	{
		private Dictionary<string, Operation> operations = new Dictionary<string, Operation>(); 

		public CommandParser(Operation defaultOptions)
		{
			operations.Add("default", defaultOptions);
		}

		public CommandParser AddOperation(string name, Operation options)
		{
			operations.Add(name, options);
			return this;
		}

		public OperationResult Parse(string arguments)
		{
			if (arguments == null)
			{
				return new OperationResult("default");
			}

			var components = new List<CommandComponent>();
			var tokens = arguments.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
			int currentIndex = 0;
			foreach (var token in tokens)
			{
				currentIndex = arguments.IndexOf(token, currentIndex, StringComparison.Ordinal);
				components.Add(new CommandComponent(currentIndex, token));
			}

			if (components.Count == 0)
			{
				return new OperationResult("default");
			}
			if (components[0].Value.StartsWith("-"))
			{
				return operations["default"].Parse(components, "default", arguments);
			}
			if (!operations.ContainsKey(components[0].Value))
			{
				return operations["default"].Parse(components, "default", arguments);
			}
			else
			{
				return operations[components[0].Value].Parse(components.Skip(1), components[0].Value, arguments);
			}
		}
	}
}
