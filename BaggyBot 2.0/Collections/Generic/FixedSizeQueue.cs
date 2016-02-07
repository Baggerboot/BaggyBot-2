using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Collections.Generic
{
	// TODO: Check whether concurrency is correctly implemented
	internal class FixedSizeQueue<T> : ConcurrentQueue<T>
	{
		private readonly object syncObject = new object();
		public int Size { get; }

		public FixedSizeQueue(int size)
		{
			Size = size;
		}

		public new void Enqueue(T obj)
		{
			base.Enqueue(obj);
			lock (syncObject)
			{
				while (Count > Size)
				{
					T outObj;
					TryDequeue(out outObj);
				}
			}
		}

		public new T[] ToArray()
		{
			return base.ToArray();
		}
	}
}
