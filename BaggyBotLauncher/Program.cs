using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using IRCSharp;

namespace BaggyBotLauncher
{
	class BaggyBotHost
	{
		private IrcClient ircClient;
		private Process botClient;

		public BaggyBotHost()
		{
			botClient = new Process();
			botClient.StartInfo.FileName = "BaggyBot20.exe";

			ircClient = new IrcClient();

			ircClient.OnNickChanged += dataFunctionSet.HandleNickChange;
			ircClient.OnMessageReceived += ProcessMessage;
			ircClient.OnFormattedLineReceived += ProcessFormattedLine;
			ircClient.OnDisconnect += () =>
			{
				Logger.Log("Disconnected.");
			};
			ircClient.OnConnectionLost += () =>
			{
				Logger.Log("Connection lost. Attempting to reconnect...", LogLevel.Info);
				bool reconnected = false;
				do {
					System.Threading.Thread.Sleep(2000);
					try {
						ircClient.Reconnect();
						reconnected = true;
					} catch (System.Net.Sockets.SocketException) {
						Logger.Log("Failed to reconnect. Retrying in 2 seconds.", LogLevel.Info);
					}
				} while (!reconnected);
			};

		}

		internal void Connect()
		{
			Logger.Log("Connecting to the IRC server");

			Settings s = Settings.Instance;
			string server = s["irc_server"];
			int port = int.Parse(s["irc_port"]);
			string nick = s["irc_nick"];
			string ident = s["irc_ident"];

			string previousIdent = null;
			using (System.IO.StreamReader sr = new System.IO.StreamReader("identfile")) {
				previousIdent = sr.ReadLine();
			}
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter("identfile", false)) {
				sw.WriteLine(ident);
			}

			string realname = s["irc_realname"];
			string firschannel = s["irc_initial_channel"];

			try {
				ircClient.Connect(server, port, nick, ident, realname);
				Logger.Log("Connection established.");
				do {
					ircClient.JoinChannel(firschannel);
					System.Threading.Thread.Sleep(1000);
				} while (!ircClient.InChannel(firschannel));


			} catch (System.Net.Sockets.SocketException e) {
				Logger.Log("Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
			}

			using (System.IO.StreamWriter sw = new System.IO.StreamWriter("identfile", false)) {
				sw.WriteLine(previousIdent);
			}

			PostConnect();
		}

		static void Main(string[] args)
		{

		}

		private void ProcessFormattedLine(IrcLine line)
		{
			if (line.Command.Equals("NOTICE") && ircInterface.HasNickservCall && ircClient.GetUserFromSender(line.Sender).Ident.Equals("NickServ")) {
				if (line.FinalArgument.StartsWith("Information on")) {
					string data = line.FinalArgument.Substring("Information on  ".Length);
					string nick = data.Substring(0, data.IndexOf(" ") - 1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					string nickserv = data.Substring(0, data.Length - 3);
					ircInterface.AddNickserv(nick.ToLower(), nickserv);
				} else if (line.FinalArgument.EndsWith("is not registered.")) {
					string nick = line.FinalArgument.Substring(1, line.FinalArgument.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ') - 1);
					ircInterface.AddNickserv(nick, null);
				}
			} else if (line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick")) {
				ircInterface.DisableNickservCalls();
			} else if (line.Command.Equals("311") && ircInterface.HasWhoisCall) {
				IrcUser user = new IrcUser(line.Arguments[1], line.Arguments[2], line.Arguments[3]);
				ircInterface.AddUser(user.Nick, user);
			}
			if (!line.Command.Equals("PRIVMSG")) {
				ConsoleWriteLine("[RAW]\t" + line, ConsoleColor.DarkGray);
			}
		}
	}
}
