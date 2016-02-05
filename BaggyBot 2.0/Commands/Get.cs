using System;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Get : ICommand
	{
		private readonly DataFunctionSet dataFunctionSet;
		private readonly IrcInterface ircInterface;
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
				// TODO: Allow settings lookup for new settings format
				throw new NotImplementedException("Dynamic YAML settings lookup is not supported yet.");
				//result = Settings.Instance[command.Args[1]];
				if (result != null) {
					command.Reply("value for {0}: {1}", command.Args[1], result);
					return;
				}
			}
			switch (command.Args[0]) {
				case "uid":
					var nick = command.Args.Length > 1 ? command.Args[1] : command.Sender.Nick;
					var uid = dataFunctionSet.GetIdFromNick(nick);
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
