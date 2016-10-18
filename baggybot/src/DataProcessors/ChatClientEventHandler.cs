using BaggyBot.Collections.Generic;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using BaggyBot.DataProcessors.Mapping;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using IRCSharp.IRC;

namespace BaggyBot.DataProcessors
{
	internal class ChatClientEventHandler
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
		// Handles parsing and executing commands
		private readonly CommandHandler commandHandler;
		// Handles the generation of statistics from incoming messages
		private readonly StatsHandler statsHandler;
		// Holds the most recent 15 messages.
		private readonly FixedSizeConcurrentQueue<string> recentMessages = new FixedSizeConcurrentQueue<string>(15);
		// These IRC commands are not handled in any way, as the information contained in them
		// is not considered useful for the bot.
		private readonly string[] ignoredCommands =
		{
			"004" /*RPL_MYINFO*/,
			"005" /*RPL_ISUPPORT*/,
			"251" /*RPL_LUSERCLIENT*/,
			"254" /*RPL_LUSERCHANNELS*/,
			"252" /*RPL_LUSEROP*/,
			"255" /*RPL_LUSERME*/,
			"265" /*RPL_LOCALUSERS*/,
			"266" /*RPL_GLOBALUSERS*/,
			"250" /*RPL_STATSCONN*/
		};

		public ChatClientEventHandler(CommandHandler commandHandler, StatsHandler statsHandler)
		{
			this.commandHandler = commandHandler;
			this.statsHandler = statsHandler;
		}
		
		internal void ProcessMessage(ChatMessage message)
		{
			recentMessages.Enqueue(message.Message);

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

			// Either process the message as a command, or as a message for which statistics should be generated.
			// In other words, commands do not end up in the stats DB.
			if(Bot.CommandIdentifiers.Any(identifier => message.Message.StartsWith(identifier)))
			{
				commandHandler.ProcessMessage(message);
			}
			else
			{
				statsHandler.ProcessMessage(message);
			}
		}

		internal void ProcessNotice(ChatUser sender, string notice)
		{
			Logger.Log(this, notice, LogLevel.Irc);
			sender.Client.StatsDatabase.AddIrcMessage(DateTime.Now, -1, CHANNEL_NOTICE, sender.Nickname, notice);
		}

		/// <summary>
		/// Custom code for checking whether a user has registered with NickServ. Ugly, but it works.
		/// </summary>
		internal void ProcessFormattedLine(IrcLine line)
		{
			//TODO: where's the nickserv code gone?
			// This IRC server does not have a NickServ service.
			/*if (line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick"))
			{
				ircInterface.DisableNickservCalls();
				// Proess reply to WHOIS call.
			}*/
			
			/*else if (line.Command.Equals("464"))
			{
				//throw new NotImplementedException("Unable to supply the password from the IRC Event Handler.");
				Logger.Log(this, "Password required by server.", LogLevel.Info);
				var msg = "PASS " + ircInterface.Password;
				Logger.Log(this, "Replying with " + msg, LogLevel.Info);
				ircInterface.SendRaw(msg);
			}*/
			if (!ignoredCommands.Contains(line.Command))
			{
				switch (line.Command)
				{
					case "001": // RPL_WELCOME
					case "002": // RPL_YOURHOST
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "003": // RPL_CREATED
						Logger.Log(this, $"{line.Sender}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "332": // RPL_TOPIC
						Logger.Log(this, $"Topic for {line.Arguments[1]}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "333": // Ignore names list
					case "366":
						break;
					case "253": // RPL_LUSERUNKNOWN
						break;
					case "375": // RPL_MOTDSTART
					case "376": // RPL_ENDOFMOTD
					case "372": // RPL_MOTD
					case "451": // ERR_NOTREGISTERED
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "MODE":
						// TODO: Figure out the difference between these two and document it. Probably channel/user
						if (line.FinalArgument != null)
						{
							Logger.Log(this, $"{line.Sender} sets mode {line.FinalArgument} for {line.Arguments[0]}", LogLevel.Irc);
						}
						else
						{
							Logger.Log(this, $"{line.Sender} sets mode {line.Arguments[1]} for {line.Arguments[0]}");
						}
						break;
					default:
						Debugger.Break();
						Logger.Log(this, line.ToString(), LogLevel.Irc);
						break;
				}
			}
		}

		internal void HandleJoin(ChatUser user, ChatChannel channel)
		{
			var message = $"{user} has joined {channel}";
			DisplayEvent(message, user, channel);
		}
		internal void HandlePart(ChatUser user, ChatChannel channel)
		{
			var message = $"{user} has left {channel}";
			DisplayEvent(message, user, channel);
		}
		internal void HandleKick(ChatUser kickee, ChatChannel channel, string reason, ChatUser kicker)
		{
			var message = $"{kickee} was kicked by {kicker.Nickname} from {channel} ({reason})";
			DisplayEvent(message, kicker, channel);
		}
		internal void HandleNickChange(ChatUser user, ChatUser newNick)
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

		internal void ProcessRawLine(string line)
		{
			Logger.Log(this, "--" + line, LogLevel.Irc);
		}

		internal void HandleQuit(ChatUser user, string reason)
		{
			DisplayEvent(user + " has quit (" + reason + ")", user, new ChatChannel(CHANNEL_QUIT));
		}
	}
}
