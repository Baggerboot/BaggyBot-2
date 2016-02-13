using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			var components = arguments.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
			if (components.Length == 0)
			{
				return new OperationResult("default");
			}
			if (components[0].StartsWith("-"))
			{
				return operations["default"].Parse(components, "default");
			}
			if (!operations.ContainsKey(components[0]))
			{
				return operations["default"].Parse(components, "default");
			}
			else
			{
				return operations[components[0]].Parse(components.Skip(1), components[0]);
			}
		}
	}
}
