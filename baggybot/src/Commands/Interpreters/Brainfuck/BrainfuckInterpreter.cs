using System;
using System.Text;
using BaggyBot.Monitoring;

namespace BaggyBot.Commands.Interpreters.Brainfuck
{
	class BrainfuckInterpreter
	{
		public string ProcessCode(string code)
		{
			byte register = 0;
			var pointer = new MemoryCell();
			try
			{
				var output = ProcessCodeBlock(ref pointer, code, ref register);
				return output;
			}
			catch (ArgumentException e)
			{
				return e.Message;
			}
		}

		private string ProcessCodeBlock(ref MemoryCell pointer, string code, ref byte register)
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
					/*case 'r':
						register = pointer.Value;
						break;
					case 'w':
						pointer.Value = register;
						break;*/
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
							outputBuilder.Append(ProcessCodeBlock(ref pointer, codeBlock, ref register));
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
