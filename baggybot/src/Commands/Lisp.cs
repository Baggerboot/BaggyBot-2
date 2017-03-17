using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	internal class Lisp : StdioBridge
	{
		public override string Name => "lisp";
		public override string Usage => "<lisp expression>";
		public override string Description => "";

		public Lisp()
		{
			Init("sbcl");
		}

		public override void Use(CommandArgs command)
		{
			EnsureProcess(command.Channel);

			var parser = new CommandParser(new Operation()
				.AddFlag("reset")
				.AddKey("key", null, 'k')
				.AddRestArgument(null));

			var res = parser.Parse(command.FullArgument);
			var key = res.Keys["key"];

			if (res.Flags["reset"])
			{
				Reset(command.Channel);
			}
			else if (key != null)
			{
				SendKey(command.Channel, key);
			}
			else
			{
				Write(command.Channel, command.FullArgument);
			}
		}
	}
}