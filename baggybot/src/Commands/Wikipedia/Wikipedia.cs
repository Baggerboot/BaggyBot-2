using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BaggyBot.ExternalApis;
using BaggyBot.Plugins;
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
					var document = GetDocument(page.canonicalurl);
					var image = GetImageUrl(document);
					var attachment = image == null ? null : new ImageAttachment(image);
					command.ReturnMessage($"{page.title} ({page.canonicalurl}): {GetContent(document)}", attachment);
				}
			}
		}

		private HtmlDocument GetDocument(string url)
		{
			var rq = WebRequest.Create(url + "?action=render");
			var rs = rq.GetResponse();
			var doc = new HtmlDocument();
			doc.Load(rs.GetResponseStream(), Encoding.UTF8);
			foreach (var cite in doc.DocumentNode.SelectNodes(".//sup[@class=\"reference\"]"))
			{
				cite.Remove();
			}
			return doc;
		}

		private string GetImageUrl(HtmlDocument doc)
		{
			var image = doc.DocumentNode.SelectNodes("//img").FirstOrDefault(node => node.GetAttributeValue("data-file-width", 100) > 100 && node.GetAttributeValue("data-file-height", 100) > 100);
			var source = image?.GetAttributeValue("src", null);
			if (source == null) return null;

			var imageUrl = $"https:{source}";
			if (Client.Capabilities.RequireReupload)
			{
				return Imgur.Upload(imageUrl);
			}
			else
			{
				return imageUrl;
			}
		}

		private string GetContent(HtmlDocument doc)
		{
			// TODO: Enhance Wikipedia command output
			// - Send more text when allowed.
			// - Attach an image.
			// - Attach additional data.
			// Add ChatClient capabilities as required

			var nodes = doc.DocumentNode.SelectNodes("//table[@class=\"infobox\"]/tr[td][th]") ?? new HtmlNodeCollection(null);
			var pairs = nodes.Select(tr => new KeyValuePair<string,string>(tr.SelectSingleNode("th").InnerText, tr.SelectSingleNode("td").InnerText)).ToList();
			
			var firstParagraph = doc.DocumentNode.SelectSingleNode("/p[1]");
			var decodedText = WebUtility.HtmlDecode(firstParagraph.InnerText);
			return decodedText;
		}
	}
}
