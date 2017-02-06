using System;
using System.Data;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;

namespace BaggyBot.DataProcessors
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

		internal void ProcessMessage(ChatMessage message)
		{
			// Display the message in the log
			if (message.Action)
			{
				Logger.Log(this, $"*{message.Sender.Nickname} {message.Message}*", LogLevel.Message);
			}
			else
			{
				Logger.Log(this, message.Sender.Nickname + ": " + message.Message, LogLevel.Message);
			}
			
			// TODO: Process query console messages directly inside the commandhandler
		}

		internal void ProcessNotice(ChatUser sender, string notice)
		{
			Logger.Log(this, notice, LogLevel.Irc);
			sender.Client.StatsDatabase.AddIrcMessage(DateTime.Now, -1, CHANNEL_NOTICE, sender.Nickname, notice);
		}

		public override void HandleJoin(ChatUser user, ChatChannel channel)
		{
			var message = $"{user} has joined {channel}";
			DisplayEvent(message, user, channel);
		}

		public override void HandlePart(ChatUser user, ChatChannel channel)
		{
			var message = $"{user} has left {channel}";
			DisplayEvent(message, user, channel);
		}

		public override void HandleKick(ChatUser kickee, ChatChannel channel, string reason, ChatUser kicker)
		{
			var message = $"{kickee} was kicked by {kicker.Nickname} from {channel} ({reason})";
			DisplayEvent(message, kicker, channel);
		}

		public override void HandleNickChange(ChatUser user, ChatUser newNick)
		{
			var message = $"{user.Nickname} is now known as {newNick.Nickname}";
			user.Client.StatsDatabase.UpdateUser(newNick);
			DisplayEvent(message, user, new ChatChannel(CHANNEL_NICK_CHANGE));
		}
		internal void DisplayEvent(string message, ChatUser sender, ChatChannel channel)
		{
			Logger.Log(this, message, LogLevel.Irc);
			if (sender.Client.StatsDatabase.ConnectionState == ConnectionState.Open)
			{
				var uid = sender.Client.StatsDatabase.MapUser(sender).Id;
				sender.Client.StatsDatabase.AddIrcMessage(DateTime.Now, uid, channel.Identifier, "NOTICE", message);
			}
		}

		public override void HandleQuit(ChatUser user, string reason)
		{
			DisplayEvent(user + " has quit (" + reason + ")", user, new ChatChannel(CHANNEL_QUIT));
		}
	}
}
