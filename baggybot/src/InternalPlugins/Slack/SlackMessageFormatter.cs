using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

namespace BaggyBot.InternalPlugins.Slack
{
	class SlackMessageFormatter : IMessageFormatter
	{

		public void ProcessIncomingMessage(ChatMessage message)
		{
			message.Message = FixUrls(message.Message);
			message.Message = message.Message.Replace("&lt;", "<").Replace("&gt;", ">");
		}

		private string FixUrls(string message)
		{
			//-resolve <http://geluk.io|geluk.io>
			var newMessage = message;
			var rgx = new Regex(@"\<http://.*\|(.*)\>");
			foreach (Match match in rgx.Matches(message))
			{
				var domain = match.Groups[1].Value;
				newMessage = rgx.Replace(newMessage, domain);
			}
			return newMessage;
		}

		public string ProcessOutgoingMessage(string message)
		{
			return message;
		}
	}
}
