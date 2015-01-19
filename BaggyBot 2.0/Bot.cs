using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using IRCSharp;
using BaggyBot.Database;
using BaggyBot.Tools;
using BaggyBot.DataProcessors;
using System.Threading;
using System.Threading.Tasks;
using IRCSharp.IRC;

namespace BaggyBot
{
	public class Bot : IDisposable
	{
		// Provides an interface to an SQL Entity Provider
		private readonly SqlConnector sqlConnector;
		// Abstraction over SqlConnector for database operations
		private readonly DataFunctionSet dataFunctionSet;
		private IrcClient client;
		// Provides an interface to the IRC client for sending data
		private readonly IrcInterface ircInterface;
		// Handles IRC events, passing them on the the appropriate data processors if neccessary
		private readonly IrcEventHandler ircEventHandler;
		// Collects performance statistics
		private readonly BotDiagnostics botDiagnostics;

		// Any message prefixed with this character will be interpreted as a command
		public const string CommandIdentifier = "-";
		// The versioning system used is Revision.Update.Bugfix, where 'revision' means a large revision of the application's inner workings,
		// often coupled with a change in the environment that the bot functions in. Example: changing the way the database is structured,
		// changing the platforms the bot can run on, etc.
		// Any change that exposes new features to the users of the bot (including the administrator) counts as an update.
		// Any update which doesn't add new features, and therefore only fixes issues with the bot or its dependencies is considered a bugfix.
		public const string Version = "3.34.4";

		public bool QuitRequested
		{
			get;
			private set;
		}

	    public static DateTime LastUpdate
	    {
	        get { return MiscTools.RetrieveLinkerTimestamp(); }
	    }

