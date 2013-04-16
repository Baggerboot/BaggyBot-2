#define	LINUX
//#define	WINDOWS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Net;

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
			if (command.Args.Length == 1 && command.Args[0] == "-d") {
				ircInterface.SendMessage(command.Channel, "Starting download...");
				using (WebClient Client = new WebClient()) {
					try {
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/BaggyBot20.exe", "BaggyBot20.exe");
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/CSNetLib.dll", "CSNetLib.dll");
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/IRCSharp.dll", "IRCSharp.dll");
					} catch (WebException e) {
						ircInterface.SendMessage(command.Channel, "Web Exception: " + e.Message + ", more information: " + e.HelpLink);
					}
				}
			}
#if LINUX
			Process.Start("mono", "BaggyBot20.exe -nc -pv " + Program.Version);
#elif WINDOWS
			Process.Start("BaggyBot20.exe", "-pv " + Program.Version);
#endif
			ircInterface.Disconnect();
			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();
		}
	}
}
