using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Commands.Interpreters.Roslyn;

namespace BaggyBot.Commands
{
	internal class RoslynExec : Command
	{
		public override PermissionLevel Permissions { get { return PermissionLevel.All; } }
		public override string Usage => "";
		public override string Description => "";

		private RoslynInterpreter interpreter = new RoslynInterpreter();

		public override void Use(CommandArgs command)
		{
			command.ReturnMessage(interpreter.Interpret(command.FullArgument));
		}
	}
}
