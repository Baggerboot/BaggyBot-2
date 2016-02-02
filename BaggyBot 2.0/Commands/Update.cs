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
			Logger.Log(this, "Preparing to update");
			requestChannel = command.Channel;

			if (command.Args.Length == 1 && command.Args[0] == "--no-dl")
			{
				Logger.Log(this, "Requesting a restart", LogLevel.Info);
				bot.RequestUpdate(requestChannel, false);
			}
			else
			{
				var proc = new Process();
				proc.StartInfo = new ProcessStartInfo
				{
					FileName = "bash",
					UseShellExecute = false,
					Arguments = "-c 'cd .. && git pull && xbuild'",
					RedirectStandardOutput = true
				};
				proc.Start();
				command.ReturnMessage("Building update...");
				var output = proc.StandardOutput.ReadToEnd();
				Logger.Log(this, output);
				proc.WaitForExit();
				if (proc.ExitCode != 0)
				{
					command.ReturnMessage("Updater exited with code {0}. Update process aborted. No files were changed.", proc.ExitCode);
					return;
				}
				Logger.Log(this, "Requesting a restart", LogLevel.Info);
				bot.Shutdown();
				//bot.RequestUpdate(requestChannel, true);
			}
		}
	}
}
