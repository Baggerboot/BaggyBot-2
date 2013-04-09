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
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Standard);

			sqlConnector = new SqlConnector();
			if (sqlConnector.OpenConnection()) {
				Logger.Log("Succesfully connected to the database.");
			} else {
				Logger.Log("Failed to connect to the database!");
				return;
			}
			sqlConnector.InitializeDatabase();

			client = new IrcClient();
			client.OnMessageReceived += ProcessMessage;
			dataFunctionSet = new DataFunctionSet(sqlConnector);
			sHandler = new StatsHandler(dataFunctionSet, sqlConnector);
			commandHandler = new CommandHandler(client.SendMessage, sqlConnector,dataFunctionSet);
		}

		internal void Connect()
		{
			Logger.Log("Connecting to the IRC server");
			try {
				client.Connect("irc.esper.net", 6669, "BaggyBetaBot", "Dredger2", "BaggyBot Beta");
				client.JoinChannel("#fofftopic");
				Logger.Log("Connection established.");
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}
			
		}

		private void ProcessMessage(IrcMessage message)
		{
			Console.WriteLine(message.ToString());
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
