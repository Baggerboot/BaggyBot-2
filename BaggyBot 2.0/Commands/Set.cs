using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Set : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Set(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length < 3) {
				ircInterface.SendMessage(command.Channel, "Usage: -set <property> [key] <value>");
				return;
			}
			switch (command.Args[0]) {
				case "name":
					int uid;
					if (int.TryParse(command.Args[1], out uid)) {
						dataFunctionSet.SetPrimary(uid, command.Args[2]);
						ircInterface.SendMessage(command.Channel, "Done.");
					} else {
						if (dataFunctionSet.SetPrimary(command.Args[1], command.Args[2])) {
							ircInterface.SendMessage(command.Channel, "Done.");
						} else {
							ircInterface.SendMessage(command.Channel, "Name entry not found. Did you spell the username correctly?");
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
						ircInterface.SendMessage(command.Channel, command.Args[1] + " set to " + data);
					} else {
						ircInterface.SendMessage(command.Channel, string.Format("New key \"{0}\" created. Value set to {1}", command.Args[1], data));
					}
					
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That property doesn't exist.");
					break;
			}
		}
	}
}
