using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaggyBot
{
	public delegate void MessageSender(string target, string message);

	class CommandHandler
	{
		private MessageSender sendMessage;
		private SqlConnector sqlConnector;
		private DataFunctionSet dataFunctionSet;

		public CommandHandler(MessageSender ms, SqlConnector sc, DataFunctionSet ds)
		{
			sendMessage += ms;
			sqlConnector = sc;
			dataFunctionSet = ds;
		}
		public void ProcessCommand(IRCSharp.IrcMessage message)
		{
			if (!message.Sender.Ident.Equals("~baggerboo")) {
				sendMessage(message.Channel, "You are not authorized to use commands.");
			}

			string command = message.Message.Substring(1);
			if (command.Equals("test")) {
				uint[] uids = dataFunctionSet.GetUids(message.Sender);
				sendMessage(message.Channel, "Your uid is " + uids);
			}
		}
	}
}
