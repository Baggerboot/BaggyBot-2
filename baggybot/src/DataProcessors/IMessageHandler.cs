using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;

namespace BaggyBot.src.DataProcessors
{
	internal interface IMessageHandler
	{
		/// <summary>
		/// Handle an incoming message.
		/// </summary>
		/// <param name="message">The message that has been received.</param>
		/// <returns>True if the message has been consumed, and should not be processed by any other handlers.
		/// False if it should be processed by other handlers as well.</returns>
		bool HandleMessage(IrcMessage message);
	}
}
