using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Commands
{
	internal abstract class StdioBridge : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		protected readonly Dictionary<ChatChannel, Process> Processes = new Dictionary<ChatChannel, Process>();

		protected TimeSpan ReadDelay = TimeSpan.FromMilliseconds(200);

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

		protected void Reset(ChatChannel channel)
		{
			if (Processes.ContainsKey(channel))
			{
				var proc = Processes[channel];
				proc.Kill();
				proc.Dispose();
			}
			Spawn(channel);
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
				var lastAddition = DateTime.Now;
				var messagePool = new List<string>();

				while (!proc.HasExited && !proc.StandardOutput.EndOfStream)
				{
					var line = proc.StandardOutput.ReadLine();
					var readTime = DateTime.Now;

					if (!Client.Capabilities.AllowsMultilineMessages)
					{
						Client.SendMessage(channel, $"{Frm.M}{line}{Frm.M}");
						return;
					}

					lock (messagePool)
					{
						messagePool.Add(line);
						lastAddition = readTime;
					}

					Task.Run(() =>
					{
						Task.Delay(ReadDelay).Wait();
						lock (messagePool)
						{
							if (lastAddition == readTime)
							{
								// No new messages were added recently, so send all collected lines to the client.
								var lines = string.Join("\n", messagePool);
								Client.SendMessage(channel, $"{Frm.MMultiline}{lines}{Frm.MMultiline}");
								messagePool.Clear();
							}
						}
					});
				}
			});

			Task.Run(() =>
			{
				var lastAddition = DateTime.Now;
				var messagePool = new List<string>();

				while (!proc.HasExited && !proc.StandardError.EndOfStream)
				{
					var line = proc.StandardError.ReadLine();
					var readTime = DateTime.Now;

					if (!Client.Capabilities.AllowsMultilineMessages)
					{
						Client.SendMessage(channel, $"{Frm.M}{line}{Frm.M}");
						return;
					}

					lock (messagePool)
					{
						messagePool.Add(line);
						lastAddition = readTime;
					}

					Task.Run(() =>
					{
						Task.Delay(ReadDelay);
						lock (messagePool)
						{
							if (lastAddition == readTime)
							{
								// No new messages were added recently, so send all collected lines to the client.
								var lines = string.Join("\n", messagePool.Select(m => $"!!{m}"));
								Client.SendMessage(channel, $"{Frm.MMultiline}{lines}{Frm.MMultiline}");
								messagePool.Clear();
							}
						}
					});
				}
			});
		}

		protected void Write(ChatChannel channel, string data)
		{
			Processes[channel].StandardInput.WriteLine(data);
		}

		protected void SendKey(ChatChannel channel, string key)
		{
			var seq = GetEscapeSequence(key);
			Write(channel, seq);
		}

		private string GetEscapeSequence(string key)
		{
			switch (key)
			{
				case "^@":
				case "\\0":
					return EscapeSequence.Null;
				case "^C":
					return EscapeSequence.EndOfText;
				case "^D":
					return EscapeSequence.EndOfTransmission;
				case "^H":
					return EscapeSequence.Backspace;
				case "^J":
				case "\\n":
					return EscapeSequence.LineFeed;
				case "^M":
				case "\\r":
					return EscapeSequence.CarriageReturn;
				case "^Q":
					return EscapeSequence.DeviceControl1;
				case "^S":
					return EscapeSequence.DeviceControl3;
				case "^[":
				case "\\e":
					return EscapeSequence.Escape;
			}
			throw new ArgumentException("Invalid escape sequence");
		}

		protected static class EscapeSequence
		{
			public const string Null = "\0";
			public const string EndOfText = "\x3";
			public const string EndOfTransmission = "\x4";
			public const string Backspace = "\x8";
			public const string LineFeed = "\n";
			public const string CarriageReturn = "\r";
			public const string DeviceControl1 = "\x11";
			public const string DeviceControl3 = "\x13";
			public const string Escape = "\x1b";
		}
	}
}