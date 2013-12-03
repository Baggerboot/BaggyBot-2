using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace BaggyBot.Commands
{
	class ResolveReverse : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public ResolveReverse(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length != 1) {
				ircInterface.SendMessage(command.Channel, "Usage: -rdns <ip>");
				return;
			}
			IPAddress hostIPAddress;
			try {
				hostIPAddress = IPAddress.Parse(command.Args[0]);
			} catch (FormatException) {
				ircInterface.SendMessage(command.Channel, "I was unable to parse the IP address you entered.");
				return;
			}
			IPHostEntry hostEntry;
			try {
				hostEntry = Dns.GetHostEntry(hostIPAddress);
			} catch (ArgumentException) {
				ircInterface.SendMessage(command.Channel, "I can't do a lookup on 0.0.0.0  ::0");
				return;
			} catch (System.Net.Sockets.SocketException) {
				ircInterface.SendMessage(command.Channel, "Unable to do a lookup on " + hostIPAddress.ToString() + ". Most likely a reverse DNS entry does not exist for this address.");
				return;
			}
			// Get the IP address list that resolves to the host names contained in 
			// the Alias property.
			IPAddress[] address = hostEntry.AddressList;
			// Get the alias names of the addresses in the IP address list.
			String[] alias = hostEntry.Aliases;

			Console.WriteLine("Host name : " + hostEntry.HostName);
			ircInterface.SendMessage(command.Channel, String.Format("{0} resolves to {1}", command.Args[0], hostEntry.HostName));

			Console.WriteLine("\nAliases :");
			for (int index = 0; index < alias.Length; index++) {
				Console.WriteLine(alias[index]);
			}
			Console.WriteLine("\nIP address list : ");
			for (int index = 0; index < address.Length; index++) {
				Console.WriteLine(address[index]);
			}
		}
	}
}
