using System;
using System.Data;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;

namespace BaggyBot.MessagingInterface.Handlers
{
	internal class LogHandler : ChatClientEventHandler
	{
		public override void HandleMessage(MessageEvent ev)
		{
			var message = ev.Message;
			// Display the message in the log
			if (message.Action)
			{
				Logger.Log(this, $"*{message.Sender.Nickname} {message.Body}*", LogLevel.Message);
			}
			else
			{
				Logger.Log(this, $"#{message.Channel.Name} {message.Sender.Nickname}: {message.Body}", LogLevel.Message);
			}
		}

		public override void HandleJoin(JoinEvent ev)
		{
			var message = $"{ev.User} has joined {ev.Channel}";
			DisplayEvent(message);
		}

		public override void HandlePart(PartEvent ev)
		{
			var message = $"{ev.User} has left {ev.Channel}";
			DisplayEvent(message);
		}

		public override void HandleKick(KickEvent ev)
		{
			var message = $"{ev.Kickee} was kicked by {ev.Kicker.Nickname} from {ev.Channel} ({ev.Reason})";
			DisplayEvent(message);
		}

		public override void HandleKicked(KickedEvent ev)
		{
			Logger.Log(this, $"I was kicked from {ev.Channel} by {ev.Kicker.Nickname} ({ev.Reason})", LogLevel.Warning);
		}

		public override void HandleNameChange(NameChangeEvent ev)
		{
			var message = $"{ev.OldName.Nickname} is now known as {ev.NewName.Nickname}";
			DisplayEvent(message);
		}
		internal void DisplayEvent(string message)
		{
			Logger.Log(this, message, LogLevel.Irc);
		}

		public override void HandleQuit(QuitEvent ev)
		{
			DisplayEvent(ev.User + " has quit (" + ev.Reason + ")");
		}
	}
}
