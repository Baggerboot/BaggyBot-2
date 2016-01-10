using System;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace BaggyBot.Commands
{
	class Uptime : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private readonly DateTime startTime;
		private readonly PerformanceCounterCategory cat = new PerformanceCounterCategory("Processor Information");
		private readonly string[] instances;
		public Uptime()
		{
			instances = cat.GetInstanceNames();

			startTime = DateTime.Now;
		}

		public void Use(CommandArgs command)
		{
			var diff = DateTime.Now - startTime;
			var d = diff.Days + (diff.Days == 1 ? " day" : " days");
			var h = diff.Hours + (diff.Hours == 1 ? " hour" : " hours");
			var m = diff.Minutes + (diff.Minutes == 1 ? " minute" : " minutes");
			var s = diff.Seconds + (diff.Seconds == 1 ? " second" : " seconds");
			command.Reply("I have been running for {0}, {1}, {2} and {3}", d, h, m, s);
		}
	}
}
