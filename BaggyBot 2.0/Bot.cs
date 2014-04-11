using System;
using System.Collections.Generic;
using System.Linq;

using IRCSharp;
using BaggyBot.Database;
using BaggyBot.Tools;
using BaggyBot.DataProcessors;
using System.Threading;
using System.Threading.Tasks;

namespace BaggyBot
{
	public class Bot : IDisposable
	{
		// Processes all incoming data for which statistics should be generated
		private StatsHandler statsHandler;
		// Processes all incoming messages that look like commands, and executes them if they are commands
		private CommandHandler commandHandler;
		// Provides an interface to an SQL Entity Provider
		private SqlConnector sqlConnector;
		// Abstraction over SqlConnector for database operations
		private DataFunctionSet dataFunctionSet;
		private IrcClient client;
		// Provides an interface to the IRC client for sending data
		private IrcInterface ircInterface;
		// Handles IRC events, passing them on the the appropriate data processors if neccessary
		private IrcEventHandler ircEventHandler;
		// Collects performance statistics
		private BotDiagnostics botDiagnostics;

		// Any message prefixed with this character will be interpreted as a command
		public const string commandIdentifier = "-";
		// The versioning system used is Revision.Update.Bugfix, where 'revision' means a large revision of the application's inner workings,
		// often coupled with a change in the environment that the bot functions in. Example: changing the way the database is structured,
		// changing the platforms the bot can run on, etc.
		// Any change that exposes new features to the users of the bot (including the administrator) counts as an update.
		// Any update which doesn't add new features, and therefore only fixes issues with the bot or its dependencies is considered a bugfix.
		public const string Version = "3.29.6";

		public bool QuitRequested
		{
			get;
			private set;
		}

		public static DateTime LastUpdate
		{
			get
			{
				return Tools.MiscTools.RetrieveLinkerTimestamp();
			}
		}

		// If the bot is started in update mode, a previous version has to be specified.
		// The bot will then announce whether the update was a success or a failure.
		// To determine this, the previous version is stored in here.
		// If the bot is not started in update mode, the value of this field remains null.
		private string previousVersion = null;

		public Bot()
		{
			Logger.ClearLog();
			CheckSettingsFile();

			previousVersion = Settings.Instance["version"];
			Console.Title = "BaggyBot Statistics Collector version " + Version;
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);
			if (previousVersion != null && previousVersion != Version) {
				Logger.Log("Updated from version {0} to version {1}", LogLevel.Info, true, previousVersion, Version);
			}
			Settings.Instance["version"] = Version;

			// Create various data processors and I/O handlers
			sqlConnector = new SqlConnector();
			client = new IrcClient(Settings.Instance["deployed"] == "false");
			ircInterface = new IrcInterface(client);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			ircInterface.dataFunctionSet = dataFunctionSet;
			statsHandler = new StatsHandler(dataFunctionSet, ircInterface);
			Tools.UserTools.DataFunctionSet = dataFunctionSet;
			botDiagnostics = new BotDiagnostics(ircInterface);
			commandHandler = new CommandHandler(ircInterface, dataFunctionSet, this, botDiagnostics);
			ircEventHandler = new IrcEventHandler(dataFunctionSet, ircInterface, commandHandler, statsHandler);

			HookupIrcEvents();
		}

		private void CheckSettingsFile()
		{
			var s = Settings.Instance;
			if (s.NewFileCreated) {
				s.FillDefault();
				Logger.Log("New settings file created ({0}). Please fill it with the correct data, then start the bot again. BaggyBot will now exit.", LogLevel.Info, true, Logger.LogFileName);
				Environment.Exit(0);
			}
		}

		~Bot()
		{
			Console.WriteLine("Shutting down bot instance (version {0})", Version);
		}


		private void HookupIrcEvents()
		{
			client.OnNickChanged += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ircEventHandler.ProcessMessage;
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			//client.OnRawLineReceived += ircEventHandler.ProcessRawLine;
			client.OnNoticeReceived += ircEventHandler.ProcessNotice;
			client.OnDisconnect += HandleDisconnect;
			client.OnKicked += (channel, reason, kicker) => Logger.Log("I was kicked from {0} by {1} ({2})", LogLevel.Warning, true, channel, kicker.Nick, reason);
			client.OnDebugLog += message => MiscTools.ConsoleWriteLine("[ILB]\t" + message, ConsoleColor.DarkCyan);
			client.OnNetLibDebugLog += message => MiscTools.ConsoleWriteLine("[NET]\t" + message, ConsoleColor.Cyan);
			client.OnJoinChannel += ircEventHandler.HandleJoin;
			client.OnPartChannel += ircEventHandler.HandlePart;
			client.OnKick += ircEventHandler.HandleKick;
			client.OnNickChanged += ircEventHandler.HandleNickChange;
			client.OnQuit += ircEventHandler.HandleQuit;
		}

