using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using BaggyBot.Configuration;
using BaggyBot.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class GoogleSearch : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "g";
		public override string Usage => "<search term>";
		public override string Description => "Searches Google for the given term, and returns the first result.";

		private readonly Bing bing = new Bing();

		public override void Use(CommandArgs command)
		{
			var query = command.FullArgument;
			if (query == null)
			{
				InformUsage(command);
				return;
			}
			var url = bing.WebSearch(query);
			if (url == null)
			{
				command.Reply("there don't seem to be any results.");
			}
			else
			{
				command.Reply($"{url.displayUrl} -- {url.snippet}");
			}
		}
	}
}
