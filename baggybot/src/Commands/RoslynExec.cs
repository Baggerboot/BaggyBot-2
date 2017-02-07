using BaggyBot.Commands.Interpreters.Roslyn;

namespace BaggyBot.Commands
{
	internal class RoslynExec : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "roslyn";
		public override string Usage => "<C# code>";
		public override string Description => "Execute C# Code. Use the Context global variable to access the bot context.";

		private readonly RoslynInterpreter interpreter = new RoslynInterpreter();

		public override void Use(CommandArgs command)
		{
			command.ReturnMessage(interpreter.Interpret(command.FullArgument));
		}
	}
}
