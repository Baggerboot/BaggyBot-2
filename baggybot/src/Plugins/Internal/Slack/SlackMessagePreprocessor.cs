using System.Text.RegularExpressions;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.Internal.Slack
{
	class SlackMessagePreprocessor : IMessagePreprocessor
	{

		public void ProcessIncomingMessage(ChatMessage message)
		{
			message.Message = FixUrls(message.Message);
			message.Message = message.Message.Replace("&lt;", "<").Replace("&gt;", ">");
		}

		private string FixUrls(string message)
		{
			//turn `<http://geluk.io|geluk.io>` into `geluk.io`
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
