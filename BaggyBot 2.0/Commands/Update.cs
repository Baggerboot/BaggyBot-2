//#define	LINUX
#define	WINDOWS

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
				ircInterface.SendMessage(command.Channel, "Downloading update.");
				using (WebClient Client = new WebClient()) {
					try {
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/BaggyBot20.exe", "BaggyBot20_new.exe");
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/CSNetLib.dll", "CSNetLib_new.dll");
						Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/IRCSharp.dll", "IRCSharp_new.dll");
					} catch (WebException e) {
						ircInterface.SendMessage(command.Channel, "Web Exception: " + e.Message + ", more information: " + e.HelpLink);
						return;
					}
				}
			}
			Process.Start("sh", String.Format("update.sh " + Program.Version));

			ircInterface.Disconnect("Updating");
			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();
		}
	}
}
