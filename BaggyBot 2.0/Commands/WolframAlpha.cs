using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Net;

namespace BaggyBot.Commands
{
	class WolframAlpha : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			var uri = new Uri(string.Format("http://api.wolframalpha.com/v2/query?appid=QK2T79-JX9QTVP5RE&input={0}&ip={1}&format=plaintext&units=metric", command.FullArgument, command.Sender.Hostmask));

			var rq = HttpWebRequest.Create(uri);
			var response = rq.GetResponse();
			
			var xmd = new XmlDocument();
			xmd.Load(response.GetResponseStream());
			var queryresult = xmd.GetElementsByTagName("queryresult").Item(0);

			if (queryresult.Attributes["success"].Value == "false") {
				var error = queryresult.Attributes["error"].Value;
				if (error == "false") {
					command.Reply("Unable to compute the answer.");
				} else {
					command.Reply("An error occurred: " + error);
				}
				return;
			}
			if (queryresult.FirstChild.Name == "assumptions") {
				var options = queryresult.FirstChild.FirstChild.ChildNodes;
				var descriptions = new List<string>();
				for (int i = 0; i < options.Count; i++) {
					var node = options[i];
					descriptions.Add("\"" + node.Attributes["desc"].Value + "\"");
				}

				string first = string.Join(", ", descriptions.Take(descriptions.Count - 1));

				command.Reply("Ambiguity between {0} and {1}. Please try again.", first, descriptions.Last());
				return;
			}

			var replace = new Func<string,string>((s) => s.Replace("\n", " -- "));
			
			var input = queryresult.FirstChild;
			var title = replace(input.Attributes["title"].Value);
			var result = replace(input.NextSibling.InnerText);

			command.Reply("({0}: {1}): {2}", title, replace(input.InnerText), result);
		}
	}
}
