using System;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands.Wikipedia
{
	internal class Wikipedia : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "wiki";
		public override string Usage => "<search term>";
		public override string Description => "Searches for an article on Wikipedia.";

		public override void Use(CommandArgs command)
		{
			if (string.IsNullOrWhiteSpace(command.FullArgument))
			{
				InformUsage(command);
				return;
			}

			var uri = new Uri($"http://en.wikipedia.org/w/api.php?format=json&action=query&titles={command.FullArgument}&prop=info&inprop=url");

			var rq = WebRequest.Create(uri);
			WebResponse response;
			try
			{
				response = rq.GetResponse();
			}
			catch (WebException e)
			{
				command.Reply($"Unable to query Wikipedia. {e.Message}");
				return;
			}
			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				var data = sr.ReadToEnd();
				var rs = JsonConvert.DeserializeObject<QueryResponse>(data);
				var page = rs.query.pages.First.First.ToObject<Page>();

				if (page.pageid == 0)
				{
					command.Reply($"I could not find a page named {command.FullArgument}");
				}
				else
				{
					command.ReturnMessage($"{page.title} ({page.canonicalurl}): {GetContent(page.canonicalurl)}");
				}
			}
		}

		private string GetContent(string url)
		{
			// TODO: Enhance Wikipedia command output
			// - Send more text when allowed.
			// - Attach an image.
			// - Attach additional data.
			// Add ChatClient capabilities as required
			var rq = WebRequest.Create(url + "?action=render");
			var rs = rq.GetResponse();
			var doc = new HtmlDocument();
			doc.Load(rs.GetResponseStream(), Encoding.UTF8);
			foreach (var cite in doc.DocumentNode.SelectNodes(".//sup[@class=\"reference\"]"))
			{
				cite.Remove();
			}
			var nodes = doc.DocumentNode.SelectNodes("//table[@class=\"infobox\"]/tr[td][th]") ?? new HtmlNodeCollection(null);
			foreach (var tr in nodes)
			{
				var th = tr.SelectSingleNode("th").InnerText;
				var td = tr.SelectSingleNode("td").InnerText;
			}

			var firstParagraph = doc.DocumentNode.SelectSingleNode(".//p[1]");

			var decodedText = WebUtility.HtmlDecode(firstParagraph.InnerText);

			return decodedText;
		}
	}
}
