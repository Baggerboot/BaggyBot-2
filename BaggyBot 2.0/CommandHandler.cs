using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BaggyBot.Commands;

namespace BaggyBot
{
	class CommandHandler
	{
		private SqlConnector sqlConnector;
		private DataFunctionSet dataFunctionSet;
		private IrcInterface ircInterface;

		private Dictionary<string, ICommand> commands;

		public CommandHandler(IrcInterface inter, SqlConnector sc, DataFunctionSet ds)
		{
			ircInterface = inter;
			sqlConnector = sc;
			dataFunctionSet = ds;

			commands = new Dictionary<string, ICommand>()
			{
				{"ns", new NickServ(ircInterface, dataFunctionSet, sqlConnector)},
				{"crash", new Crash(ircInterface, dataFunctionSet)},
				{"join", new Join(ircInterface, dataFunctionSet)},
				{"elycool", new Elycool(ircInterface)},
				{"help", new Help(ircInterface)},
				{"resolve", new Resolve(ircInterface)},
				{"snag", new Snag(ircInterface)},
				{"query", new Query(ircInterface, sqlConnector)},
				{"shutdown", new Shutdown(ircInterface)}
			};
		}

		internal void ProcessCommand(IRCSharp.IrcMessage message)
		{
			if(message.Message.Equals(Program.commandIdentifier)) return;

			string line = message.Message.Substring(1);
			string[] args = line.Split(' ');
			string command = args[0];

			// Don't bother parsing the message if it's not a command
			if (!commands.ContainsKey(command)) {
				return;
			}

			args = args.Skip(1).ToArray();

			CommandArgs cmd = new CommandArgs(command, args, message.Sender, message.Channel);

			if (line.ToLower().Equals("help") || line.ToLower().Equals("about") || line.ToLower().Equals("info") || line.ToLower().Equals("baggybot")) {
				ircInterface.SendMessage(message.Channel,"BaggyBot " + Program.Version + " -- Stats page: http://www.jgeluk.net/stats -- Made by baggerboot. For help, try the -help command.");
			}

			if (commands[command].Permissions == PermissionLevel.All || commands[command].Permissions == PermissionLevel.BotOperator && Tools.UserTools.Validate(message.Sender)) {
				commands[command].Use(cmd);
			} else {
				ircInterface.SendMessage(message.Channel, Messages.CMD_NOT_AUTHORIZED);
			}
		}
	}
}
