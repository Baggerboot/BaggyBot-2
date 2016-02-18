using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class GoogleSearch : Command
	{
		public override PermissionLevel Permissions { get { return PermissionLevel.All; } }
		public override string Usage => "<search term>";
		public override string Description => "Searches Google for the given term, and returns the first result.";

		public override void Use(CommandArgs command)
		{
			var query = command.FullArgument;
			if (query == null)
			{
				InformUsage(command);
				return;
			}

			var rq = WebRequest.Create($@"https://ajax.googleapis.com/ajax/services/search/web?v=1.0&q={Uri.EscapeDataString(query)}&rsz=1&hl=en");
			dynamic obj;
			using (var response = rq.GetResponse())
			{
				var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
				obj = JObject.Parse(text);
			}

			dynamic result = obj.responseData.results[0];

			command.ReturnMessage($"{result.url} - \x02{WebUtility.HtmlDecode((string)result.titleNoFormatting)}\x02");
		}
	}
}
