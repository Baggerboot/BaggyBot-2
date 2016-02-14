using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	internal class Resolve : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<hostname>";
		public override string Description => "Performs an A and AAAA lookup on a given hostname, and returns all associated IP addresses.";

		public override void Use(CommandArgs command)
		{
			var parser = new CommandParser(new Operation().AddArgument("hostname", null));
			OperationResult result;
			try
			{
				result = parser.Parse(command.FullArgument);
			}
			catch (InvalidCommandException e)
			{
				command.ReturnMessage(e.Message);
				return;
			}
			if (result.Arguments["hostname"] == null)
			{
				command.Reply("Usage: -resolve <hostname>");
			}
			else
			{
				ResolveHost(result.Arguments["hostname"], command);
			}
		}

		private void ResolveHost(string hostname, CommandArgs command)
		{
			IPHostEntry hostEntry;
			try
			{
				hostEntry = Dns.GetHostEntry(hostname);
			}
			catch (SocketException e)
			{
				command.ReturnMessage(e.Message);
				return;
			}
			catch (Exception)
			{
				command.ReturnMessage("It looks like you entered a wrong domain name.");
				return;
			}

			if (hostEntry.AddressList.Length > 0)
			{
				var addr = string.Join(", ", hostEntry.AddressList.Select(host => host.ToString()));

				command.Reply($"IP address(es) belonging to {command.Args[0]}: {addr}");
			}
		}
	}
}
