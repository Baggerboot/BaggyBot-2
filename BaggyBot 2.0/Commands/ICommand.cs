namespace BaggyBot.Commands
{
	interface ICommand
	{
		void Use(CommandArgs c);
		PermissionLevel Permissions { get; }
	}
}
