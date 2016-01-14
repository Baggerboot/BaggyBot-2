
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	class UrbanDictionary : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			if (string.IsNullOrWhiteSpace(command.FullArgument))
			{
				command.ReturnMessage("usage: -ur <search term>");
				return;
			}

			var term = command.FullArgument.Replace(' ', '+');

			var rq = WebRequest.Create(@"https://www.kimonolabs.com/api/2rtoqq4w?apikey=rNsrJf1WHUHasu47YT5pxcr5anzCkji8&term=" + term);
			var response = rq.GetResponse();
			var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
			dynamic obj = JObject.Parse(text);

			string name;
			string definition;
			string example;

			name = (string)obj.results.top[0].name.text;

			try
			{
				definition = (string)obj.results.top[0].definition;
			}
			catch (Exception)
			{
				try
				{
					definition = (string)obj.results.top[0].definition.text;
				}
				catch (Exception)
				{
					command.Reply("unable to find a definition for \"{0}\"", command.FullArgument);
					return;
				}
			}

			try
			{
				example = (string)obj.results.top[0].example;
			}
			catch (Exception)
			{
				try
				{
					example = (string)obj.results.top[0].example.text;
				}
				catch (Exception)
				{
					command.Reply("unable to find a definition for \"{0}\"", command.FullArgument);
					return;
				}
			}



			name = Regex.Replace(name, @"\t|\n|\r", " ");
			definition = Regex.Replace(definition, @"\t|\n|\r", " ");
			example = Regex.Replace(example, @"\t|\n|\r", " ");

			if (definition.Length > 255)
			{
				definition = definition.Substring(0, 255);
				definition += " (...)";
			}

			if (example.Length > 255)
			{
				example = example.Substring(0, 255);
				example += " (...)";
			}
			command.ReturnMessage("\u0002{0}\u0002: {1} - \u001d{2}\u001d - http://urbandictionary.com/define.php?term={3}", name, definition, example, term);
		}
	}
}
