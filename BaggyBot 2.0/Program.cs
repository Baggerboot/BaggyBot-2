using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using IRCSharp;

namespace BaggyBot
{
	class Program
	{
		private IrcClient client;
		private StatsHandler sHandler;
		private SqlConnector sqlConnector;
		private DataFunctionSet dataFunctionSet;
		private CommandHandler commandHandler;
		private IrcInterface ircInterface;
		internal const string commandIdentifier = "-";

		public static bool noColor;
		internal const string Version = "2.0.28";

		private string previousVersion = null;

		public Program(string previousVersion = null)
		{
			this.previousVersion = previousVersion;

			Logger.Log("Starting BaggyBot version " + Version, LogLevel.Info);

			sqlConnector = new SqlConnector();
			client = new IrcClient();
			ircInterface = new IrcInterface(client, dataFunctionSet);
			dataFunctionSet = new DataFunctionSet(sqlConnector, ircInterface);
			sHandler = new StatsHandler(dataFunctionSet, ircInterface);
			commandHandler = new CommandHandler(ircInterface, sqlConnector,dataFunctionSet);
			BaggyBot.Tools.UserTools.DataFunctionSet = dataFunctionSet;

			client.OnNickChanged += dataFunctionSet.HandleNickChange;
			client.OnMessageReceived += ProcessMessage;
			client.OnFormattedLineReceived += ProcessFormattedLine;
			client.OnConnectionLost += () => {
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
			}; 

			sqlConnector.OpenConnection();
			sqlConnector.InitializeDatabase();
		}

		private void ConnectFromSocket()
		{
			// Read the serialized socket from standard input
			Console.OpenStandardInput();
			BinaryFormatter bf = new BinaryFormatter();
			Object input = bf.Deserialize(Console.OpenStandardInput());
			
			//SocketInformation si = (SocketInformation)input;
			Socket s = (Socket)input;

			client.ConnectFromSocket(s);

			Settings set = Settings.Instance;

			client.SendMessage(set["irc_initial_channel"], "Succesfully updated to version " + Version);
		}

		internal void Connect()
		{
			Logger.Log("Connecting to the IRC server");

			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];
			string realname = s["irc_realname"];
			string firschannel = s["irc_initial_channel"];

			try {
				client.Connect(server, port, nick, ident, realname);
				ircInterface.TestNickServ();
				System.Threading.Thread.Sleep(2000);
				Logger.Log("Connection established.");
				do {
					client.JoinChannel(firschannel);
					System.Threading.Thread.Sleep(1000);
				} while (!client.InChannel(firschannel));


			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}
			if (previousVersion != null && previousVersion != Version) {
				client.SendMessage(firschannel, "Succesfully updated from version " + previousVersion + " to version " + Version);
			} else if (previousVersion != null) {
				client.SendMessage(firschannel, "Failed to update: No newer version available. Previous version: " + previousVersion);
			}
		}

		private void ProcessFormattedLine(IrcLine line)
		{
			if (line.Command.Equals("NOTICE") && ircInterface.HasNickservCall && client.GetUserFromSender(line.Sender).Ident.Equals("NickServ")) {
				if (line.FinalArgument.StartsWith("Information on")) {
					string data = line.FinalArgument.Substring("Information on  ".Length);
					string nick = data.Substring(0, data.IndexOf(" ")-1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					string nickserv = data.Substring(0,data.Length-3);
					ircInterface.AddNickserv(nick.ToLower(),nickserv);
				} else if (line.FinalArgument.EndsWith("is not registered.")) {
					string nick = line.FinalArgument.Substring(1, line.FinalArgument.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ')-1);
					ircInterface.AddNickserv(nick, null);
				}
			}else if(line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick")){
				ircInterface.DisableNickservCalls();
			}else if(line.Command.Equals("311") && ircInterface.HasWhoisCall)
			{
				IrcUser user = new IrcUser(line.Arguments[1], line.Arguments[2], line.Arguments[3]);
				ircInterface.AddUser(user.Nick, user);
			}
			if (!line.Command.Equals("PRIVMSG")) {
				ConsoleWriteLine("[RAW]\t" + line, ConsoleColor.DarkGray);
			}
		}

		private static void ConsoleWriteLine(string line, ConsoleColor color = ConsoleColor.Gray)
		{
			if (!noColor) {
				Console.ForegroundColor = color;
			}

			Console.WriteLine(line);
		}

		private void ProcessRawLine(string line)
		{
			ConsoleWriteLine("[RAW]\t" + line);
		}

		

		private void ProcessMessage(IrcMessage message)
		{
			ConsoleWriteLine("[MSG]\t" + message.Sender.Nick+ ": " + message.Message, ConsoleColor.DarkGreen);
			if (message.Message.StartsWith(commandIdentifier)) {
				commandHandler.ProcessCommand(message);
			} else {
				sHandler.ProcessMessage(message);
			}
		}

		static void Main(string[] args)
		{
			string previousVersion = null;
			bool deserialize = false;

			for (int i = 0; i < args.Length; i++) {
				switch (args[i]) {
					case "-nc":
						noColor = true;
						break;
					case "-pv":
						previousVersion = args[i + 1];
						i++;
						break;
					case "-ds":
						deserialize = true;
						break;
				}
			}
			Logger.ClearLog();
			if (deserialize) {
				new Program(previousVersion).ConnectFromSocket();
			} else {
				new Program(previousVersion).Connect();
			}
		}

		
	}
}
