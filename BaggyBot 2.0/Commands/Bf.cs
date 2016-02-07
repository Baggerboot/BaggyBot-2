using System;
using System.Linq;
using System.Text;
using BaggyBot.Commands.Interpreters.Brainfuck;

namespace BaggyBot.Commands
{
	internal class Bf : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<Brainfuck code>";
		public string Description => "Executes the given Brainfuck code and prints its result to IRC. The interpreter additionally supports reading/writing a single register with `r` and `w`.";

		private readonly BrainfuckInterpreter interpreter = new BrainfuckInterpreter();

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0)
			{
				command.Reply("usage: -bf <brainfuck code>");
			}
			else
			{
				if (command.FullArgument.Contains(','))
				{
					command.ReturnMessage("Reading the Input Buffer is not supported yet.");
				}
				command.ReturnMessage(interpreter.ProcessCode(command.FullArgument));
			}
		}
	}
}
