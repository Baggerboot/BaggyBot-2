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
using IRCSharp.IrcCommandProcessors.Quirks;

namespace BaggyBot
{
	public sealed class Bot : IDisposable
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
		public const string Version = "4.1.3";
		// Version number of the database. This is checked against the 'version' key in the metadata table. If they do not match,
		// the DB connection is closed, and the user will be required to update the DB by hand, as automatic updates are not yet supported.
		public const string DatabaseVersion = "1.2";

		public bool QuitRequested
		{
			get;
			private set;
		}

		public static DateTime LastUpdate => MiscTools.RetrieveLinkerTimestamp();

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
			Logger.Log(this, "Starting BaggyBot version " + Version, LogLevel.Info);
			if (previousVersion != null && previousVersion != Version)
			{
				Logger.Log(this, "Updated from version {0} to version {1}", LogLevel.Info, true, previousVersion, Version);
			}
			Settings.Instance["version"] = Version;

			// Create various data processors and I/O handlers
			sqlConnector = new SqlConnector();
			client = new IrcClient();
			ircInterface = new IrcInterface(client);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			ircInterface.DataFunctionSet = dataFunctionSet;
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
			if (s.NewFileCreated)
			{
				s.FillDefault();
				Logger.Log(this, "New settings file created ({0}). Please fill it with the correct data, then start the bot again. BaggyBot will now exit.", LogLevel.Info, true, Logger.LogFileName);
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
			client.OnMessageReceived += message => Task.Run(() => ircEventHandler.ProcessMessage(message));
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			//client.OnRawLineReceived += ircEventHandler.ProcessRawLine;
			client.OnNoticeReceived += ircEventHandler.ProcessNotice;
			client.OnDisconnect += (reason, exception) => HandleDisconnect(client, reason, exception);
			client.OnKicked += (channel, reason, kicker) => Logger.Log(this, "I was kicked from {0} by {1} ({2})", LogLevel.Warning, true, channel, kicker.Nick, reason);
			client.OnDebugLog += (sender, message) => Logger.Log(sender, "[IC#]" + message, LogLevel.Info);
			client.OnNetLibDebugLog += (sender, message) => Logger.Log(sender, "[NL#]" + message, LogLevel.Info);
			client.OnJoinChannel += ircEventHandler.HandleJoin;
			client.OnPartChannel += ircEventHandler.HandlePart;
			client.OnKick += ircEventHandler.HandleKick;
			client.OnQuit += ircEventHandler.HandleQuit;
		}

