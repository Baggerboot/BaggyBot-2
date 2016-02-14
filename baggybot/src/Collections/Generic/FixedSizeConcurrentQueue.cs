using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Collections.Generic
{
	/// <summary>
	/// Custom Queue class that contains a fixed maximum number of items.
	/// If, after enqueueing a new item, the queue contains too many items,
	/// the item at the front of the queue will be dequeued.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class FixedSizeConcurrentQueue<T> : ConcurrentQueue<T>
	{
		private readonly object syncObject = new object();
		public int Size { get; }

		public FixedSizeConcurrentQueue(int size)
		{
			Size = size;
		}

		public new void Enqueue(T obj)
		{
			base.Enqueue(obj);
			T outObj;
			TryDequeue(out outObj);
		}

		public new T[] ToArray()
		{
			return base.ToArray();
		}
	}
}
