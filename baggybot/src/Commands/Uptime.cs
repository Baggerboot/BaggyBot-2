using System;

namespace BaggyBot.Commands
{
	internal class Uptime : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "uptime";
		public override string Usage => "";
		public override string Description => "Shows how long I've been running.";

		private readonly DateTime startTime;
		public Uptime()
		{
			startTime = DateTime.Now;
		}

		public override void Use(CommandArgs command)
		{
			var diff = DateTime.Now - startTime;
			var d = diff.Days + (diff.Days == 1 ? " day" : " days");
			var h = diff.Hours + (diff.Hours == 1 ? " hour" : " hours");
			var m = diff.Minutes + (diff.Minutes == 1 ? " minute" : " minutes");
			var s = diff.Seconds + (diff.Seconds == 1 ? " second" : " seconds");
			command.Reply($"I have been running for {d}, {h}, {m} and {s}");
		}
	}
}
