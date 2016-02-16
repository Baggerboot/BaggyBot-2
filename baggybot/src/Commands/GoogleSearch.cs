using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Commands;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class GoogleSearch : Command
	{
		public override PermissionLevel Permissions { get { return PermissionLevel.All; } }
		public override string Usage => "";
		public override string Description => "";

		public override void Use(CommandArgs command)
		{
			var query = command.FullArgument;

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
