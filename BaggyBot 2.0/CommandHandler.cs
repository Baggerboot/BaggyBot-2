using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BaggyBot.Commands;
using BaggyBot.Database;

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
				{"crash", new Crash(ircInterface, dataFunctionSet)},
				{"elycool", new Elycool(ircInterface)},
				{"ed", new ExceptionDetails(ircInterface)},
				{"get", new Get(ircInterface, dataFunctionSet)},
				{"help", new Help(ircInterface)},
				{"join", new Join(ircInterface, dataFunctionSet)},
				{"ns", new NickServ(ircInterface, dataFunctionSet, sqlConnector)},
				{"nuke", new Nuke(ircInterface, dataFunctionSet)},
				{"part", new Part(ircInterface)},
				{"ping", new Ping(ircInterface)},
				{"resolve", new Resolve(ircInterface)},
				{"say", new Say(ircInterface)},
				{"set", new Set(ircInterface, dataFunctionSet)},
				{"sqlreconnect", new SqlReconnect(ircInterface, sqlConnector)},
				{"shutdown", new Shutdown(ircInterface, sqlConnector)},
				{"snag", new Snag(ircInterface)},
				{"update", new Update(ircInterface, sqlConnector)},
				{"version", new Commands.Version(ircInterface)}
			};
		}

		internal void ProcessCommand(IRCSharp.IrcMessage message)
		{
			if(message.Message.Equals(Program.commandIdentifier)) return;

			string line = message.Message.Substring(1);
			
			if (line.ToLower().Equals("help") || line.ToLower().Equals("about") || line.ToLower().Equals("info") || line.ToLower().Equals("baggybot")) {
				ircInterface.SendMessage(message.Channel,"BaggyBot " + Program.Version + " -- Stats page: http://www.jgeluk.net/stats -- Made by baggerboot. For help, try the -help command.");
			}

			string[] args = line.Split(' ');
			string command = args[0];
			args = args.Skip(1).ToArray();
			CommandArgs cmd = new CommandArgs(command, args, message.Sender, message.Channel, line.Substring(line.IndexOf(' ')+1));

			if (!commands.ContainsKey(command)) {
				return;
			}

			if (commands[command].Permissions == PermissionLevel.All || commands[command].Permissions == PermissionLevel.BotOperator && Tools.UserTools.Validate(message.Sender)) {
				try {
					commands[command].Use(cmd);
				} catch (Exception e) {
					ircInterface.SendMessage(message.Channel, "An unhandled exception occured while trying to process your command! Exception details: " + e.Message);
					Program.Exceptions.Add(e);
				}
			} else {
				ircInterface.SendMessage(message.Channel, Messages.CMD_NOT_AUTHORIZED);
			}
		}
	}
}
