//#define	LINUX
#define	WINDOWS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaggyBot.Commands
{
	class Update : ICommand
	{
		private IrcInterface ircInterface;
		private SqlConnector sqlConnector;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Update(IrcInterface inter, SqlConnector sc)
		{
			ircInterface = inter;
			sqlConnector = sc;
		}

		public void Use(CommandArgs command)
		{
			string location = "http://home2.jgeluk.net/files/baggybot20/";
			bool meta = false;
			bool download = false;
			bool botOnly = false;
			for (int i = 0; i < command.Args.Length; i++) {
				switch (command.Args[i]) {
					case "-l":
						location = command.Args[i + 1];
						i++;
						break;
					case "-h":
						location = "http://home1.jgeluk.net/files/baggybot20/";
						break;
					case "-m":
						meta = true;
						break;
					case "-d":
						download = true;
						break;
					case "-b":
						botOnly = true;
						break;
				}
			}

			if (download) {

				Dictionary<Uri, string> files = new Dictionary<Uri, string>();
				if (meta) {
					File.Delete("UpdateManager.exe");
					files.Add(new Uri(location + "UpdateManager.exe"), "UpdateManager.exe");
				}else{
					files.Add(new Uri(location + "BaggyBot20.exe"), "BaggyBot20_new.exe");
					if (!botOnly) {
						files.Add(new Uri(location + "CSNetLib.dll"), "CSNetLib_new.dll");
						files.Add(new Uri(location + "IRCSharp.dll"), "IRCSharp_new.dll");
					}
				}

				Logger.Log("Downloading files...");
				using (WebClient client = new WebClient()) {
					try {
						foreach (var pair in files) {
							client.DownloadFile(pair.Key, pair.Value);
						}
					} catch (WebException e) {
						WebExceptionStatus s = e.Status;
						if (s == WebExceptionStatus.ProtocolError) {
							WebResponse r = e.Response;
							ircInterface.SendMessage(command.Channel, e.Message + " - Response URI: " + e.Response.ResponseUri);
							return;
						}
						ircInterface.SendMessage(command.Channel, "Web Exception: " + e.Message + ", response URI: " + e.Response.ResponseUri);
						return;
					}
				}
			}

			// No need to run the updater when it's a meta update
			if (meta) {
				ircInterface.SendMessage(command.Channel, "Meta-update completed.");
				return;
			}

			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();

			Process proc = new Process();
			proc.StartInfo.FileName = "UpdateManager.exe";
			proc.StartInfo.Arguments = Program.Version;

			proc.Start();

			System.Net.Sockets.SocketInformation si = ircInterface.DuplicateAndClose(proc.Id);
			//System.Net.Sockets.Socket s = ircInterface.GetHandle();

			BinaryFormatter bf = new BinaryFormatter();

			using (Stream str = File.Open("socket.stream", FileMode.Create)) {
				bf.Serialize(str, si);
			}
			Environment.Exit(0);
		}
	}
}
