using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public interface ITriggerable
	{
		bool Check(MessageEvent ev);
		void Initialise();
	}
}