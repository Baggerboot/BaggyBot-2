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
				{"ns", new NickServ(ircInterface, dataFunctionSet)}
			};
		}

		internal void ProcessCommand(IRCSharp.IrcMessage message)
		{
			if(message.Message.Equals(Program.commandIdentifier)) return;

			string command = message.Message.Substring(1);
			string[] args = command.Split(' ');
			string primary = args[0];

			// Don't bother parsing the message if it's not a command
			if (!commands.ContainsKey(primary)) {
				return;
			}

			if(args.Length > 1){
				args = args.Skip(1).ToArray();
			}else{
				args = null;
			}
			Command cmd = new Command(primary, args, message.Sender);

			commands[primary].Use(cmd);
		}
	}
}
