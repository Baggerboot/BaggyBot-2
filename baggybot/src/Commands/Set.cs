using System;
using BaggyBot.Database;
using BaggyBot.Database.Model;

namespace BaggyBot.Commands
{
	internal class Set : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "set";
		public override string Usage => "<property> [key] <value>";
		public override string Description => "Sets the value of a property, or the value of a key belonging to that property.";
		
		public override void Use(CommandArgs command)
		{
			if (command.Args.Length < 3)
			{
				command.Reply("Usage: -set <property> [key] <value>");
				return;
			}
			switch (command.Args[0])
			{
				case "name":
					int uid;
					User user;
					if (int.TryParse(command.Args[1], out uid))
					{
						user = StatsDatabase.GetUserById(uid);
					}
					else
					{
						user = StatsDatabase.GetUserByNickname(command.Args[1]);
					}
					user.AddressableNameOverride = command.Args[2];
					StatsDatabase.UpdateUser(user);
					command.Reply("Done.");

					break;
				case "-s":
					/*string data;
					if (command.Args.Length > 3)
					{
						data = string.Join(" ", command.Args.Skip(2));
					}
					else {
						data = command.Args[2];
					}*/

					throw new NotImplementedException("Runtime modification of the settings file is not supported yet.");

				//TODO: Allow runtime modification of settings file
				/*if (Settings.Instance.SettingExists(command.Args[1])) {
					command.Reply(command.Args[1] + " set to " + data);
				} else {
					command.ReturnMessage("New key \"{0}\" created. Value set to {1}", command.Args[1], data);
				}*/
				default:
					command.ReturnMessage($"The property \"{command.Args[0]}\" does not exist.");
					break;
			}
		}
	}
}
