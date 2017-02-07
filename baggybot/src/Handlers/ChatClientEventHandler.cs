using BaggyBot.Database;
using BaggyBot.Handlers.ChatClientEvents;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Handlers
{
	public abstract class ChatClientEventHandler
	{
		internal ChatClient Client { get; set; }
		internal StatsDatabaseManager StatsDatabase => Client.StatsDatabase;
		

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
	}
}
