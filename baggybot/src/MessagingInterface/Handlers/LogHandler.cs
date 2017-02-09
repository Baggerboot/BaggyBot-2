using System;
using System.Data;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;

namespace BaggyBot.MessagingInterface.Handlers
{
	internal class LogHandler : ChatClientEventHandler
	{
		// Not all logged events can be accurately represented as messages that were sent to channels.
		// In such cases, the channel name will instead be set to one of the following values.
		// Events originating from users will be prefixed with USER_
		// Events originating from the server will be prefixed with SERVER_
		// All non-channel events will always start with @
		private const string CHANNEL_MISC = "@MISC";
		private const string CHANNEL_NOTICE = "@SERVER_NOTICE";
		private const string CHANNEL_NICK_CHANGE = "@USER_NICK_CHANGE";
		private const string CHANNEL_QUIT = "@USER_QUIT";

		public override void HandleMessage(MessageEvent ev)
		{
			var message = ev.Message;
			// Display the message in the log
			if (message.Action)
			{
				Logger.Log(this, $"*{message.Sender.Nickname} {message.Message}*", LogLevel.Message);
			}
			else
			{
				Logger.Log(this, $"#{message.Channel.Name} {message.Sender.Nickname}: {message.Message}", LogLevel.Message);
			}
			
			// TODO: Process query console messages directly inside the commandhandler
		}

		internal void ProcessNotice(ChatUser sender, string notice)
		{
			Logger.Log(this, notice, LogLevel.Irc);
			Client.StatsDatabase.AddIrcMessage(DateTime.Now, -1, CHANNEL_NOTICE, sender.Nickname, notice);
		}

		public override void HandleJoin(JoinEvent ev)
		{
			var message = $"{ev.User} has joined {ev.Channel}";
			DisplayEvent(message, ev.User, ev.Channel);
		}

		public override void HandlePart(PartEvent ev)
		{
			var message = $"{ev.User} has left {ev.Channel}";
			DisplayEvent(message, ev.User, ev.Channel);
		}

		public override void HandleKick(KickEvent ev)
		{
			var message = $"{ev.Kickee} was kicked by {ev.Kicker.Nickname} from {ev.Channel} ({ev.Reason})";
			DisplayEvent(message, ev.Kicker, ev.Channel);
		}

		public override void HandleKicked(KickedEvent ev)
		{
			Logger.Log(this, $"I was kicked from {ev.Channel} by {ev.Kicker.Nickname} ({ev.Reason})", LogLevel.Warning);
		}

		public override void HandleNameChange(NameChangeEvent ev)
		{
			var message = $"{ev.OldName.Nickname} is now known as {ev.NewName.Nickname}";
			Client.StatsDatabase.UpdateUser(ev.NewName);
			DisplayEvent(message, ev.NewName, new ChatChannel(CHANNEL_NICK_CHANGE));
		}
		internal void DisplayEvent(string message, ChatUser sender, ChatChannel channel)
		{
			Logger.Log(this, message, LogLevel.Irc);
			if (Client.StatsDatabase.ConnectionState == ConnectionState.Open)
			{
				var uid = Client.StatsDatabase.MapUser(sender).Id;
				Client.StatsDatabase.AddIrcMessage(DateTime.Now, uid, channel.Identifier, "NOTICE", message);
			}
		}

		public override void HandleQuit(QuitEvent ev)
		{
			DisplayEvent(ev.User + " has quit (" + ev.Reason + ")", ev.User, new ChatChannel(CHANNEL_QUIT));
		}
	}
}
