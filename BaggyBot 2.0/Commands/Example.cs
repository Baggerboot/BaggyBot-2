﻿// NOTE: This is not an actual command, just a template for quickly adding a new command

namespace BaggyBot.Commands
{
	class Example : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		public string Usage => "";
		public string Description => "";

		public void Use(CommandArgs command)
		{

		}
	}
}
