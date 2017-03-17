using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	internal class Kotlin : StdioBridge
	{
		public override string Name => "kotlinc";
		public override string Usage => "<kotlin expression>";
		public override string Description => "";

		public Kotlin()
		{
			Init("kotlin");
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