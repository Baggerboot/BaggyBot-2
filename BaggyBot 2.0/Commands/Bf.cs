using System;
using System.Linq;
using System.Text;

namespace BaggyBot.Commands
{
	internal class Bf : ICommand
	{
		private class MemoryCell
		{
			private MemoryCell next;
			public MemoryCell Next
			{
				get
				{
					if (next != null) return next;
					next = new MemoryCell { Previous = this };
					return next;
				}
				private set
				{
					next = value;
				}
			}
			private MemoryCell previous;
			public MemoryCell Previous
			{
				get
				{
					if (previous != null) return previous;
					previous = new MemoryCell { Next = this };
					return previous;
				}
				private set
				{
					previous = value;
				}
			}
			public byte Value { get; set; }
			public override string ToString()
			{
				return Previous.Value + " - " + Value + " - " + Next.Value;
			}
		}

		public PermissionLevel Permissions => PermissionLevel.All;

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0)
			{
				command.Reply("usage: -bf <brainfuck code>");
			}
			else {
				if (command.FullArgument.Contains(','))
				{
					command.ReturnMessage("Reading the Input Buffer is not supported yet.");
				}
				command.ReturnMessage(ProcessCode(command.FullArgument));
			}
		}
		private string ProcessCode(string code)
		{
			byte register = 0;
			var pointer = new MemoryCell();
			try
			{
				var output = ProcessCodeBlock(pointer, code, ref register);
				return output;
			}
			catch (ArgumentException e)
			{
				return e.Message;
			}
		}

		private string ProcessCodeBlock(MemoryCell pointer, string code, ref byte register)
		{
			var outputBuilder = new StringBuilder();

			for (var i = 0; i < code.Length; i++)
			{
				switch (code[i])
				{
					case '>':
						pointer = pointer.Next;
						break;
					case '<':
						pointer = pointer.Previous;
						break;
					case '+':
						pointer.Value++;
						break;
					case '-':
						pointer.Value--;
						break;
					case 'r':
						register = pointer.Value;
						break;
					case 'w':
						pointer.Value = register;
						break;
					case '[':
						var depth = 0;
						var length = -1;
						for (var j = i; j < code.Length; j++)
						{
							if (code[j] == '[')
							{
								depth++;
							}
							else if (code[j] == ']')
							{
								depth--;
								if (depth == 0)
								{
									length = j - i;
									break;
								}
							}
						}
						if (length == -1)
						{
							Logger.Log(this, depth.ToString());
							throw new ArgumentException("Syntax Error: '[' and ']' do not match up.");
						}

						while (pointer.Value != 0)
						{
							var codeBlock = code.Substring(i + 1, length - 1);
							outputBuilder.Append(ProcessCodeBlock(pointer, codeBlock, ref register));
						}
						i += length;
						break;
					case '.':
						outputBuilder.Append(Encoding.ASCII.GetString(new[] { pointer.Value }));
						break;
				}
			}
			return outputBuilder.ToString();
		}
	}
}
