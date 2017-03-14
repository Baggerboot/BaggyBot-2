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
		private readonly ChatClientManager chatClientManager = new ChatClientManager();
		// Collects performance statistics
		private readonly BotDiagnostics botDiagnostics;
		// The main thread will wait for this semaphore to be signalled before stopping.
		private static readonly SemaphoreSlim shutdownSemaphore = new SemaphoreSlim(0);

		// Any message prefixed with this character will be interpreted as a command
		public static IReadOnlyList<string> CommandIdentifiers = new[] { "-", "/" };
		// Version number of the database. This is checked against the 'version' key in the metadata table,
		// and a database upgrade is attempted if they do not match.
		public const string DatabaseVersion = "2.1";
		public const string ConfigVersion = "0.1";

		// If the bot is started in update mode, a previous version has to be specified.
		// The bot will then announce whether the update was a success or a failure.
		// To determine this, the previous version is stored in here.
		// If the bot is not started in update mode, the value of this field remains null.
		public static string PreviousVersion { get; private set; }

		public static DateTime LastUpdate => MiscTools.RetrieveLinkerTimestamp();

		public Bot()
		{
			Logger.ClearLog();

			Console.Title = "BaggyBot Statistics Collector version " + Version;
			Logger.Log(this, "Starting BaggyBot version " + Version, LogLevel.Info);

			botDiagnostics = new BotDiagnostics(NotifyOperators);

			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning)
				{
					NotifyOperators("LOG WRN/ERR: " + message);
				}
			};

		}

		/// <summary>
		/// Sends a notification to all operators of the bot.
		/// </summary>
		private void NotifyOperators(string message)
		{
			chatClientManager.NotifyOperators(message);
		}

		/// <summary>
		/// Attempts to connect to the given servers.
		/// </summary>
		public void Connect(IEnumerable<ServerCfg> servers)
		{
			foreach (var server in servers)
			{
				Connect(server);
			}
			PostConnect();
		}

		/// <summary>
		/// Tries to connect to a server by looking up the correct plugin for it,
		/// and connecting that plugin to its server.
		/// </summary>
		private void Connect(ServerCfg server)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var intPlugins = assembly.GetTypes()
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
			var serverType = server.ServerType;
			Type pluginType;
			if (intPlugins.TryGetValue(serverType, out pluginType) || extPlugins.TryGetValue(serverType, out pluginType))
			{
				chatClientManager.ConnectUsingPlugin(pluginType, server);
			}
			else
			{
				Logger.Log(this, $"Unable to connect to {server.ServerName}: no plugin of type {serverType} found.");
			}
		}

		private void PostConnect()
		{
			if (PreviousVersion != null && PreviousVersion != Version)
			{
				NotifyOperators($"Succesfully updated from version {PreviousVersion} to version {Version}");
			}
			botDiagnostics.StartPerformanceLogging();
		}

		public void AwaitShutdown()
		{
			// Wait for the semaphore to be signalled
			shutdownSemaphore.Wait();
			Logger.Log(this, "Preparing to shut down", LogLevel.Info);
			chatClientManager.Disconnect("Shutting down");
			Dispose();
			Logger.Dispose();
			Console.WriteLine("Goodbye.");
			//Environment.Exit(0);
		}

				public void Dispose()
		{
			botDiagnostics.Dispose();
			chatClientManager.Dispose();
		}

		/// <summary>
		/// Signals the bot to shut down.
		/// </summary>
		internal static void Shutdown()
		{
			shutdownSemaphore.Release();
		}

		public static void ShutdownAfterMessage()
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

			// TODO: verify that this is the correct way to append a directory to the private bin path
			var setupInformation = AppDomain.CurrentDomain.SetupInformation;
			setupInformation.PrivateBinPath = $"{setupInformation.PrivateBinPath}{Path.PathSeparator}plugins";

			var parser = new CommandParser(new Operation()
				.AddKey("previous-version", 'p')
				.AddKey("config", "baggybot-settings.yaml", 'c')
				.AddKey("server", 's'));

			var opts = parser.Parse(args);
			PreviousVersion = opts.Keys["previous-version"] ?? Version;
			var server = opts.Keys["server"];

			var result = ConfigManager.Load(opts.Keys["config"]);
			switch (result)
			{
				case ConfigManager.LoadResult.Success:
					break;
				case ConfigManager.LoadResult.NewFileCreated:
					Logger.Log(null, "A new configuration file has been created, please fill it with the correct settings. BaggyBot will now exit.", LogLevel.Info);
					ShutdownAfterMessage();
					break;
				case ConfigManager.LoadResult.Failure:
					Logger.Log(null, "Unable to load the configuration file. BaggyBot will now exit.", LogLevel.Error);
					ShutdownAfterMessage();
					break;
			}
			// On Linux, with older Mono versions, coloured output sometimes doesn't seem to work.
			// Setting this to Colours.Ansi will fix that issue.
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
				bot.AwaitShutdown();
			}
		}
	}
}
