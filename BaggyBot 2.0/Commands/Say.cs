namespace BaggyBot.Commands
{
	internal class Say : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<message>";
		public override string Description => "Makes me say something.";

		public override void Use(CommandArgs command)
		{
			command.ReturnMessage(command.FullArgument);
		}
	}
}
