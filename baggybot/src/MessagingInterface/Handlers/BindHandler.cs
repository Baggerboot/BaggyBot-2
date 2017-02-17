using System.Data;
using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers
{
	/// <summary>
	/// Binds database User objects to ChatUser objects.
	/// Binding will not occur if there is no database connection.
	/// </summary>
	internal class BindHandler : ChatClientEventHandler
	{
		private void Bind(ChatUser user)
		{
			if (StatsDatabase.ConnectionState != ConnectionState.Open) return;

			var dbUser = StatsDatabase.UpsertUser(user);
			user.BindDbUser(dbUser);
		}

		private void Bind(ChatMessage message)
		{
			Bind(message.Sender);
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