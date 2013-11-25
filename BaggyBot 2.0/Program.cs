using System;
using System.Collections.Generic;
using System.Linq;

using IRCSharp;
using BaggyBot.Database;
using BaggyBot.Tools;

namespace BaggyBot
{
	public class Program
	{
		private StatsHandler statsHandler;
		private CommandHandler commandHandler;
		private SqlConnector sqlConnector;
		private DataFunctionSet dataFunctionSet;
		private IrcClient client;
		private IrcInterface ircInterface;
		private IrcEventHandler ircEventHandler;
		private BotDiagnostics botDiagnostics;

		public const string commandIdentifier = "-";
		public const string Version = "3.12.3";

		public static DateTime LastUpdate; // = new DateTime(2013, 11, 16, 4, 9, 18, DateTimeKind.Local);

		private string previousVersion = null;

		public void Shutdown()
		{
			Logger.Log("Preparing to shut down", LogLevel.Info);
			ircInterface.Disconnect("Shutting down");
			Logger.Log("Disconnected from IRC server", LogLevel.Info);
			sqlConnector.CloseConnection();
			Logger.Log("Closed SQL server connection", LogLevel.Info);
			sqlConnector.Dispose();
			Logger.Log("Disposed SQL server connection object", LogLevel.Info);
			Logger.Dispose();
			Environment.Exit(0);
		}

		private void HandleConnectionLoss()
		{
			Logger.Log("Connection lost. Attempting to reconnect...", LogLevel.Info);
			bool reconnected = false;
			do {
				System.Threading.Thread.Sleep(2000);
				try {
					client.Reconnect();
					reconnected = true;
				} catch (System.Net.Sockets.SocketException) {
					Logger.Log("Failed to reconnect. Retrying in 2 seconds.", LogLevel.Info);
				}
			} while (!reconnected);
		}

		public Program(string previousVersion = null)
		{
			this.previousVersion = previousVersion;
			Console.Title = "BaggyBot Statistics Collector version " + Version;
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);

			LastUpdate = Tools.MiscTools.RetrieveLinkerTimestamp();
			sqlConnector = new SqlConnector();
			client = new IrcClient();
			ircInterface = new IrcInterface(client, dataFunctionSet);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			statsHandler = new StatsHandler(dataFunctionSet, ircInterface);
			BaggyBot.Tools.UserTools.DataFunctionSet = dataFunctionSet;
			botDiagnostics = new BotDiagnostics(ircInterface);
			commandHandler = new CommandHandler(ircInterface, sqlConnector, dataFunctionSet, this, botDiagnostics);
			ircEventHandler = new IrcEventHandler(dataFunctionSet, ircInterface, commandHandler, statsHandler);

			client.OnNickChanged += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ircEventHandler.ProcessMessage;
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			client.OnNoticeReceived += ircEventHandler.ProcessNotice;
			client.OnDisconnect += () => { Logger.Log("Disconnected.", LogLevel.Debug); };
			client.OnConnectionLost += HandleConnectionLoss;
			client.OnKicked += (channel) => { Logger.Log("I was kicked from " + channel, LogLevel.Warning); };
			client.OnDebugLog += (message) => { MiscTools.ConsoleWriteLine("[INF]\t" + message, ConsoleColor.DarkCyan); };
			client.OnJoinChannel += ircEventHandler.HandleJoin;
			client.OnPartChannel += ircEventHandler.HandlePart;
			client.OnKick += ircEventHandler.HandleKick;
			client.OnNickChanged += ircEventHandler.HandleNickChange;

			Logger.Log("Connecting to the database", LogLevel.Info);
			sqlConnector.OpenConnection();
			Logger.Log("Database connection established", LogLevel.Info);
		}

		public void PostConnect()
		{
			botDiagnostics.StartPerformanceLogging();

			if (previousVersion != null && previousVersion != Version) {
				ircInterface.SendMessage(Settings.Instance["operator_nick"], "Succesfully updated from version " + previousVersion + " to version " + Version);
			} else if (previousVersion != null) {
				ircInterface.SendMessage(Settings.Instance["operator_nick"], "Failed to update: No newer version available. Previous version: " + previousVersion);
			}
			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning) {
					ircInterface.SendMessage(Settings.Instance["operator_nick"], "LOG WARNING: " + message);
				}
			};
		}

		public void Connect()
		{
			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];

			string previousIdent = null;
			using (System.IO.StreamReader sr = new System.IO.StreamReader("identfile")) {
				previousIdent = sr.ReadLine();
			}
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter("identfile", false)) {
				sw.WriteLine(ident);
			}

			string realname = s["irc_realname"];
			string firschannel = s["irc_initial_channel"];

			try {
				client.Connect(server, port, nick, ident, realname);
				do {
					client.JoinChannel(firschannel);
					System.Threading.Thread.Sleep(1000);
				} while (!client.InChannel(firschannel));
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter("identfile", false)) {
				sw.WriteLine(previousIdent);
			}
			Logger.Log("Ready to collect statistics in " + firschannel, LogLevel.Info);
			PostConnect();
		}

		static void Main(string[] args)
		{
			string previousVersion = null;
			for (int i = 0; i < args.Length; i++) {
				switch (args[i]) {
					case "-pv":
						previousVersion = args[i + 1];
						i++;
						break;
				}
			}
			Logger.ClearLog();

			Program p = new Program(previousVersion);
			p.Connect();
		}
	}
}
