using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace BaggyBot.Commands
{
	class Update : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Update(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, "Starting download...");
			using (WebClient Client = new WebClient()) {
				try {
					Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/BaggyBot20.exe", "BaggyBot20.exe");
					Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/CSNetLib.dll", "CSNetLib.dll");
					Client.DownloadFile("http://home1.jgeluk.net/files/baggybot20/IRCSharp.dll", "IRCSharp.dll");
				}catch(Exception e)
				{
					ircInterface.SendMessage(command.Channel, "Error: " + e.Message);
				}
			}

		}
	}
}