		private void ConnectDatabase()
		{
			Logger.Log(this, "Connecting to the database", LogLevel.Info);
			try
			{
				if (sqlConnector.OpenConnection())
					Logger.Log(this, "Database connection established", LogLevel.Info);
				else
					Logger.Log(this, "Database connection not established. Bot functionality will be very limited.",
						LogLevel.Warning);
			}
			catch (Exception e)
			{
				Logger.Log(this, "An exception occurred while trying to connect to the database: {0}:{1}", LogLevel.Error, true, e.GetType().Name, e.Message);
			}
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
			var password = s["irc_password"];
			var tls = s.ReadBool("irc_use_tls", false);
			var verify = s.ReadBool("irc_verify_server_certificate", true);
			var slackCompatMode = s.ReadBool("irc_use_slack_compat_mode", false);

			// Slack IRC messes up domain names by prepending http://<domain> to everything that looks like a domain name or incorrectly formatted URL,
			// despite the fact that such formatting should really be done by IRC clients, not servers.
			// BaggyBot doesn't like this; it messes up several of his commands, such as -resolve and -ping.
			// Therefore, when Slack Compatibility Mode is enabled, we add a SlackIrcProcessor which strips these misformatted domain names from the message.
			if (slackCompatMode)
			{
				client.DataProcessors.Add(new SlackIrcProcessor());
			}

			Logger.Log(this, "Connecting to the IRC server..");
			Logger.Log(this, "\tNick\t" + nick);
			Logger.Log(this, "\tIdent\t" + ident);
			Logger.Log(this, "\tName\t" + realname);
			Logger.Log(this, "\tHost\t" + host);
			Logger.Log(this, "\tPort\t" + port);
			try
			{
				client.Connect(new ConnectionInfo
				{
					Host = host,
					Port = port,
					Nick = nick,
					Ident = ident,
					Password = password,
					RealName = realname,
					useTLS = tls,
					verifyServerCertificate = verify
				});
				return true;
			}
			catch (SocketException e)
			{
				Logger.Log(this, "Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			}
			catch (ArgumentException e)
			{
				Logger.Log(this, "Failed to connect to the IRC server: The settings file does not contain a value for \"{0}\"", LogLevel.Error, true, e.ParamName);
				return false;
			}
			catch (Exception e)
			{
				Logger.Log(this, "Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			}
		}

		public void OnPostConnect()
		{
			botDiagnostics.StartPerformanceLogging();
			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning)
				{
					if (ircInterface.Connected)
					{
						ircInterface.NotifyOperator("LOG WARNING/ERROR: " + message);
					}
				}
			};
		}

		private void HandleDisconnect(object source, DisconnectReason reason, Exception ex)
		{
			if (reason == DisconnectReason.DisconnectOnRequest)
			{
				Logger.Log(this, "Disconnected from IRC server.", LogLevel.Info);
				QuitRequested = true;
			}
			else
			{
				if (reason == DisconnectReason.Other)
				{
					Logger.Log(source, "Connection lost ({0}: {1}) Attempting to reconnect...", LogLevel.Error, true, ex.GetType().Name, ex.Message);
				}
				else
				{
					Logger.Log(source, "Connection lost ({0}) Attempting to reconnect...", LogLevel.Warning, true, reason.ToString());
				}
				var state = client.GetClientState();

				bool success;

				do
				{
					client = new IrcClient(/*Settings.Instance["deployed"] == "false"*/);
					ircInterface.ChangeClient(client);
					HookupIrcEvents();

					success = ConnectIrc(state.RemoteHost, state.RemotePort);
					if (success) continue;
					Logger.Log(this, "Reconnection attempt failed. Retrying in 5 seconds.", LogLevel.Warning, true, reason.ToString());
					Thread.Sleep(5000);
				} while (!success);

				Logger.Log(this, "Successfully reconnected to the server!", LogLevel.Warning, true, reason.ToString());

				foreach (var channel in state.Channels.Where(channel => !client.JoinChannel(channel.Key)))
				{
					Logger.Log(this, "Failed to rejoin {0}! Skipping this channel.", LogLevel.Error, true, channel.Key);
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
				Logger.Log(this, "Unable to read the channel list. I will not be able to rejoin my previous channels.", LogLevel.Error);
			}
			else
			{
				foreach (var channel in state.Channels)
				{
					if (!client.JoinChannel(channel.Key))
					{
						Logger.Log(this, "Failed to rejoin {0}! Skipping this channel.", LogLevel.Error, true, channel.Key);
					}
				}
			}
			if (previousVersion != null && previousVersion != Version)
			{
				ircInterface.SendMessage(mainChannel, "Succesfully updated from version " + previousVersion + " to version " + Version);
			}
			else
			{
				ircInterface.SendMessage(mainChannel, "Failed to update: no newer version available (current version: " + Version + ")");
			}
			EnterMainLoop();
		}

		private void EnterMainLoop()
		{
			while (!QuitRequested)
			{
				Thread.Sleep(1000);
			}
			Logger.Log(this, "Preparing to shut down", LogLevel.Info);
			ircInterface.Disconnect("Shutting down");
			sqlConnector.CloseConnection();
			Logger.Log(this, "Closed SQL server connection", LogLevel.Info);
			sqlConnector.Dispose();
			Logger.Log(this, "Disposed SQL server connection object", LogLevel.Info);
			Logger.Dispose();
			Console.WriteLine("Goodbye.");
			Environment.Exit(0);
		}

		private void TryConnectIrc(string host, int port)
		{
			if (ConnectIrc(host, port)) return;
			Logger.Log(this, "FATAL: IRC Connection failed. Application will now exit.", LogLevel.Error);
			Environment.Exit(1);
		}

		private void JoinInitialChannel()
		{
			var firstChannel = Settings.Instance["irc_initial_channel"];
			// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
			if (string.IsNullOrWhiteSpace(firstChannel))
			{
				Logger.Log(this, "Unable to join the initial channel: settings value for irc_initial_channel not set.", LogLevel.Error);
			}
			else
			{
				ircInterface.JoinChannel(firstChannel);
				Logger.Log(this, "Ready to collect statistics in " + firstChannel, LogLevel.Info);
			}
		}

		public void Connect(string host, int port)
		{
			var dbConTask = Task.Run(() => ConnectDatabase());
			var ircConTask = Task.Run(() => TryConnectIrc(host, port));
			Task.WaitAll(dbConTask, ircConTask);

			JoinInitialChannel();
			OnPostConnect();

			if (previousVersion != null && previousVersion != Version)
			{
				ircInterface.NotifyOperator("Succesfully updated from version " + previousVersion + " to version " + Version);
			}
			EnterMainLoop();
		}


		internal void RequestUpdate(string channel, bool updateFiles)
		{
			var state = client.GetClientState();
			var data = MiscTools.SerializeObject(state);
			if (string.IsNullOrWhiteSpace(data))
			{
				Logger.Log(this, "Unable to serialize client state: Client state not marked as serializable.", LogLevel.Error);
				return;
			}
			ircInterface.Disconnect("Updating...");
			Process.Start("mono", string.Format("BaggyBot20.exe -updated {0} {1}", data, channel));
			Environment.Exit(0);
		}

		public static void Main(string[] args)
		{
			var settingsFile = "baggybot.settings";
			var colours = true;

			for (int i = 0; i < args.Length; i++)
			{
				try
				{
					switch (args[i])
					{
						case "--config":
							settingsFile = args[++i];
							break;
						case "--colours":
							colours = bool.Parse(args[++i]);
							break;
					}
				}
				catch (IndexOutOfRangeException)
				{
					Console.WriteLine("ERROR: expected a value after {0}", args[i - 1]);
				}
			}
			Settings.Instance.LoadSettingsFile(settingsFile);
			Logger.UseColouredOutput = colours;

			using (var bot = new Bot())
			{
				bot.Connect(Settings.Instance["irc_server"], int.Parse(Settings.Instance["irc_port"]));
			}
		}
	}
}
