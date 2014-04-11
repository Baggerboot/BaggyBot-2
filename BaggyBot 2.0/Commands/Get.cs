using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Get : ICommand
	{
		private DataFunctionSet dataFunctionSet;
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Get(DataFunctionSet df, IrcInterface ircInterface )
		{
			dataFunctionSet = df;
			this.ircInterface = ircInterface;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length >2) {
				command.Reply("Usage: -get <property> <key>");
				return;
			}
			if (command.Args.Length == 2 && command.Args[0] == "-s") {
				string result = null;
				if (command.Args[1] == "sql_connection_string") {

				}
				result = Settings.Instance[command.Args[1]];
				if (result != null) {
					command.Reply("value for {0}: {1}", command.Args[1], result);
					return;
				}
			}
			switch (command.Args[0]) {
				case "uid":
					string nick = command.Args.Length > 1 ? command.Args[1] : command.Sender.Nick;
					int uid = dataFunctionSet.GetIdFromNick(nick);
					command.Reply("Your user Id is " + uid);
					break;
				case "users":
					if (command.Args.Length != 2) {
						command.Reply("usage: -get users <#channel>");
					} else {
						if (ircInterface.InChannel(command.Args[1])) {
							command.Reply("users in {0}: {1}", command.Args[1], string.Join(", ", ircInterface.GetUsers(command.Args[1])));
						} else {
							command.Reply("I am not in that channel");
						}
					}
					break;
				default:
					command.ReturnMessage("That is not a valid property.");
					break;
			}

		}
	}
}
