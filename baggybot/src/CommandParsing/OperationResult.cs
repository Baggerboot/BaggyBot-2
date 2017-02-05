using System.Collections.Generic;

namespace BaggyBot.CommandParsing
{
	class OperationResult
	{
		public Dictionary<string, object> Keys { get; } = new Dictionary<string, object>();
		public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>();
		public ArgumentList Arguments { get; } = new ArgumentList();
		public string RestArgument { get; set; }
		public string OperationName { get; }

		public OperationResult(string operationName, string restArgument = null)
		{
			RestArgument = restArgument;
			OperationName = operationName;
		}

		public T GetKey<T>(string key)
		{
			var k = Keys[key];

			return (T) k;
		}
	}
}
