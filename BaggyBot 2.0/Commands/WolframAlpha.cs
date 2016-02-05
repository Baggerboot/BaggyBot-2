using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Net;
using BaggyBot.Configuration;

namespace BaggyBot.Commands
{
	class WolframAlpha : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		private XmlNode lastDisplayedResult;


		private XmlNode GetNextSibling(XmlNode startingPoint)
		{
			var currentSibling = startingPoint;

			if (currentSibling == null)
				return null;

			do
			{
				currentSibling = currentSibling.NextSibling;
				if (currentSibling == null)
				{
					return null;
				}
			} while (string.IsNullOrEmpty(currentSibling.InnerText));

			return currentSibling;
		}


		private string ShowMore()
		{
			string returnData = null;
			if (lastDisplayedResult == null) return null;

			var result = GetNextSibling(lastDisplayedResult);
			if (result != null)
			{
				returnData = "\x02" + result.Attributes["title"].Value + "\x02: " + result.InnerText;
				lastDisplayedResult = result;
			}
			return returnData;
		}

		public string ReplaceNewlines(string input)
		{
			return input.Replace("\n", " -- ");
		}

		public void Use(CommandArgs command)
		{
			if (string.IsNullOrWhiteSpace(command.FullArgument))
			{
				command.Reply("Usage: '-wa <WolframAlpha query>' -- Displays information acquired from http://www.wolframalpha.com. In addition to this, you can use the command '-wa more' to display additional information about the last subject");
				return;
			}

			if (command.FullArgument == "more")
			{
				var more = ShowMore();
				if (more == null)
				{
					command.ReturnMessage("No more information available.");
				}
				else
				{
					var secondItem = ShowMore();
					if (secondItem != null)
					{
						more += " -- " + secondItem;
					}
					command.ReturnMessage(ReplaceNewlines(more));
				}
				return;
			}

			lastDisplayedResult = null;

			var appid = ConfigManager.Config.Integrations.WolframAlpha;

			var uri = string.Format("http://api.wolframalpha.com/v2/query?appid={0}&input={1}&ip={2}&format=plaintext&units=metric", appid, Uri.EscapeDataString(command.FullArgument), command.Sender.Hostmask);
			//var escaped = Uri.EscapeDataString(uri);

			var rq = WebRequest.Create(uri);
			var response = rq.GetResponse();

			var xmd = new XmlDocument();
			xmd.Load(response.GetResponseStream());
			var queryresult = xmd.GetElementsByTagName("queryresult").Item(0);

			if (queryresult.Attributes["success"].Value == "false")
			{
				var error = queryresult.Attributes["error"].Value;
				if (error == "false")
				{
					command.Reply("Unable to compute the answer.");
					var didyoumeans = GetDidYouMeans(xmd.GetElementsByTagName("didyoumean"));
					if (!string.IsNullOrEmpty(didyoumeans))
					{
						command.ReturnMessage("Did you mean: " + didyoumeans + "?");
					}
				}
				else
				{
					var errorCode = xmd.GetElementsByTagName("error").Item(0).FirstChild;
					var errorMessage = errorCode.NextSibling;

					command.Reply("An error occurred: Error {0}: {1}", errorCode.InnerText, errorMessage.InnerText);
				}
				return;
			}
			if (queryresult.FirstChild.Name == "assumptions")
			{
				var options = queryresult.FirstChild.FirstChild.ChildNodes;
				var descriptions = new List<string>();
				for (var i = 0; i < options.Count; i++)
				{
					var node = options[i];
					descriptions.Add("\"" + node.Attributes["desc"].Value + "\"");
				}

				var first = string.Join(", ", descriptions.Take(descriptions.Count - 1));

				command.Reply("Ambiguity between {0} and {1}. Please try again.", first, descriptions.Last());
				return;
			}
			var input = queryresult.FirstChild;
			var title = ReplaceNewlines(input.Attributes["title"].Value);
			var result = ReplaceNewlines(input.NextSibling.InnerText);
			lastDisplayedResult = input.NextSibling;

			if (result == string.Empty)
			{
				result = ShowMore();
			}
			if (result.Length < 100)
			{
				result += " -- " + ShowMore();
			}

			command.Reply("({0}: {1}): {2}", title, ReplaceNewlines(input.InnerText), ReplaceNewlines(result));
		}

		private string GetDidYouMeans(XmlNodeList xmlNodeList)
		{
			var nodes = new List<XmlNode>(xmlNodeList.Cast<XmlNode>());
			if (nodes.Count == 0)
				return null;

			nodes.OrderByDescending(node => double.Parse(node.Attributes["score"].Value, CultureInfo.InvariantCulture));
			var didyoumeans = nodes.Select(node => string.Format("\"{0}\" (score: {1}%)", node.InnerText,
				Math.Round(double.Parse(node.Attributes["score"].Value, CultureInfo.InvariantCulture) * 100)
			));

			var firstItems = string.Join(", ", didyoumeans.Take(didyoumeans.Count() - 1));

			string result;
			if (didyoumeans.Count() > 1)
			{
				result = firstItems + " or " + didyoumeans.Last();
			}
			else
			{
				result = firstItems;
			}
			return result;
		}
	}
}
