using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using CsNetLib2;

namespace BaggyBotHost
{
	/// <summary>
	/// This executable serves as a proxy between BaggyBot and an IRC server,
	/// allowing the BaggyBot executable to  while keeping the BaggyBot IRC user
	/// connected to the IRC server.
	/// </summary>
	class BaggyBotHost
	{
		private NetLibClient ircClient;
		private NetLibServer botServer;
		private NetLibServer commandServer;
		private Process botProcess;
		private bool connecting;
		private string mainChannel;
		private string serializedClientData;
		private long botClientId;

		public BaggyBotHost()
		{
			Logger.Log("Starting BaggyBot Host application");

			botServer = new NetLibServer(IPAddress.Any, 5000, TransferProtocolType.Delimited, Encoding.UTF8);
			botServer.OnDataAvailable += HandleBotData;
			botServer.OnClientConnected += HandleBotConnect;
			botServer.StartListening();
			commandServer = new NetLibServer(IPAddress.Any, 6668, TransferProtocolType.Delimited, Encoding.UTF8);
			commandServer.OnDataAvailable += HandleCommandData;
			commandServer.StartListening();
			StartBaggyBot();

			while (true) {
				Thread.Sleep(1000);
			}
		}

		private void CreateIrcClient()
		{
			ircClient = new NetLibClient(TransferProtocolType.Delimited, Encoding.UTF8);
			ircClient.OnDataAvailable += HandleClientData;
			ircClient.OnDisconnect += () =>
			{
				Logger.Log("IRC Connection lost!");
				botServer.CloseClientConnection(botClientId);
			};
		}

		private void ConnectIrcClient()
		{
			string hostname = Settings.Instance["irc_server"];
			int port;
			if (!int.TryParse(Settings.Instance["irc_port"], out port)) {
				Logger.Log("Unable to parse the settings value for irc_port. Please make sure that it is a valid integer.");
				botProcess.Close();
				Environment.Exit(1);
			}
			Logger.Log("Attaching host to {0}:{1}", hostname, port);
			connecting = true;
			ircClient.Connect(hostname, port);
			Logger.Log("Sending test data");
			ircClient.Send("NICK BaggyBotTest");
			Logger.Log("Sent test data");
			connecting = false;
		}

		private void HandleCommandData(string data, long client)
		{
			string[] args = data.Split(' ');
			switch (args[0].ToUpper()) {
				case "UPDATE":
					mainChannel = args[1];
					serializedClientData = args[2];
					break;
				default:
					Logger.Log("WARNING: Unknown command sent by BaggyBot: " + args[0]);
					break;
			}
		}
		private void HandleBotConnect(long clientId)
		{
			Logger.Log("Bot #{0} connected", clientId);
			botClientId = clientId;
			if (ircClient == null) {
				CreateIrcClient();
				ConnectIrcClient();
			}
		}
		private void HandleBotData(string data, long sender)
		{
			Console.WriteLine( sender + "DATA: " + data);
			while (connecting) {
				Thread.Sleep(20);
			}
			var prev = Console.ForegroundColor;
			/*Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(">> " + data);
			Console.ForegroundColor = prev;*/
			ircClient.Send(data);
		}
		private void HandleClientData(string data, long sender)
		{
			var prev = Console.ForegroundColor;
			/*Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("<< " + data);
			Console.ForegroundColor = prev;*/
			botServer.Send(data, botClientId);
		}

		private void StartBaggyBotProcessOsIndependent(string arguments)
		{
			if (Environment.OSVersion.Platform.ToString().ToLower() == "unix") {
				botProcess = Process.Start("mono", "BaggyBot20.exe " + arguments);
			} else {
				Logger.Log("Starting baggybot with arguments \"{0}\"", arguments);
				botProcess = Process.Start("BaggyBot20.exe", arguments);
				Process.Start("vsjitdebugger.exe", "-p " + botProcess.Id);
			}
		}

		/// <summary>
		/// Starts a new BaggyBot instance.
		/// </summary>
		/// <param name="PreviousExitCode">The exit code returned by the previous instance. 
		/// The default value of zero may be passed if this is the first instance to run.</param>
		private void StartBaggyBot(int PreviousExitCode = 0)
		{
			Logger.Log("Starting BaggyBot Child Process...");
			if (PreviousExitCode != 0) {
				if (mainChannel == null || mainChannel.Length == 0) {
					mainChannel = Settings.Instance["irc_initial_channel"];
				}
				string arguments = PreviousExitCode.ToString() + " " + mainChannel + " " + serializedClientData;
				StartBaggyBotProcessOsIndependent(arguments);
			} else {
				StartBaggyBotProcessOsIndependent(PreviousExitCode.ToString());
			}
			botProcess.EnableRaisingEvents = true;
			botProcess.Exited += HandleBotExit;
			mainChannel = null;
		}

		private void HandleBotExit(object sender, EventArgs e)
		{
			int exitCode = botProcess.ExitCode;

			switch (exitCode) {
				case 0:
					Logger.Log("Bot process has exited cleanly. Shutting down host..");
					Environment.Exit(0);
					break;
				case 100:
					UpdateBinaries();
					StartBaggyBot(exitCode);
					break;
				default:
					StartBaggyBot(exitCode);
					break;
			}
		}

		private void UpdateBinaries()
		{
			Console.WriteLine("Deleting old assemblies");
			File.Delete("BaggyBot20.exe");
			File.Delete("CsNetLib2.dll");
			Console.WriteLine("Moving new assemblies");
			File.Move("BaggyBot20.exe.new", "BaggyBot20.exe");
			File.Move("CsNetLib2.dll.new", "CsNetLib2.dll");
		}

		private static string EscapeCode(int color)
		{
			return "\x1b[38;5;" + color + "m";
		}

		static void Main(string[] args)
		{
			/*if (args.Length == 1 && args[0] == "--colortest") {

				Logger.Log("Beginning color test");

				int i = 0;

				Logger.Log("System colors:");
				for (; i < 16; i++) {
					Console.Write(EscapeCode(i) + "█");
					if (i == 7) {
					}
				}
				Logger.Log("Color cube, 6x6x6:");
				for (int green = 0; green < 6; green++) {
					for (int red = 0; red < 6; red++) {
						for (int blue = 0; blue < 6; blue++) {
							int color = 16 + (red * 36) + (green * 6) + blue;
							Console.Write("{0}█{1:000}█", EscapeCode(color), color);
						}
						Console.Write("\x1b[0m ");
					}
					Console.Write("\n");
				}

				Logger.Log("Grayscale ramp:");
				for (i = 232; i < 256; i++) {
					Console.Write(EscapeCode(i) + "█");
				}
				Logger.Log("Done.");

			} else {*/

			new BaggyBotHost();

		}
	}
}
