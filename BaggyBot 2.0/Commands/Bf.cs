using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Bf : ICommand
	{
		private class MemoryCell
		{
			private MemoryCell next;
			public MemoryCell Next
			{
				get
				{
					if (next == null) {
						next = new MemoryCell();
						next.Previous = this;
					}
					return next;
				}
				set
				{
					next = value;
				}
			}
			private MemoryCell previous;
			public MemoryCell Previous
			{
				get
				{
					if (previous == null) {
						previous = new MemoryCell();
						previous.Next = this;
					}
					return previous;
				}
				set
				{
					previous = value;
				}
			}
			public byte Value = 0;
			public override string ToString()
			{
				return Previous.Value + " - " + Value + " - " + Next.Value;
			}
		}

		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0) {
				command.Reply("usage: -bf <brainfuck code>");
			} else {
				if (command.FullArgument.Contains(',')) {
					command.ReturnMessage("Reading the Input Buffer is not supported yet.");
				}
				command.ReturnMessage(ProcessCode(command.FullArgument));
			}
		}
		private string ProcessCode(string code)
		{
			byte register = 0;
			var pointer = new MemoryCell();
			try {
				string output = ProcessCodeBlock(pointer, code, ref register);
				return output;
			} catch (ArgumentException e) {
				return e.Message;
			}
		}

		private string ProcessCodeBlock(MemoryCell pointer, string code, ref byte register)
		{
			StringBuilder outputBuilder = new StringBuilder();

			for (int i = 0; i < code.Length; i++) {
				switch (code[i]) {
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
						int depth = 0;
						int length = -1;
						for (int j = i; j < code.Length; j++) {
							if (code[j] == '[') {
								depth++;
							} else if (code[j] == ']') {
								depth--;
								if (depth == 0) {
									length = j - i;
									break;
								}
							}
						}
						if (length == -1) {
							Logger.Log(depth + "");
							throw new ArgumentException("Syntax Error: '[' and ']' do not match up.");
						}

						while (pointer.Value != 0) {
							string codeBlock = code.Substring(i + 1, length - 1);
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
