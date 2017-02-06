using BaggyBot.MessagingInterface;

namespace BaggyBot.DataProcessors
{
	public abstract class ChatClientEventHandler
	{
		/// <summary>
		/// Handle an incoming message.
		/// </summary>
		/// <param name="message">The message that has been received.</param>
		/// <returns>True if the message has been consumed, and should not be processed by any other handlers.
		/// False if it should be processed by other handlers as well.</returns>
		public virtual bool HandleMessage(ChatMessage message)
		{
			return false;
		}

		public virtual void HandleNickChange(ChatUser oldName, ChatUser newName)
		{
			
		}

		public virtual void HandleJoin(ChatUser user, ChatChannel channel)
		{
			
		}

		public virtual void HandlePart(ChatUser user, ChatChannel channel)
		{
			
		}

		public virtual void HandleKick(ChatUser kickee, ChatChannel channel, string reason, ChatUser kicker)
		{
			
		}

		public virtual void HandleQuit(ChatUser user, string reason)
		{
			
		}
	}
}
