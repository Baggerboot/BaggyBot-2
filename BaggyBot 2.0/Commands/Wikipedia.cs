using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class Wikipedia : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<search term>";
		public string Description => "Searches for an article on Wikipedia.";

		public void Use(CommandArgs command)
		{
			var uri = new Uri(
				$"http://en.wikipedia.org/w/api.php?format=json&action=query&titles={command.FullArgument}&prop=revisions&rvprop=content");

			var rq = WebRequest.Create(uri);
			var response = rq.GetResponse();

			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				string data = sr.ReadToEnd();
				dynamic jsonObj = JObject.Parse(data);
				Console.WriteLine("Title: " + jsonObj.query.pages[0].title);
			}
		}
	}
}
