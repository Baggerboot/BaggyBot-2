using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	class Wikipedia : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			var uri = new Uri(string.Format("http://en.wikipedia.org/w/api.php?format=json&action=query&titles={0}&prop=revisions&rvprop=content", command.FullArgument));

			var rq = WebRequest.Create(uri);
			var response = rq.GetResponse();

			using (var sr = new StreamReader(response.GetResponseStream())) {
				dynamic jsonObj = JObject.Parse(sr.ReadToEnd());
				Console.WriteLine("Title: " + jsonObj.query.pages[0].title);
			}
		}
	}
}
