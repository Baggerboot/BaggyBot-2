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

		public OperationResult Parse(string[] tokens)
		{
			var fullCommand = string.Join(" ", tokens);
			var components = new List<CommandComponent>();
			var currentIndex = 0;
			foreach (var token in tokens)
			{
				currentIndex = fullCommand.IndexOf(token, currentIndex, StringComparison.Ordinal);
				components.Add(new CommandComponent(currentIndex, token));
				currentIndex += token.Length;
			}

			if (components.Count == 0)
			{
				return operations["default"].Parse(components, "default", fullCommand);
			}
			if (components[0].Value.StartsWith("-"))
			{
				return operations["default"].Parse(components, "default", fullCommand);
			}
			if (!operations.ContainsKey(components[0].Value))
			{
				return operations["default"].Parse(components, "default", fullCommand);
			}
			else
			{
				return operations[components[0].Value].Parse(components.Skip(1), components[0].Value, fullCommand);
			}
		}

		public OperationResult Parse(string arguments)
		{
			if (arguments == null)
			{
				return operations["default"].Parse(null, "default", null);
				//return new OperationResult("default");
			}

			var tokens = arguments.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);

			return Parse(tokens);
		}
	}
}
