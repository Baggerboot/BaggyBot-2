using BaggyBot.Configuration;
using BaggyBot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BaggyBot.CommandParsing;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Monitoring.Diagnostics;
using BaggyBot.Plugins;

namespace BaggyBot
{
	public sealed partial class Bot : IDisposable
	{
		// Manages multiple chat clients by handling events and forwarding messages
		private readonly ChatClientManager chatClientManager;
		// Collects performance statistics
		private readonly BotDiagnostics botDiagnostics;

		// Any message prefixed with this character will be interpreted as a command
		public static string[] CommandIdentifiers = { "-", "/" };
		// Version number of the database. This is checked against the 'version' key in the metadata table,
		// and a database upgrade is attempted if they do not match.
		public const string DatabaseVersion = "2.0";
		public const string ConfigVersion = "0.1";
		public static string PreviousVersion { get; private set; }

		public static bool QuitRequested
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

			chatClientManager = new ChatClientManager();
			botDiagnostics = new BotDiagnostics(this);
		}

		public void NotifyOperator(string message)
		{
			chatClientManager.NotifyOperators(message);
		}

		public void OnPostConnect()
		{
			botDiagnostics.StartPerformanceLogging();
		}

		public static void Shutdown()
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

		public void Connect(IEnumerable<ServerCfg> servers)
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
			var intPlugins = Assembly.GetExecutingAssembly().GetTypes()
			                              .Where(type => typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract)
			                              .ToDictionary(type => type.GetCustomAttribute<ServerTypeAttribute>().ServerType);

			var extPlugins = new Dictionary<string, Type>();
			if (Directory.Exists("plugins"))
			{
				extPlugins = Directory.GetFiles("plugins", "*.dll", SearchOption.TopDirectoryOnly)
				                           .Select(file => Assembly.LoadFile(Path.GetFullPath(file)))
				                           .SelectMany(asm => asm.GetExportedTypes()
				                                                 .Where(type => typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract))
				                           .ToDictionary(type => type.GetCustomAttribute<ServerTypeAttribute>().ServerType);
			}
			Type pluginType;
			if (intPlugins.TryGetValue(server.ServerType, out pluginType) || extPlugins.TryGetValue(server.ServerType, out pluginType))
			{
				chatClientManager.ConnectUsingPlugin(pluginType, server);
			}
			else
			{
				Logger.Log(this, $"Unable to connect to {server.ServerName}: no plugin of type {server.ServerType} found.");
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

			//AppDomain.CurrentDomain.AppendPrivatePath(@"plugins");
			// TODO: verify that this is the correct way to append a directory to the private bin path
			AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = $"{AppDomain.CurrentDomain.SetupInformation.PrivateBinPath}{Path.PathSeparator}plugins";

			var parser = new CommandParser(new Operation()
				.AddKey("previous-version", 'p')
				.AddKey("config", "baggybot-settings.yaml", 'c')
				.AddKey("server", 's'));

			var opts = parser.Parse(args);
			PreviousVersion = opts.GetKey<string>("previous-version") ?? Version;
			var server = opts.GetKey<string>("server");

			var result = ConfigManager.Load(opts.GetKey<string>("config"));
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
				if (server == null)
				{
					bot.Connect(ConfigManager.Config.Servers);
				}
				else
				{
					bot.Connect(ConfigManager.Config.Servers.Where(s => s.ServerName == server));
				}
			}
		}
	}
}
