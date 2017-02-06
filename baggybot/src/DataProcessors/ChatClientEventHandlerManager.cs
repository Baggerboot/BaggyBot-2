using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;

namespace BaggyBot.DataProcessors
{
	class ChatClientEventHandlerManager
	{
		public void HandleNickChange(ChatUser oldName, ChatUser newName)
		{
			
		}

		public void HandleMessage(ChatMessage message)
		{
			
		}

		public void HandleJoin(ChatUser user, ChatChannel channel)
		{
			
		}

		public void HandlePart(ChatUser user, ChatChannel channel)
		{
			
		}

		public void HandleKick(ChatUser kickee, ChatChannel channel, string reason, ChatUser kicker)
		{
			
		}

		public void HandleQuit(ChatUser user, string reason)
		{
			
		}
	}
}
