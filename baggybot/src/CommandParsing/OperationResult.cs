using System.Collections.Generic;

namespace BaggyBot.CommandParsing
{
	class OperationResult
	{
		public Dictionary<string, string> Keys { get; private set; } = new Dictionary<string, string>();
		public Dictionary<string, bool> Flags { get; private set; } = new Dictionary<string, bool>();
		public ArgumentList Arguments { get; private set; } = new ArgumentList();
		public string RestArgument { get; set; }
		public string OperationName { get; private set; }

		public OperationResult(string operationName, string restArgument = null)
		{
			RestArgument = restArgument;
			OperationName = operationName;
		}
	}
}
