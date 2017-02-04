using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.MessageFormatters
{
	class CurseMessageFormatter : IMessageFormatter
	{
		public void ProcessIncomingMessage(ChatMessage message)
		{

		}

		public string ProcessOutgoingMessage(string message)
		{
			return message.Replace(Frm.U, "_").Replace(Frm.I, "~").Replace(Frm.B, "*");
		}
	}
}
