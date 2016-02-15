namespace BaggyBot.Commands.Interpreters.Brainfuck
{
	internal class MemoryCell
	{
		private MemoryCell next;

		public MemoryCell Next
		{
			get
			{
				if (next != null) return next;
				next = new MemoryCell {Previous = this};
				return next;
			}
			private set { next = value; }
		}

		private MemoryCell previous;

		public MemoryCell Previous
		{
			get
			{
				if (previous != null) return previous;
				previous = new MemoryCell {Next = this};
				return previous;
			}
			private set { previous = value; }
		}

		public byte Value { get; set; }

		public override string ToString()
		{
			return Previous.Value + " - " + Value + " - " + Next.Value;
		}
	}
}
