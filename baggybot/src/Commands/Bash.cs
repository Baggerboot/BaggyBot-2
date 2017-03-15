namespace BaggyBot.Commands
{
	internal class Bash : StdioBridge
	{
		public override string Name => "bash";
		public override string Usage => "<bash expression>";
		public override string Description => "";

		public Bash()
		{
			Init("bash");
		}

		public override void Use(CommandArgs command)
		{
			EnsureProcess(command.Channel);
			Write(command.Channel, command.FullArgument);
		}
	}
}
