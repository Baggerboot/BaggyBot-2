using System.Data;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.MessagingInterface.Handlers;

namespace BaggyBot.MessagingInterface.Handlers
{
	internal class BindHandler : ChatClientEventHandler
	{
		private ChatUser Bind(ChatUser user)
		{
			if (StatsDatabase.ConnectionState != ConnectionState.Open) return user;

			var dbUser = StatsDatabase.MapUser(user);
			user.BindDbUser(dbUser);
			return user;
		}

		private ChatMessage Bind(ChatMessage message)
		{
			Bind(message.Sender);
			return message;
		}


		public override void HandleMessage(MessageEvent ev)
		{
			Bind(ev.Message);
		}

		public override void HandleNameChange(NameChangeEvent ev)
		{
			Bind(ev.NewName);
			Bind(ev.OldName);
		}

		public override void HandleJoin(JoinEvent ev)
		{
			Bind(ev.User);
		}

		public override void HandlePart(PartEvent ev)
		{
			Bind(ev.User);
		}

		public override void HandleKick(KickEvent ev)
		{
			Bind(ev.Kickee);
			Bind(ev.Kicker);
		}

		public override void HandleKicked(KickedEvent ev)
		{
			Bind(ev.Kicker);
		}

		public override void HandleQuit(QuitEvent ev)
		{
			Bind(ev.User);
		}
	}
}