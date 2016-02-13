using BaggyBot.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using BaggyBot.Monitoring;

namespace BaggyBot.Commands
{
	internal class WolframAlpha : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<query>|<more>";

		public override string Description
			=>
				"Makes Wolfram Alpha calculate or look up the query you've entered, and returns the result. Using the 'more' argument will print additional information about the last query you looked up."
			;

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
				lastDisplayedResult = result;
				return ReadPod(result);
			}
			return returnData;
		}

		private static string ReadPod(XmlNode node)
		{
			var children = node.SelectNodes("subpod/plaintext");
			if (children.Count > 1)
			{
				string data = string.Empty;
				foreach (var childNode in children)
				{
					if (childNode is XmlNode)
					{
						data += " \x001dand\x001d " + ((XmlNode) childNode).InnerText;
					}
					else
					{
						Debugger.Break();
					}
				}
				data = data.Substring(" \x0002and\x0002 ".Length);
				return "\x02" + node.Attributes["title"].Value + "\x02: " + data;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(node.InnerText))
				{
					return null;
				}
				else
				{
					return "\x02" + node.Attributes["title"].Value + "\x02: " + node.InnerText;
				}
			}
		}

		public string ReplaceNewlines(string input)
		{
			return input.Replace("\n", " -- ");
		}

		public override void Use(CommandArgs command)
		{
			if (string.IsNullOrWhiteSpace(command.FullArgument))
			{
				command.Reply(
					"Usage: '-wa <WolframAlpha query>' -- Displays information acquired from http://www.wolframalpha.com. In addition to this, you can use the command '-wa more' to display additional information about the last subject");
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
					command.ReturnMessage(ReplaceNewlines(WaReplace(more)));
				}
				return;
			}

			lastDisplayedResult = null;

			var appid = ConfigManager.Config.Integrations.WolframAlpha.AppId;

			var uri =
				$"http://api.wolframalpha.com/v2/query?appid={appid}&input={Uri.EscapeDataString(command.FullArgument)}&ip={command.Sender.Hostmask}&format=plaintext&units=metric";
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
			var result = ReadPod(input.NextSibling);
			lastDisplayedResult = input.NextSibling;

			if (result == null)
			{
				result = ShowMore();
			}
			if (result.Length < 100)
			{
				result += " -- " + ShowMore();
			}

			command.Reply("({0}: {1}): {2}", WaReplace(title), ReplaceNewlines(WaReplace(input.InnerText)), ReplaceNewlines(WaReplace(result)));
		}

		private static string WaReplace(string text)
		{
			text = text.Replace("\uf7d9", "==");
			text = text.Replace("\uf6c4", "\x1ds\x1d");
			text = text.Replace("\uf522", "->");

			foreach (char c in text)
			{
				if ((int) c >= 0xE000 && (int) c <= 0xF8FF)
				{
					Logger.Log(null, $"Unrecognised character detected: Code point 0x{(int)c:x4}", LogLevel.Warning);
				}
			}

			return text;
		}

		private string GetDidYouMeans(XmlNodeList xmlNodeList)
		{
			var nodes = new List<XmlNode>(xmlNodeList.Cast<XmlNode>());
			if (nodes.Count == 0)
				return null;

			nodes.OrderByDescending(node => double.Parse(node.Attributes["score"].Value, CultureInfo.InvariantCulture));
			var didyoumeans = nodes.Select(node =>
				$"\"{node.InnerText}\" (score: {Math.Round(double.Parse(node.Attributes["score"].Value, CultureInfo.InvariantCulture) * 100)}%)");

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
