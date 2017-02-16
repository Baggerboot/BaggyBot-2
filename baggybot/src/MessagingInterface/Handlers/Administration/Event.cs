using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.MessagingInterface.Handlers.Administration.Triggers;

namespace BaggyBot.MessagingInterface.Handlers.Administration
{
	public class Event
	{
		public string Name { get; set; }
		public bool Enabled { get; set; } = true;
		public Trigger[] Triggers { get; set; }
		public Action[][] Actions { get; set; }
		public ActionMessages Messages { get; set; }

		public UserEvent Create()
		{
			return new UserEvent
			{
				Name = Name,
				Enabled = Enabled,
				Triggers = Triggers.Select(t => t.Create()).ToArray(),
				Actions = Actions,
				Messages = Messages
			};
		}
	}

	public class UserEvent : Event
	{
		public new UserTrigger[] Triggers { get; set; }
		private int actionsPerformed = 0;

		public Action[] GetActions(MessageEvent ev)
		{
			var index = actionsPerformed >= Actions.Length ? Actions.Length - 1 : actionsPerformed;
			
			actionsPerformed++;
			return Actions[index];
		}

		public override string ToString() => $"{Name}: {Triggers.Length} triggers.";
	}
}
