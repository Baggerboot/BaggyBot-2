using BaggyBot.Configuration;
using BaggyBot.DataProcessors;
using BaggyBot.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Hosting;
using System.Threading;
using BaggyBot.InternalPlugins.Curse;
using BaggyBot.InternalPlugins.Discord;
using BaggyBot.InternalPlugins.Irc;
using BaggyBot.InternalPlugins.Slack;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Monitoring.Diagnostics;
using BaggyBot.Plugins;
using Microsoft.Scripting.Utils;

namespace BaggyBot
{
	public sealed partial class Bot : IDisposable
	{
		// Manages multiple chat clients by handling events and forwarding messages
		private readonly ChatClientManager chatClientManager;
		// Collects performance statistics
		private readonly BotDiagnostics botDiagnostics;

		// Any message prefixed with this character will be interpreted as a command
		public static string[] CommandIdentifiers = {"-", "/"};
		// Version number of the database. This is checked against the 'version' key in the metadata table,
		// and a database upgrade is attempted if they do not match.
		public const string DatabaseVersion = "2.0";
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
			
			chatClientManager = new ChatClientManager(new ChatClientEventHandler(new CommandHandler(this), new StatsHandler()));
			botDiagnostics = new BotDiagnostics(this);
		}

		public void NotifyOperator(string message)
		{
			chatClientManager.NotifyOperators(message);
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
			chatClientManager.Dispose();
		}

		private void EnterMainLoop()
		{
			while (!QuitRequested)
			{
				Thread.Sleep(1000);
			}
			Logger.Log(this, "Preparing to shut down", LogLevel.Info);
			chatClientManager.Disconnect("Shutting down");
			chatClientManager.Dispose();
			Logger.Log(this, "Disposed SQL server connection object", LogLevel.Info);
			Logger.Dispose();
			Console.WriteLine("Goodbye.");
			Environment.Exit(0);
		}

		public void Connect(ServerCfg[] servers)
		{
			foreach (var server in servers)
			{
				TryConnect(server);
			}

			OnPostConnect();

			if (previousVersion != null && previousVersion != Version)
			{
				NotifyOperator($"Succesfully updated from version {previousVersion} to version {Version}");
			}
			EnterMainLoop();
		}

		private void TryConnect(ServerCfg server)
		{
			if (server.ServerType == "irc")
			{
				if (!chatClientManager.ConnectUsingPlugin(server, new IrcPlugin(server)))
				{
					Logger.Log(this, "FATAL: IRC connection failed. Application will now exit.", LogLevel.Error);
					Shutdown();
				}
			}
			else if (server.ServerType == "slack")
			{
				if (!chatClientManager.ConnectUsingPlugin(server, new SlackPlugin(server)))
				{
					Logger.Log(this, "FATAL: Slack connection failed. Application will now exit.", LogLevel.Error);
					Shutdown();
				}
			}
			else if (server.ServerType == "discord")
			{
				if (!chatClientManager.ConnectUsingPlugin(server, new DiscordPlugin(server)))
				{
					Logger.Log(this, "FATAL: Discord connection failed. Application will now exit.", LogLevel.Error);
					Shutdown();
				}
			}
			else if (server.ServerType == "curse")
			{
				if (!chatClientManager.ConnectUsingPlugin(server, new CursePlugin(server)))
				{
					Logger.Log(this, "FATAL: Curse connection failed. Application will now exit.", LogLevel.Error);
					Shutdown();
				}
			}
			else
			{
				Logger.Log(this, $"Attempting to load a plugin for server type '{server.ServerType}'");
				Directory.CreateDirectory("plugins");

				bool success = false;
				foreach (var dll in Directory.GetFiles("plugins", "*.dll", SearchOption.TopDirectoryOnly))
				{
					var loadedDll = Assembly.LoadFile(Path.GetFullPath(dll));
					foreach (var type in loadedDll.GetExportedTypes().Where(type => typeof(Plugin).IsAssignableFrom(type)))
					{
						// Matching type found, create an instance of it
						var instance = (Plugin)Activator.CreateInstance(type, server);

						chatClientManager.ConnectUsingPlugin(server, (Plugin)instance);
						success = true;
						break;
					}
					if (success) break;
				}
				if (!success)
				{
					Logger.Log(this, $"Unable to find a plugin for server type '{server.ServerType}'. The server connection {server.ServerName} will be skipped.", LogLevel.Error);
					return;
				}

			}

			foreach (var channel in server.AutoJoinChannels)
			{
				chatClientManager[server.ServerName].JoinChannel(new ChatChannel(channel));
			}
		}

		public static void ShutdownNow()
		{
			// If we're running on Windows, the application was most likely started by double-clicking the executable.
			// This creates a command prompt window that will immediately disappear when the bot exits.
			// For this reason, we should wait for user input so they can read the messages that have just been displayed.
			if (Environment.OSVersion.VersionString.Contains("Microsoft Windows"))
			{
				Console.WriteLine("Press any key to exit... ");
				Console.ReadKey();
			}
			Environment.Exit(1);
		}

		public static void Main(string[] args)
		{
			if (Environment.OSVersion.VersionString.Contains("Microsoft Windows"))
			{
				// If we're running on Windows, we're likely running in a tiny console window.
				// Resize it to prevent most lines from wrapping around.
				Console.BufferWidth = 160;
				Console.WindowWidth = 160;
				Console.BufferHeight = 200;
			}


			AppDomain.CurrentDomain.AppendPrivatePath(@"plugins");

			// TODO: parse program arguments
			/*var parser = new CommandParser(new Operation()
				.AddKey("config", "baggybot-settings.yaml", 'c')
				.AddKey("colours", "Ansi", 'C'));*/

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
