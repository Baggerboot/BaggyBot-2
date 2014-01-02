using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace BaggyBot.Commands
{
	class Resolve : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			if (command.Args.Length != 1) {
				command.Reply("Usage: -resolve <hostname>");
			}
			IPHostEntry hostEntry;
			try {
				hostEntry = Dns.GetHostEntry(command.Args[0]);
			} catch (Exception) {
				command.ReturnMessage("It looks like you entered a wrong domain name.");
				return;
			}



			if (hostEntry.AddressList.Length > 0) {
				string addr = "";
				foreach (var address in hostEntry.AddressList) {
					addr += ", " + address;
				}

				addr = addr.Substring(2);

				command.Reply(String.Format("IP address(es) belonging to {0}: {1}", command.Args[0], addr));
			}
		}
	}
}
