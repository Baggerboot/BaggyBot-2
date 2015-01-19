using System.Diagnostics;
using System.IO;

namespace BaggyBot.Commands
{
	class Update : ICommand
	{
		private readonly Bot bot;
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
				var proc = new Process();
				proc.StartInfo = new ProcessStartInfo
				{
					FileName = "sh",
					Arguments = "autoupdate.sh"
				};
				proc.Start();
				command.ReturnMessage("Downloading update...");
				proc.WaitForExit();
				if (proc.ExitCode != 0) {
					command.ReturnMessage("Downloader exited with code {0}. Update process aborted. No files were changed.", proc.ExitCode);
					return;
				}
                Logger.Log("Replacing files");
				File.Replace("BaggyBot20.exe.new", "BaggyBot20.exe", null);
				File.Replace("CsNetLib2.dll.new", "CsNetLib2.dll", null);
				Logger.Log("Requesting a restart", LogLevel.Info);
				bot.RequestUpdate(requestChannel, true);
			}
		}
	}
}
