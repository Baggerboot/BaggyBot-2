using System.Diagnostics;
using BaggyBot.Monitoring;

namespace BaggyBot.Commands
{
	internal class Update : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "[--no-dl]";
		public override string Description => "Downloads a new update and makes me restart to apply it.";

		private readonly Bot bot;
		public Update(Bot bot)
		{
			this.bot = bot;
		}
		
		private string requestChannel;

		public override void Use(CommandArgs command)
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
