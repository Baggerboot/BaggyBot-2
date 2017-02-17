using System;

namespace BaggyBot.CommandParsing
{
	class LambdaLookup<TKey, TValue>
	{
		private readonly Func<TKey, TValue> lookup;
		public TValue this[TKey key] => lookup(key);

		public LambdaLookup(Func<TKey, TValue> lookup)
		{
			this.lookup = lookup;
		}
	}
}
