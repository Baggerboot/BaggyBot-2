using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class UrbanDictionary : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<search term>";
		public override string Description => "Searches Urban Dictionary for a given term.";

		public override void Use(CommandArgs command)
		{
			if (string.IsNullOrWhiteSpace(command.FullArgument))
			{
				command.ReturnMessage("usage: -ur <search term>");
				return;
			}

			var term = command.FullArgument.Replace(' ', '+');

			var rq = WebRequest.Create(@"http://api.urbandictionary.com/v0/define?term=" + term);
			var response = rq.GetResponse();
			var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
			dynamic obj = JObject.Parse(text);

			if (obj.result_type == "no_results")
			{
				command.Reply("no results found.");
			}
			else
			{
				string name = obj.list[0].word;
				string definition = obj.list[0].definition;
				string example = obj.list[0].example;
				string permalink = obj.list[0].permalink;
				name = Regex.Replace(name, @"\t|\n|\r", " ");
				definition = Regex.Replace(definition, @"\t|\n|\r", " ");
				example = Regex.Replace(example, @"\t|\n|\r", " ");

				if (definition.Length > 255)
				{
					definition = definition.Substring(0, 250);
					definition += " (...)";
				}

				if (example.Length > 255)
				{
					example = example.Substring(0, 250);
					example += " (...)";
				}
				var exampleString = string.IsNullOrWhiteSpace(example) ? string.Empty : $" - \u001d{example}\u001d";


				command.ReturnMessage("\u0002{0}\u0002: {1}{2} - {3}", name, definition, exampleString, permalink);
			}
		}
	}
}
