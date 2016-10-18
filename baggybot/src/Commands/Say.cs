namespace BaggyBot.Commands
{
	internal class Say : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<message>";
		public override string Description => "Makes me say something.";

		public override void Use(CommandArgs command)
		{
			if (command.FullArgument == null)
			{
				InformUsage(command);
				return;
			}
			command.ReturnMessage(command.FullArgument);
		}
	}
}
