using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.MessageFormatters
{
	public interface IMessageFormatter
	{
		void ProcessIncomingMessage(ChatMessage message);
		string ProcessOutgoingMessage(string message);
	}
}
