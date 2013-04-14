using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	class Program
	{
		private IrcClient client;
		private StatsHandler sHandler;
		private SqlConnector sqlConnector;
		private DataFunctionSet dataFunctionSet;
		private CommandHandler commandHandler;
		private IrcInterface ircInterface;
		internal const string commandIdentifier = "-";

		internal const string Version = "2.0 Release Candidate 2";

		public Program()
		{
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);

			sqlConnector = new SqlConnector();
			client = new IrcClient();
			ircInterface = new IrcInterface(client, dataFunctionSet);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			sHandler = new StatsHandler(dataFunctionSet, ircInterface);
			commandHandler = new CommandHandler(ircInterface, sqlConnector,dataFunctionSet);
			BaggyBot.Tools.UserTools.DataFunctionSet = dataFunctionSet;

			client.OnNickChanged += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ProcessMessage;
			client.OnFormattedLineReceived += ProcessFormattedLine;

			sqlConnector.OpenConnection();
			ConfirmPurge();
			sqlConnector.InitializeDatabase();
		}

		private void ConfirmPurge()
		{
			ConsoleWriteLine("Purge the database? y/n", ConsoleColor.Blue);
			Console.ForegroundColor = ConsoleColor.Gray;
			var k = Console.ReadKey();
			if (k.KeyChar == 'y') {
				dataFunctionSet.PurgeDatabase();
			}
			Console.WriteLine();
		}

		private void ProcessFormattedLine(IrcLine line)
		{
			if (line.Command.Equals("NOTICE") && ircInterface.HasNickservCall && client.GetUserFromSender(line.Sender).Ident.Equals("NickServ")) {
				if (line.FinalArgument.StartsWith("Information on")) {
					string data = line.FinalArgument.Substring("Information on  ".Length);
					string nick = data.Substring(0, data.IndexOf(" ")-1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					string nickserv = data.Substring(0,data.Length-3);
					ircInterface.AddNickserv(nick.ToLower(),nickserv);
				} else if (line.FinalArgument.EndsWith("is not registered.")) {
					string nick = line.FinalArgument.Substring(1, line.FinalArgument.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ')-1);
					ircInterface.AddNickserv(nick, null);
				}
			}else if(line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick")){
				ircInterface.DisableNickservCalls();
			}else if(line.Command.Equals("311") && ircInterface.HasWhoisCall)
			{
				IrcUser user = new IrcUser(line.Arguments[1], line.Arguments[2], line.Arguments[3]);
				ircInterface.AddUser(user.Nick, user);
			}
			if (!line.Command.Equals("PRIVMSG")) {
				ConsoleWriteLine("[RAW]\t" + line, ConsoleColor.DarkGray);
			}
		}

		private static void ConsoleWriteLine(string line, ConsoleColor color = ConsoleColor.Gray)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(line);
		}

		private void ProcessRawLine(string line)
		{
			ConsoleWriteLine("[RAW]\t" + line);
		}

		internal void Connect()
		{
			Logger.Log("Connecting to the IRC server");

			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];
			string realname = s["irc_realname"];
			string firschannel = s["irc_initial_channel"];

			try {
				client.Connect(server,port, nick, ident, realname);
				ircInterface.TestNickServ();
				client.JoinChannel(firschannel);
				Logger.Log("Connection established.");
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}
		}

		private void ProcessMessage(IrcMessage message)
		{
			ConsoleWriteLine("[MSG]\t" + message.Sender.Nick+ ": " + message.Message, ConsoleColor.DarkGreen);
			if (message.Message.StartsWith(commandIdentifier)) {
				commandHandler.ProcessCommand(message);
			} else {
				sHandler.ProcessMessage(message);
			}
		}

		static void Main(string[] args)
		{
			ConsoleWriteLine("Clear the log file? (y/n)", ConsoleColor.Blue);
			Console.ForegroundColor = ConsoleColor.Gray;
			if (Console.ReadKey().KeyChar == 'y') {
				Logger.ClearLog();
				Console.WriteLine();
			}
			new Program().Connect();
		}
	}
}
