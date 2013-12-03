using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Get : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Get(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length >2) {
				ircInterface.SendMessage(command.Channel, "Usage: -get <property> <key>");
				return;
			}
			if (command.Args.Length == 2 && command.Args[0] == "-s") {
				string result = null;
				if (command.Args[1] == "sql_connection_string") {

				}
				result = Settings.Instance[command.Args[1]];
				if (result != null) {
					ircInterface.SendMessage(command.Channel, String.Format("value for {0}: {1}", command.Args[1], result));
					return;
				}
			}
			switch (command.Args[0]) {
				case "uid":
					string nick = command.Args.Length > 1 ? command.Args[1] : command.Sender.Nick;
					int uid = dataFunctionSet.GetIdFromNick(nick);
					ircInterface.SendMessage(command.Channel, "Your user Id is " + uid);
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That is not a valid property.");
					break;
			}

		}
	}
}
