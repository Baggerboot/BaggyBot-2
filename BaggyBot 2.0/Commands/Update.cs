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
			if (command.Args.Length == 2 && command.Args[0] == "-m" && command.Args[1] == "-d") {
				Logger.Log("Performing meta-update...", LogLevel.Debug);
				using (WebClient Client = new WebClient()) {
					try {
						Client.DownloadFile("http://home2.jgeluk.net/files/baggybot20/UpdateManager.exe", "UpdateManager_new.exe");
						File.Delete("UpdateManager.exe");
						File.Move("UpdateManager_new.exe", "UpdateManager.exe");
						ircInterface.SendMessage(command.Channel, "Meta-update finished.");
						return;
					} catch (WebException e) {
						ircInterface.SendMessage(command.Channel, "Web Exception: " + e.Message + ", more information: " + e.HelpLink);
						return;
					}
				}
			}

			if (command.Args.Length == 1 && command.Args[0] == "-d") {
				Logger.Log("Updating...", LogLevel.Debug);
				using (WebClient Client = new WebClient()) {
					try {
						Client.DownloadFile("http://home2.jgeluk.net/files/baggybot20/BaggyBot20.exe", "BaggyBot20_new.exe");
						Client.DownloadFile("http://home2.jgeluk.net/files/baggybot20/CSNetLib.dll", "CSNetLib_new.dll");
						Client.DownloadFile("http://home2.jgeluk.net/files/baggybot20/IRCSharp.dll", "IRCSharp_new.dll");
					} catch (WebException e) {
						ircInterface.SendMessage(command.Channel, "Web Exception: " + e.Message + ", more information: " + e.HelpLink);
						return;
					}
				}
			}

			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();

			//Process.Start("sh", String.Format("update.sh " + Program.Version));

			Process proc = new Process();
			//proc.StartInfo.FileName = "mono";
			//proc.StartInfo.Arguments = "UpdateManager.exe " + Program.Version;
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