	    // If the bot is started in update mode, a previous version has to be specified.
		// The bot will then announce whether the update was a success or a failure.
		// To determine this, the previous version is stored in here.
		// If the bot is not started in update mode, the value of this field remains null.
		private readonly string previousVersion;

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
			client = new IrcClient(/*Settings.Instance["deployed"] == "false"*/);
			ircInterface = new IrcInterface(client);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			ircInterface.dataFunctionSet = dataFunctionSet;
			var statsHandler = new StatsHandler(dataFunctionSet, ircInterface);
			UserTools.DataFunctionSet = dataFunctionSet;
			botDiagnostics = new BotDiagnostics(ircInterface);
			var commandHandler = new CommandHandler(ircInterface, dataFunctionSet, this);
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
			client.OnNickChange += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ircEventHandler.ProcessMessage;
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			//client.OnRawLineReceived += ircEventHandler.ProcessRawLine;
			client.OnNoticeReceived += ircEventHandler.ProcessNotice;
			client.OnDisconnect += HandleDisconnect;
			client.OnKicked += (channel, reason, kicker) => Logger.Log("I was kicked from {0} by {1} ({2})", LogLevel.Warning, true, channel, kicker.Nick, reason);
		    client.OnDebugLog += message => Logger.Log("[IC#]" + message, LogLevel.Info);
            client.OnNetLibDebugLog += message => Logger.Log("[NL#]" + message, LogLevel.Info);
			client.OnJoinChannel += ircEventHandler.HandleJoin;
			client.OnPartChannel += ircEventHandler.HandlePart;
			client.OnKick += ircEventHandler.HandleKick;
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
		private bool ConnectIrc(string host, int port)
		{
			var s = Settings.Instance;
			var nick = s["irc_nick"];
			var ident = s["irc_ident"];
			var realname = s["irc_realname"];

			Logger.Log("Connecting to the IRC server..");
			Logger.Log("\tNick\t" + nick);
			Logger.Log("\tIdent\t" + ident);
			Logger.Log("\tName\t" + realname);
			Logger.Log("\tHost\t" + host);
			Logger.Log("\tPort\t" + port);
			try {
				client.Connect(host, port, nick, ident, realname);
				return true;
			} catch (SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			} catch (ArgumentException e) {
				Logger.Log("Failed to connect to the IRC server: The settings file does not contain a value for \"{0}\"", LogLevel.Error, true, e.ParamName);
				return false;
			} catch (Exception e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
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

		private void HandleDisconnect(DisconnectReason reason, Exception ex)
		{
			if (reason == DisconnectReason.DisconnectOnRequest) {
				Logger.Log("Disconnected from IRC server.", LogLevel.Info);
				QuitRequested = true;
			} else {
				if (reason == DisconnectReason.Other) {
					Logger.Log("Connection lost ({0}: {1}) Attempting to reconnect...", LogLevel.Error, true, ex.GetType().Name, ex.Message);
				} else {
					Logger.Log("Connection lost ({0}) Attempting to reconnect...", LogLevel.Warning, true, reason.ToString());
				}
				var state = client.GetClientState();

				bool success;

				do {
					client = new IrcClient(/*Settings.Instance["deployed"] == "false"*/);
					ircInterface.ChangeClient(client);
					HookupIrcEvents();

					success = ConnectIrc(state.RemoteHost, state.RemotePort);
					if (success) continue;
					Logger.Log("Reconnection attempt failed. Retrying in 5 seconds.", LogLevel.Warning, true, reason.ToString());
					Thread.Sleep(5000);
				} while (!success);

				Logger.Log("Successfully reconnected to the server!", LogLevel.Warning, true, reason.ToString());
				
				foreach (var channel in state.Channels.Where(channel => !client.JoinChannel(channel.Key)))
				{
					Logger.Log("Failed to rejoin {0}! Skipping this channel.", LogLevel.Error, true, channel.Key);
				}
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

		public void Rejoin(string serializedClientState, string mainChannel)
		{
			var state = (ClientState)MiscTools.DeserializeObject(serializedClientState);
			if (state.Channels == null)
			{
				Logger.Log("Unable to read the channel list. I will not be able to rejoin my previous channels.", LogLevel.Error);
			}
			else
			{
				foreach (var channel in state.Channels)
				{
					if (!client.JoinChannel(channel.Key))
					{
						Logger.Log("Failed to rejoin {0}! Skipping this channel.", LogLevel.Error, true, channel.Key);
					}
				}
			}
			if (previousVersion != null && previousVersion != Version) {
				ircInterface.SendMessage(mainChannel, "Succesfully updated from version " + previousVersion + " to version " + Version);
			} else {
				ircInterface.SendMessage(mainChannel, "Failed to update: no newer version available (current version: " + Version + ")");
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

		private void TryConnectIrc(string host, int port)
		{
			if (ConnectIrc(host, port)) return;
			Logger.Log("FATAL: IRC Connection failed. Application will now exit.", LogLevel.Error);
			Environment.Exit(1);
		}

		private void JoinInitialChannel()
		{
			var firstChannel = Settings.Instance["irc_initial_channel"];
			// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
			if (string.IsNullOrWhiteSpace(firstChannel)) {
				Logger.Log("Unable to join the initial channel: settings value for irc_initial_channel not set.", LogLevel.Error);
			} else {
				ircInterface.JoinChannel(firstChannel);
				Logger.Log("Ready to collect statistics in " + firstChannel, LogLevel.Info);
			}
		}

		public void Connect(string host, int port)
		{
			var dbConTask = Task.Run(() => ConnectDatabase());
			var ircConTask = Task.Run(() => TryConnectIrc(host, port));
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
			var state = client.GetClientState();
			var data = MiscTools.SerializeObject(state);
			if (string.IsNullOrWhiteSpace(data)) {
				Logger.Log("Unable to serialize client state: Client state not marked as serializable.", LogLevel.Error);
				return;
			}
			ircInterface.Disconnect("Updating...");
			Process.Start("mono", string.Format("BaggyBot20.exe -updated {0} {1}", data, channel));
			Environment.Exit(0);
		}

		public static void Main(string[] args)
		{
			using (var bot = new Bot()) {
				bot.Connect(Settings.Instance["irc_server"], int.Parse(Settings.Instance["irc_port"]));
				if (args.Length == 3 && args[0] == "-updated") {
					bot.Rejoin(args[1], args[2]);
				}
			}
		}
	}
}
