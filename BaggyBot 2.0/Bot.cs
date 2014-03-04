﻿using System;
using System.Collections.Generic;
using System.Linq;

using IRCSharp;
using BaggyBot.Database;
using BaggyBot.Tools;
using BaggyBot.DataProcessors;
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
		// Writes the local port and requested ident to an ident file
		private IdentWriter identWriter;

		// Any message prefixed with this character will be interpreted as a command
		public const string commandIdentifier = "-";
		// The versioning system used is Revision.Update.Bugfix, where 'revision' means a large revision of the application's inner workings,
		// often coupled with a change in the environment that the bot functions in. Example: changing the way the database is structured,
		// changing the platforms the bot can run on, etc.
		// Any change that exposes new features to the users of the bot (including the administrator) counts as an update.
		// Any change that's made only to fix bugs within bot's system without adding new features is seen as a bugfix.
		public const string Version = "3.26.15";

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
			previousVersion = Settings.Instance["version"];
			Console.Title = "BaggyBot Statistics Collector version " + Version;
			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);
			if (previousVersion != Version) {
				Logger.Log("Updated from version {0} to version {1}", LogLevel.Info, true, previousVersion, Version);
			}
			Settings.Instance["version"] = Version;

			// Create various data processors and I/O handlers
			sqlConnector = new SqlConnector();
			client = new IrcClient();
			ircInterface = new IrcInterface(client);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			ircInterface.dataFunctionSet = dataFunctionSet;
			statsHandler = new StatsHandler(dataFunctionSet, ircInterface);
			Tools.UserTools.DataFunctionSet = dataFunctionSet;
			botDiagnostics = new BotDiagnostics(ircInterface);
			commandHandler = new CommandHandler(ircInterface, dataFunctionSet, this, botDiagnostics);
			ircEventHandler = new IrcEventHandler(dataFunctionSet, ircInterface, commandHandler, statsHandler);
			identWriter = new IdentWriter();

			HookupIrcEvents();
		}


		private void HookupIrcEvents()
		{
			client.OnLocalPortKnown += port => identWriter.Write(port, Settings.Instance["irc_ident"]);
			client.OnNickChanged += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ircEventHandler.ProcessMessage;
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			//client.OnRawLineReceived += ircEventHandler.ProcessRawLine;
			client.OnNoticeReceived += ircEventHandler.ProcessNotice;
			client.OnDisconnect += HandleDisconnect;
			client.OnKicked += channel => Logger.Log("I was kicked from " + channel, LogLevel.Warning);
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
			sqlConnector.OpenConnection();
			Logger.Log("Database connection established", LogLevel.Info);
		}

		/// <summary>
		/// Connects the bot to the IRC server
		/// </summary>
		private void ConnectIrc()
		{
			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];
			string realname = s["irc_realname"];
			try {
				client.Connect(server, port, nick, ident, realname);
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return;
			}
		}

		public void Reconnect(string[] channels)
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
			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return;
				
			}
		}

		public void JoinChannels(params string[] channels)
		{
			// TODO: Parallelize channel joins
			foreach (var c in channels) {
				client.JoinChannel(c);
			}
			
		}

		public void OnPostConnect()
		{
			botDiagnostics.StartPerformanceLogging();

			if (previousVersion != null && previousVersion != Version) {
				ircInterface.NotifyOperator("Succesfully updated from version " + previousVersion + " to version " + Version);
			}
			// Hook up IRC Log warings, which notify the bot operator of warnings and errors being logged to the log file.
			Logger.OnLogEvent += (message, level) =>
			{
				if (level == LogLevel.Error || level == LogLevel.Warning) {
					ircInterface.NotifyOperator("LOG WARNING: " + message);
				}
			};
		}

		private void Reconnect(DisconnectReason reason)
		{
			var channels = client.GetChannels();
			var connectionInfo = client.GetConnectionInfo();

			bool reconnected = false;
			do {
				try {

					client = new IrcClient();
					HookupIrcEvents();
					Reconnect(channels.Select((chan) => chan.Name).ToArray());

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
		}

		private void HandleDisconnect(DisconnectReason reason)
		{
			if (reason == DisconnectReason.DisconnectOnRequest) {
				Logger.Log("Disconnected.", LogLevel.Debug);
			} else {
				Logger.Log("Connection lost ({0}) Attempting to reconnect...", LogLevel.Warning, true, reason.ToString());
				Reconnect(reason);
			}
		}

		/// <summary>
		/// This kills the bot
		/// </summary>
		public void Shutdown()
		{
			// HACK: Why a thread? There was a reason for this but I forgot about it. Maybe figure it out or something.
			var t = new System.Threading.Thread(() =>
			{
				Logger.Log("Preparing to shut down", LogLevel.Info);
				ircInterface.Disconnect("Shutting down");
				Logger.Log("Disconnected from IRC server", LogLevel.Info);
				sqlConnector.CloseConnection();
				Logger.Log("Closed SQL server connection", LogLevel.Info);
				sqlConnector.Dispose();
				Logger.Log("Disposed SQL server connection object", LogLevel.Info);
				Logger.Dispose();
				Console.ReadKey();
			});
			t.Start();
		}

		public void Dispose()
		{
			botDiagnostics.Dispose();
			sqlConnector.Dispose();
		}

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += (sender, eventargs) => {
				Console.WriteLine("Exiting");
			};
			Logger.ClearLog();

			Bot p = new Bot();
			p.Connect();
		}

		public void Connect()
		{
			Task dbConTask = Task.Run(() => ConnectDatabase());
			Task ircConTask = Task.Run(() => ConnectIrc());
			Task.WaitAll(dbConTask, ircConTask);

			string firstChannel = Settings.Instance["irc_initial_channel"];
			// NOTE: Join might fail if the server does not accept JOIN commands before it has sent the entire MOTD to the client
			JoinChannels(firstChannel);
			Logger.Log("Ready to collect statistics in " + firstChannel, LogLevel.Info);
			OnPostConnect();
		}

		void IDisposable.Dispose()
		{
			throw new NotImplementedException();
		}
	}
}