using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins
{
	public interface IMessageFormatter
	{
		void ProcessIncomingMessage(ChatMessage message);
		string ProcessOutgoingMessage(string message);
	}
}
