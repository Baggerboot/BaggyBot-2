using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.MessageFormatters
{
	class MarkdownMessageFormatter : IMessageFormatter
	{
		public void ProcessIncomingMessage(ChatMessage message)
		{

		}

		public string ProcessOutgoingMessage(string message)
		{
			return message.Replace(Frm.I, "_").Replace(Frm.B, "*");
		}
	}
}
