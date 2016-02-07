using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class UrbanDictionary : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<search term>";
		public string Description => "Searches Urban Dictionary for a given term.";

		public void Use(CommandArgs command)
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

			string name;
			string definition;
			string example = string.Empty;

			name = (string)obj.list[0].word;

			try
			{
				definition = (string)obj.list[0].definition;
			}
			catch (Exception)
			{
				command.Reply("unable to find a definition for \"{0}\"", command.FullArgument);
				return;
			}

			try
			{
				example = (string)obj.list[0].example;
			}
			catch (Exception)
			{
				// ignored
			}

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

			command.ReturnMessage("\u0002{0}\u0002: {1}{2} - urbandictionary.com/define.php?term={3}", name, definition, exampleString, term);
		}
	}
}
