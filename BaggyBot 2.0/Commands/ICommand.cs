namespace BaggyBot.Commands
{
	internal interface ICommand
	{
		void Use(CommandArgs c);
		PermissionLevel Permissions { get; }
		//string Usage { get; }
		//string Description { get; }
	}
}
