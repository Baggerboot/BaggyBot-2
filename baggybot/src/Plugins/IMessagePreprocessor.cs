using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins
{
	/// <summary>
	/// A message preprocessor transforms messages when they leave and/or enter the plugin.
	/// </summary>
	public interface IMessagePreprocessor
	{
		ChatMessage ProcessIncomingMessage(ChatMessage message);
		string ProcessOutgoingMessage(string message);
	}
}
