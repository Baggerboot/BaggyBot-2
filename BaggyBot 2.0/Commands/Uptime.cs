using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Uptime : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private DateTime startTime;

		public Uptime()
		{
			startTime = DateTime.Now;
		}

		public void Use(CommandArgs command)
		{
			var diff = DateTime.Now - startTime;
			string d = diff.Days + (diff.Days == 1 ? " day" : " days");
			string h = diff.Hours + (diff.Hours == 1 ? " hour" : " hours");
			string m = diff.Minutes + (diff.Minutes == 1 ? " minute" : " minutes");
			string s = diff.Seconds + (diff.Seconds == 1 ? " second" : " seconds");
			command.Reply("I have been running for {0}, {1}, {2} and {3}", d, h, m, s);
		}
	}
}
