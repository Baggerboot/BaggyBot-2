namespace BaggyBot.Commands
{
	class Say : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command){
			command.ReturnMessage(command.FullArgument);
		}
	}
}
