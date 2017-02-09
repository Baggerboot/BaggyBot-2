using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
