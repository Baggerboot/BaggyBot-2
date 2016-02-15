using System.Linq;
using BaggyBot.Commands.Interpreters.Brainfuck;

namespace BaggyBot.Commands
{
	internal class Bf : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<Brainfuck code>";
		public override string Description => "Executes the given Brainfuck code and prints its result to IRC. The interpreter additionally supports reading/writing a single register with `r` and `w`.";

		private readonly BrainfuckInterpreter interpreter = new BrainfuckInterpreter();

		public override void Use(CommandArgs command)
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
