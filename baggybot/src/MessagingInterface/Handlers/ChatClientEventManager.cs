using System.Collections.Generic;
using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers
{
	internal class ChatClientEventManager
	{
		private readonly List<ChatClientEventHandler> internalHandlers;

		public ChatClientEventManager(List<ChatClientEventHandler> internalHandlers)
		{
			this.internalHandlers = internalHandlers;
		}

		public void HandleNameChange(NameChangeEvent nameChangeEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleNameChange(nameChangeEvent);
			}
		}

		public void HandleMessage(MessageEvent messageEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleMessage(messageEvent);
			}
		}

		public void HandleJoin(JoinEvent joinEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleJoin(joinEvent);
			}
		}

		public void HandlePart(PartEvent partEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandlePart(partEvent);
			}
		}

		public void HandleKick(KickEvent kickEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleKick(kickEvent);
			}
		}

		public void HandleKicked(KickedEvent kickedEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleKicked(kickedEvent);
			}
		}

		public void HandleQuit(QuitEvent quitEvent)
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleQuit(quitEvent);
			}
		}

		public void HandleConnectionEstablished()
		{
			foreach (var handler in internalHandlers)
			{
				handler.HandleConnectionEstablished();
			}
		}
	}
}
