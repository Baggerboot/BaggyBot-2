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
			if (command.Args.Length != 3) {
				ircInterface.SendMessage(command.Channel, "Usage: -set <property> <key> <value>");
				return;
			}
			switch (command.Args[0]) {
				case "name":
					int uid;
					if (int.TryParse(command.Args[1], out uid)) {
						dataFunctionSet.SetPrimary(uid, command.Args[2]);
						ircInterface.SendMessage(command.Channel, "Done.");
					}
					break;
				case "-s":
					Settings.Instance[command.Args[1]] = command.Args[2];
					ircInterface.SendMessage(command.Channel, command.Args[1] + " set to " + command.Args[2]);
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That property doesn't exist.");
					break;
			}
		}
	}
}
