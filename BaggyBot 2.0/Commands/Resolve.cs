using System;
using System.Net;

namespace BaggyBot.Commands
{
	internal class Resolve : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<hostname>";
		public override string Description => "Performs an A and AAAA lookup on a given hostname, and returns all associated IP addresses.";

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length != 1)
			{
				command.Reply("Usage: -resolve <hostname>");
				return;
			}
			IPHostEntry hostEntry;
			try
			{
				hostEntry = Dns.GetHostEntry(command.Args[0]);
			}
			catch (Exception)
			{
				command.ReturnMessage("It looks like you entered a wrong domain name.");
				return;
			}

			if (hostEntry.AddressList.Length > 0)
			{
				var addr = string.Empty;
				foreach (var address in hostEntry.AddressList)
				{
					addr += ", " + address;
				}

				addr = addr.Substring(2);

				command.Reply($"IP address(es) belonging to {command.Args[0]}: {addr}");
			}
		}
	}
}
