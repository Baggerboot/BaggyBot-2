using System.Linq;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Set : ICommand
	{
		private readonly DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Set(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length < 3) {
				command.Reply("Usage: -set <property> [key] <value>");
				return;
			}
			switch (command.Args[0]) {
				case "name":
					int uid;
					if (int.TryParse(command.Args[1], out uid)) {
						dataFunctionSet.SetPrimary(uid, command.Args[2]);
						command.Reply("Done.");
					} else {
						if (dataFunctionSet.SetPrimary(command.Args[1], command.Args[2])) {
							command.Reply("Done.");
						} else {
							command.ReturnMessage("Name entry not found. Did you spell the username correctly?");
						}
					}
					break;
				case "-s":
					string data;
					if (command.Args.Length > 3) {
						data = string.Join(" ", command.Args.Skip(2));
					} else {
						data = command.Args[2];
					}
					Settings.Instance[command.Args[1]] = data;
					if (Settings.Instance.SettingExists(command.Args[1])) {
						command.Reply(command.Args[1] + " set to " + data);
					} else {
						command.ReturnMessage("New key \"{0}\" created. Value set to {1}", command.Args[1], data);
					}
					break;
				default:
					command.ReturnMessage("The property \"{0}\" does not exist.", command.Args[0]);
					break;
			}
		}
	}
}
