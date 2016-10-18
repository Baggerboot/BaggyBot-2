using BaggyBot.MessagingInterface;

namespace BaggyBot.DataProcessors
{
	internal interface IMessageHandler
	{
		/// <summary>
		/// Handle an incoming message.
		/// </summary>
		/// <param name="message">The message that has been received.</param>
		/// <returns>True if the message has been consumed, and should not be processed by any other handlers.
		/// False if it should be processed by other handlers as well.</returns>
		bool HandleMessage(ChatMessage message);
	}
}