		private void ConnectDatabase()
		{
			Logger.Log("Connecting to the database", LogLevel.Info);
			if (sqlConnector.OpenConnection())
				Logger.Log("Database connection established", LogLevel.Info);
			else
				Logger.Log("Database connection not established. Bot functionality will be very limited.", LogLevel.Warning);
		}

		/// <summary>
		/// Connects the bot to the IRC server
		/// </summary>
		private bool ConnectIrc()
		{
			Settings s = Settings.Instance;
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];
			string realname = s["irc_realname"];
			string host = "localhost";
			int port = 6667;

			this.client.AddOrCreateClient(null);

			Logger.Log("Connecting to the IRC server..");
			Logger.Log("\tNick\t" + nick);
			Logger.Log("\tIdent\t" + ident);
			Logger.Log("\tName\t" + realname);
			Logger.Log("\tHost\t" + host);
			Logger.Log("\tPort\t" + port);
			try {
				this.client.Connect(host, port, nick, ident, realname, true);
				return true;
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			} catch (ArgumentException e) {
				Logger.Log("Failed to connect to the IRC server: The settings file does not contain a value for \"{0}\"", LogLevel.Error, true, e.ParamName);
				return false;
			}
		}

		public void Reconnect(IEnumerable<string> channels)
		{
			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];
			string realname = s["irc_realname"];
			string firstChannel = s["irc_initial_channel"];
			try {
				client.Connect(server, port, nick, ident, realname);
				// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
				// If this is the case, IRCSharp will continue trying to join until it succeeds, blocking the call in the meantime.
				JoinChannels(channels);
				ircInterface.ChangeClient(client);
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return;
			}
		}

		public void JoinChannels(params string[] channels)
		{
			JoinChannels(channels.AsEnumerable());
		}
		public void JoinChannels(IEnumerable<string> channels)
		{
			var joins = new Task[channels.Count()];

			int i = 0;
			foreach (var c in channels) {
				joins[i] = (Task.Run(() => client.JoinChannel(c)));
				i++;
			}
			try{
				if (!Task.WaitAll(joins, 30000)) {
					var failedJoins = joins.Where((join) => !join.IsCompleted);
					Logger.Log("Join timeout: failed to join {0}", LogLevel.Warning, true, string.Join(", ", failedJoins));
				}
			}catch(AggregateException e){
				Logger.Log("One or more exceptions occurred while trying to join the following channel(s): {0}", LogLevel.Error, true, string.Join(", ", channels));
				foreach (var exception in e.InnerExceptions) {
					Logger.Log (" - {0}: \"{1}\" in {2}; stacktrace: {3}", LogLevel.Error, true, exception.GetType ().Name, exception.Message, exception.TargetSite, exception.StackTrace);
				}
				Logger.Log ("[End of exception list]", LogLevel.Error);
			}
		}

		public void OnPostConnect()
		{
			botDiagnostics.StartPerformanceLogging();

			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning) {
					if (ircInterface.Connected) {
						ircInterface.NotifyOperator("LOG WARNING/ERROR: " + message);
					}
				}
			};
		}

		/*private void Reconnect(int previousExitCode, string channels)
		{
			bool reconnected = false;
			do {
				try {
					client = new IrcClient();
					HookupIrcEvents();
					Reconnect(channels.Select(c => c.Name));

					reconnected = true;

					switch (reason) {
						case DisconnectReason.ServerDisconnect:
							Logger.Log("Reconnected to the IRC server after a connection loss", LogLevel.Warning);
							break;
						case DisconnectReason.PingTimeout:
							Logger.Log("Reconnected to the IRC server after a ping timeout", LogLevel.Warning);
							break;
						default:
							Logger.Log("Reconnected to the IRC server after a connection loss ({0})", LogLevel.Warning, true, reason.ToString());
							break;
					}
				} catch (System.Net.Sockets.SocketException) {
					Logger.Log("Failed to reconnect. Retrying in 2 seconds.", LogLevel.Info);
					// Wait a while before attempting to reconnect. This prevents the bot from flooding the IRC server with connection requests
					// in case the connection request fails.
					System.Threading.Thread.Sleep(2000);
				}
			} while (!reconnected);
		}*/

		private void HandleDisconnect(DisconnectReason reason)
		{
			if (reason == DisconnectReason.DisconnectOnRequest) {
				Logger.Log("Disconnected from IRC server.", LogLevel.Info);
				QuitRequested = true;
			} else {
				Logger.Log("Connection lost ({0}) Attempting to reconnect...", LogLevel.Warning, true, reason.ToString());
				Environment.Exit(50);
			}
		}

		public void Shutdown()
		{
			QuitRequested = true;
		}

		public void Dispose()
		{
			botDiagnostics.Dispose();
			sqlConnector.Dispose();
		}

		public void Attach(int exitCode, string mainChannel, string serializedClientState)
		{
			ConnectDatabase();
			OnPostConnect();

			ClientState state = (ClientState) Tools.MiscTools.DeserializeObject(serializedClientState);

			this.client.Attach(state);

			if (exitCode == 100) {
				if (previousVersion != null && previousVersion != Version) {
					ircInterface.SendMessage(mainChannel, "Succesfully updated from version " + previousVersion + " to version " + Version);
				} else {
					ircInterface.SendMessage(mainChannel, "Failed to update: no newer version available (current version: " + Version + ")");
				}
			} else if(exitCode == 200){
				ircInterface.SendMessage(mainChannel, "Restarted successfully.");
			}else{
				ircInterface.NotifyOperator("Reconnected to the IRC server after a connection loss.");
			}
			EnterMainLoop();
		}

		private void EnterMainLoop()
		{
			while (!QuitRequested) {
				Thread.Sleep(1000);
			}
			Logger.Log("Preparing to shut down", LogLevel.Info);
			ircInterface.Disconnect("Shutting down");
			sqlConnector.CloseConnection();
			Logger.Log("Closed SQL server connection", LogLevel.Info);
			sqlConnector.Dispose();
			Logger.Log("Disposed SQL server connection object", LogLevel.Info);
			Logger.Dispose();
			Console.WriteLine("Goodbye.");
			Environment.Exit(0);
		}

		private void TryConnectIrc()
		{
			if (!ConnectIrc()) {
				Logger.Log("FATAL: IRC Connection failed. Application will now exit.", LogLevel.Error);
				Environment.Exit(1);
			}
		}

		private void JoinInitialChannel()
		{
			string firstChannel = Settings.Instance["irc_initial_channel"];
			// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
			if (string.IsNullOrWhiteSpace(firstChannel)) {
				Logger.Log("Unable to join the initial channel: settings value for irc_initial_channel not set.", LogLevel.Error);
			}else{
				JoinChannels(firstChannel);
				Logger.Log("Ready to collect statistics in " + firstChannel, LogLevel.Info);
			}
		}

		public Dictionary<string, IrcChannel> Detach()
		{
			var channels = client.Detach();
			sqlConnector.Dispose();
			Logger.Dispose();
			botDiagnostics.Dispose();
			return channels;
		}

		public void Connect()
		{
			Task dbConTask = Task.Run(() => ConnectDatabase());
			Task ircConTask = Task.Run(() => ConnectIrc());
			Task.WaitAll(dbConTask, ircConTask);

			JoinInitialChannel();
			OnPostConnect();

			if (previousVersion != null && previousVersion != Version) {
				ircInterface.NotifyOperator("Succesfully updated from version " + previousVersion + " to version " + Version);
			}
			EnterMainLoop();
		}


		void IDisposable.Dispose()
		{
			Logger.Log("Disposing");
		}

		internal void RequestUpdate(string channel, bool updateFiles)
		{
			var commandClient = new CsNetLib2.NetLibClient(CsNetLib2.TransferProtocolType.Delimited, System.Text.Encoding.UTF8);
			commandClient.Connect("localhost", 6668);

			var state = client.GetClientState();
			
			string data = Tools.MiscTools.SerializeObject(state);

			if(data == null){
				Logger.Log("Unable to serialize client state: Client state not marked as serializable.", LogLevel.Error);
				return;
			}

			commandClient.Send("UPDATE " + channel + " " + data);

			if (updateFiles) {
				Environment.Exit(100);
			} else {
				Environment.Exit(200);
			}
		}

		public static void Main(string[] args)
		{
			if (args.Length == 3) {
				using (Bot bot = new Bot()) {
					Logger.Log("Reattaching to host...");
					bot.Attach(int.Parse(args[0]), args[1], args[2]);
				}
			} else {
				using (Bot bot = new Bot()) {
					Logger.Log("Starting new bot instance...");
					bot.Connect();
				}
			}


		}
	}
}
