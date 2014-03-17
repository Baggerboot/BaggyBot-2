using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace BaggyBot.Commands
{
	class Update : ICommand
	{
		private Bot bot;
		public Update(Bot bot)
		{
			this.bot = bot;
		}
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		private string requestChannel;

		public void Use(CommandArgs command)
		{
			Logger.Log("Preparing to update");
			requestChannel = command.Channel;

			if (command.Args.Length == 1 && command.Args[0] == "--no-dl") {
				Logger.Log("Requesting a restart", LogLevel.Info);
				bot.RequestUpdate(requestChannel, false);
			} else {
				Logger.Log("bleh");
				var proc = new Process();
				proc.StartInfo = new ProcessStartInfo
				{
					FileName = "sh",
					Arguments = "autoupdate.sh"
				};
				proc.Start();
				command.ReturnMessage("Downloading update...");
				proc.WaitForExit();
				Logger.Log("Requesting an update", LogLevel.Info);
				bot.RequestUpdate(requestChannel, true);
			}
		}
	}
}
