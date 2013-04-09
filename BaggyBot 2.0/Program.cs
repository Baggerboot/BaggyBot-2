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
		private string commandIdentifier = "-";

		public const string Version = "pre2.0";

		public Program()
		{
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);

			sqlConnector = new SqlConnector();
			if (sqlConnector.OpenConnection()) {
				Logger.Log("Succesfully connected to the database.");
			} else {
				Logger.Log("Failed to connect to the database!");
				return;
			}
			sqlConnector.InitializeDatabase();

			client = new IrcClient();
			dataFunctionSet = new DataFunctionSet(sqlConnector);
			sHandler = new StatsHandler(dataFunctionSet, sqlConnector, client);
			commandHandler = new CommandHandler(client.SendMessage, sqlConnector,dataFunctionSet);

			client.OnNickChanged += sHandler.HandleNickChange;
			client.OnMessageReceived += ProcessMessage;
			client.OnRawLineReceived += ProcessRawLine;
		}

		private void ProcessRawLine(string line)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("[RAW]\t"+line);
			Console.ResetColor();
		}

		internal void Connect()
		{
			Logger.Log("Connecting to the IRC server");
			try {
				client.Connect("irc.esper.net", 6669, "BaggyBetaBot", "Dredger2", "BaggyBot Beta");
				client.JoinChannel("#baggy");
				Logger.Log("Connection established.");
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}
			
		}

		private void ProcessMessage(IrcMessage message)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("[MSG]\t" + message.Sender.Nick+ ": " + message.Message);
			Console.ResetColor();
			if (message.Message.StartsWith(commandIdentifier)) {
				commandHandler.ProcessCommand(message);
			} else {
				sHandler.ProcessMessage(message);
			}
		}


		static void Main(string[] args)
		{
			Console.WriteLine("Clear the log file? (y/n)");
			if (Console.ReadKey().KeyChar == 'y') {
				Logger.ClearLog();
				Console.WriteLine();
			}
			new Program().Connect();
		}
	}
}
