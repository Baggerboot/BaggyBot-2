using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Commands
{
	internal abstract class StdioBridge : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		protected readonly Dictionary<ChatChannel, Process> Processes = new Dictionary<ChatChannel,Process>();

		private string command, arguments;

		protected void Init(string command, string arguments = "")
		{
			this.command = command;
			this.arguments = arguments;
		}

		protected void EnsureProcess(ChatChannel channel)
		{
			if (!Processes.ContainsKey(channel))
			{
				Spawn(channel);
			}
		}

		private void Spawn(ChatChannel channel)
		{
			var info = new ProcessStartInfo
			{
				FileName = command,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			var proc = new Process();
			Processes[channel] = proc;

			proc.StartInfo = info;
			proc.Start();

			Task.Run(() =>
			{
				while (!proc.HasExited && !proc.StandardOutput.EndOfStream)
				{
					var line = proc.StandardOutput.ReadLine();
					Client.SendMessage(channel, "--> " + line);
				}
			});

			Task.Run(() =>
			{
				while (!proc.HasExited && !proc.StandardError.EndOfStream)
				{
					var line = proc.StandardError.ReadLine();
					Client.SendMessage(channel, "!!> " + line);
				}
			});

		}

		protected void Write(ChatChannel channel, string data)
		{
			Processes[channel].StandardInput.WriteLine(data);
		}
	}
}