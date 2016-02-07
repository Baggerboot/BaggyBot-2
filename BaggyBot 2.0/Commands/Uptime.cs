using System;

namespace BaggyBot.Commands
{
	internal class Uptime : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "";
		public string Description => "Shows how long I've been running.";

		private readonly DateTime startTime;
		public Uptime()
		{
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
