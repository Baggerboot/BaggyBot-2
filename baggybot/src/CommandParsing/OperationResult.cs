using System.Collections.Generic;
using System.Linq;

namespace BaggyBot.CommandParsing
{
	class OperationResult
	{
		internal Dictionary<string, object> InternalKeys { get; } = new Dictionary<string, object>();
		public LambdaLookup<string, string> Keys;
		public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>();
		public ArgumentList Arguments { get; } = new ArgumentList();
		public string RestArgument { get; internal set; }
		public string OperationName { get; }

		public OperationResult(string operationName, string restArgument = null)
		{
			Keys = new LambdaLookup<string, string>(key => (string)InternalKeys[key]);
			RestArgument = restArgument;
			OperationName = operationName;
		}

		public bool HasKey(string key) => InternalKeys.ContainsKey(key);

		public T GetKey<T>(string key)
		{
			var k = InternalKeys[key];

			return (T) k;
		}

		public string GetKey(string key)
		{
			return (string) InternalKeys[key];
		}
	}
}
