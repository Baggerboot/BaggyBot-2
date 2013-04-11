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
				{"ns", new NickServ(ircInterface)}
			};

		}

		internal void ProcessCommand(IRCSharp.IrcMessage message)
		{
			if (!message.Sender.Ident.Equals("~baggerboo")) {
				ircInterface.SendMessage(message.Channel, "You are not authorized to use commands.");
			}

			string command = message.Message.Substring(1);
			string[] args = command.Substring(command.IndexOf(' ')).Split(' ');
			if (command.Equals("test")) {
				int[] uids = dataFunctionSet.GetUids(message.Sender);
				ircInterface.SendMessage(message.Channel, "Your uid is " + string.Join(",", uids.Select(x => x.ToString()).ToArray()));
			} else if (command.Equals("ns")) {
				ircInterface.SendMessage(message.Channel, "Your NickServ is " + ircInterface.DoNickservCall(message.Sender.Nick));
			} else if (command.Equals("join")) {
				ircInterface.JoinChannel(args[1]);
			}
		}
	}
}
