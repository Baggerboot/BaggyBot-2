using System;
using System.Diagnostics;

namespace BaggyBot.Commands
{
	internal class Diagnostics : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "diag";
		public override string Usage => "";
		public override string Description => "Print some diagnostics about the currently running instance.";

		public override void Use(CommandArgs command)
		{
			var botBit = Environment.Is64BitProcess ? "64" : "32";
			var osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
			var hasDebugger = Debugger.IsAttached ? "(Debugger attached)" : "";
#if DEBUG
			var build = "Debug Build";
#else
			var build = "Release Build";
#endif
			var managedMemory = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 0);

			double totalMemory;
			using (var proc = Process.GetCurrentProcess())
			{
				totalMemory = Math.Round(proc.PrivateMemorySize64/1024.0/1024.0, 0);
			}

			var message = $"BaggyBot {Bot.Version} ({botBit}-bit) -- Running on {Environment.OSVersion.VersionString} ({osBit}-bit) -- {build} {hasDebugger} -- {managedMemory}MB managed, {totalMemory}MB total";
			command.ReturnMessage(message);
		}
	}
}