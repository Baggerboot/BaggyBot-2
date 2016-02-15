using System;
using System.Linq;

namespace BaggyBot.Commands
{
	internal class Set : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
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
					if (int.TryParse(command.Args[1], out uid))
					{
						command.Client.StatsDatabase.SetPrimary(uid, command.Args[2]);
						command.Reply("Done.");
					}
					else {
						if (command.Client.StatsDatabase.SetPrimary(command.Args[1], command.Args[2]))
						{
							command.Reply("Done.");
						}
						else {
							command.ReturnMessage("Name entry not found. Did you spell the username correctly?");
						}
					}
					break;
				case "-s":
					string data;
					if (command.Args.Length > 3)
					{
						data = string.Join(" ", command.Args.Skip(2));
					}
					else {
						data = command.Args[2];
					}

					throw new NotImplementedException("Runtime modification of the settings file is not supported yet.");

				//TODO: Allow runtime modification of settings file
				/*if (Settings.Instance.SettingExists(command.Args[1])) {
					command.Reply(command.Args[1] + " set to " + data);
				} else {
					command.ReturnMessage("New key \"{0}\" created. Value set to {1}", command.Args[1], data);
				}*/
				default:
					command.ReturnMessage("The property \"{0}\" does not exist.", command.Args[0]);
					break;
			}
		}
	}
}
