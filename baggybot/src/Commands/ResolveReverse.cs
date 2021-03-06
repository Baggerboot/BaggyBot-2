﻿using System;
using System.Net;
using System.Net.Sockets;

namespace BaggyBot.Commands
{
	internal class ResolveReverse : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "rdns";
		public override string Usage => "<IP address>";
		public override string Description => "Performs a reverse DNS lookup on a given IP address, and returns the associated hostname.";

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length != 1)
			{
				InformUsage(command);
				return;
			}
			IPAddress hostIpAddress;
			try
			{
				hostIpAddress = IPAddress.Parse(command.Args[0]);
			}
			catch (FormatException)
			{
				command.ReturnMessage("I was unable to parse the IP address you entered.");
				return;
			}
			IPHostEntry hostEntry;
			try
			{
				hostEntry = Dns.GetHostEntry(hostIpAddress);
			}
			catch (ArgumentException)
			{
				command.ReturnMessage("I can't do a lookup on 0.0.0.0 or ::0");
				return;
			}
			catch (SocketException)
			{
				command.ReturnMessage($"Unable to do a lookup on {hostIpAddress}. Most likely a reverse DNS entry does not exist for this address.");
				return;
			}
			// Get the IP address list that resolves to the host names contained in 
			// the Alias property.
			var address = hostEntry.AddressList;
			// Get the alias names of the addresses in the IP address list.
			var alias = hostEntry.Aliases;

			Console.WriteLine("Host name : " + hostEntry.HostName);
			command.Reply($"{command.Args[0]} resolves to {hostEntry.HostName}");

			Console.WriteLine("\nAliases :");
			for (var index = 0; index < alias.Length; index++)
			{
				Console.WriteLine(alias[index]);
			}
			Console.WriteLine("\nIP address list : ");
			for (var index = 0; index < address.Length; index++)
			{
				Console.WriteLine(address[index]);
			}
		}
	}
}
