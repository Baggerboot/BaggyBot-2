using System;
using BaggyBot.Database;
using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers
{
	public abstract class ChatClientEventHandler : IDisposable
	{
		internal ChatClient Client { get; private set; }
		internal StatsDatabaseManager StatsDatabase => Client.StatsDatabase;

		internal void BindClient(ChatClient chatClient)
		{
			if(Client != null) throw new InvalidOperationException("Client has already been bound.");
			Client = chatClient;
		}

		public virtual void HandleMessage(MessageEvent ev)
		{
		}

		public virtual void HandleNameChange(NameChangeEvent ev)
		{
			
		}

		public virtual void HandleJoin(JoinEvent ev)
		{
			
		}

		public virtual void HandlePart(PartEvent ev)
		{
			
		}

		public virtual void HandleKick(KickEvent ev)
		{
			
		}

		public virtual void HandleKicked(KickedEvent ev)
		{
			
		}

		public virtual void HandleQuit(QuitEvent ev)
		{
			
		}

		public virtual void Initialise()
		{
			
		}

		public virtual void Dispose()
		{
			
		}

		public virtual void HandleConnectionEstablished()
		{

		}
	}
}
