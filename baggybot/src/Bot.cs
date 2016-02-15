using BaggyBot.Configuration;
using BaggyBot.DataProcessors;
using BaggyBot.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using BaggyBot.CommandParsing;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Monitoring.Diagnostics;

namespace BaggyBot
{
	public sealed class Bot : IDisposable
	{
		// Manages multiple IRC Clients by handling events and forwarding messages
		private readonly IrcClientManager ircClientManager;
		//private IrcClient clientWrapper;
		// Provides an interface to the IRC client for sending data
		//private readonly IrcInterface ircInterface;
		// Handles IRC events, passing them on the the appropriate data processors if neccessary
		//private readonly IrcEventHandler ircEventHandler;
		// Collects performance statistics
		private readonly BotDiagnostics botDiagnostics;

		// Any message prefixed with this character will be interpreted as a command
		public const string CommandIdentifier = "-";
		// The versioning system used is Revision.Update.Bugfix, where 'revision' means a large revision of the application's inner workings,
		// often coupled with a change in the environment that the bot functions in. Example: changing the way the database is structured,
		// changing the platforms the bot can run on, etc.
		// Any change that exposes new features to the users of the bot (including the administrator) counts as an update.
		// Any update which doesn't add new features, and therefore only fixes issues with the bot or its dependencies is considered a bugfix.
		public const string Version = "4.3";
		// Version number of the database. This is checked against the 'version' key in the metadata table,
		// and a database upgrade is attempted if they do not match.
		public const string DatabaseVersion = "1.2.2";
		public const string ConfigVersion = "0.1";

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

			//previousVersion = ConfigManager.Config.Metadata.BotVersion;
			Console.Title = "BaggyBot Statistics Collector version " + Version;
			Logger.Log(this, "Starting BaggyBot version " + Version, LogLevel.Info);

			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning)
				{
					NotifyOperator("LOG WARNING/ERROR: " + message);
				}
			};

			// TODO: Check for version changes in a different way
			/*if (previousVersion != null && previousVersion != Version)
			{
				Logger.Log(this, "Updated from version {0} to version {1}", LogLevel.Info, true, previousVersion, Version);
			}
			ConfigManager.Config.Metadata.BotVersion = Version;
			Logger.Log("");*/
			
			ircClientManager = new IrcClientManager(new IrcEventHandler(new CommandHandler(this), new StatsHandler()));
			botDiagnostics = new BotDiagnostics(this);

			//ircEventHandler = new IrcEventHandler(dataFunctionSet, ircInterface, this);
		}

		public void NotifyOperator(string message)
		{
			//TODO: implement NotifyOperators
		}
		
		~Bot()
		{
			Debugger.Break();
		}

		public void OnPostConnect()
		{
			if (ConfigManager.Config.LogPerformance)
			{
				botDiagnostics.StartPerformanceLogging();
			}
		}

		public void Shutdown()
		{
			QuitRequested = true;
		}

		public void Dispose()
		{
			botDiagnostics.Dispose();
			ircClientManager.Dispose();
		}

		private void EnterMainLoop()
		{
			while (!QuitRequested)
			{
				Thread.Sleep(1000);
			}
			Logger.Log(this, "Preparing to shut down", LogLevel.Info);
			ircClientManager.Disconnect("Shutting down");
			ircClientManager.Dispose();
			Logger.Log(this, "Disposed SQL server connection object", LogLevel.Info);
			Logger.Dispose();
			Console.WriteLine("Goodbye.");
			Environment.Exit(0);
		}

		private void TryConnectIrc(ServerCfg server)
		{
			if (!ircClientManager.ConnectIrc(server))
			{
				Logger.Log(this, "FATAL: IRC Connection failed. Application will now exit.", LogLevel.Error);
				Shutdown();
			}
		}

		/*private void JoinInitialChannels(string[] channels)
		{
			foreach (var channel in channels)
			{
				// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
				if (string.IsNullOrWhiteSpace(channel))
				{
					Logger.Log(this, $"Unable to join {channel}: invalid channel name.", LogLevel.Error);
				}
				else
				{
					if (ircInterface.JoinChannel(channel))
					{
						Logger.Log(this, $"Ready to collect statistics in {channel}.", LogLevel.Info);
					}
					else
					{
						Logger.Log(this, $"Auto-joining {channel} failed.");
					}
				}
			}
		}*/

		public void Connect(ServerCfg[] servers)
		{
			foreach (var server in servers)
			{
				TryConnectIrc(server);
				ircClientManager[server.ServerName].JoinChannels(server.AutoJoinChannels);
			}

			OnPostConnect();

			if (previousVersion != null && previousVersion != Version)
			{
				NotifyOperator($"Succesfully updated from version {previousVersion} to version {Version}");
			}
			EnterMainLoop();
		}

		public static void ShutdownNow()
		{
			// If we're running on Windows, the application was most likely started by double-clicking the executable.
			// This creates a command prompt window that will immediately disappear when the bot exits.
			// For this reason, we should wait for user input so they can read the messages that have just been displayed.
			if (Environment.OSVersion.Platform.ToString().Contains("Win32"))
			{
				Console.WriteLine("Press any key to exit... ");
				Console.ReadKey();
			}
			Environment.Exit(1);
		}

		public static void Main(string[] args)
		{
			var parser = new CommandParser(new Operation()
				.AddKey("config", "baggybot-settings.yaml", 'c')
				.AddKey("colours", "Ansi", 'C'));

			var result = ConfigManager.Load("baggybot-settings.yaml");
			switch (result)
			{
				case ConfigManager.LoadResult.Success:
					break;
				case ConfigManager.LoadResult.NewFileCreated:
					Logger.Log(null, "A new configuration file has been created, please fill it with the correct settings. BaggyBot will now exit.", LogLevel.Info);
					ShutdownNow();
					break;
				case ConfigManager.LoadResult.Failure:
					Logger.Log(null, "Unable to load the configuration file. BaggyBot will now exit.");
					ShutdownNow();
					break;
			}
			Logger.UseColouredOutput = Colours.Windows;

			using (var bot = new Bot())
			{
				bot.Connect(ConfigManager.Config.Servers);
			}
		}
	}
}
