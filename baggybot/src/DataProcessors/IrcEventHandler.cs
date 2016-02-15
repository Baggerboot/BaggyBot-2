using BaggyBot.Collections.Generic;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using BaggyBot.Monitoring;
using IRCSharp.IRC;
using IrcMessage = BaggyBot.MessagingInterface.IrcMessage;
using IrcUser = BaggyBot.MessagingInterface.IrcUser;

namespace BaggyBot.DataProcessors
{
	internal class IrcEventHandler
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

		public IrcEventHandler(CommandHandler commandHandler, StatsHandler statsHandler)
		{
			this.commandHandler = commandHandler;
			this.statsHandler = statsHandler;
		}



		// This is what we call a God Method. It's like a God Object, only it's a method.
		// It does way too fucking much, this really needs to be cleaned up sometime.
		// TODO: Clean up ProcessMessage()
		internal void ProcessMessage(IrcMessage message)
		{
			recentMessages.Enqueue(message.Message);

			// Display the message in the log
			if (message.Action)
			{
				Logger.Log(this, $"*{message.Sender.Nick} {message.Message}*", LogLevel.Message);
			}
			else
			{
				Logger.Log(this, message.Sender.Nick + ": " + message.Message, LogLevel.Message);
			}

			// TODO: The usefulness of this is rather doubtful. Probably it should just be removed.
			// For now, we'll simply disable it.
			/*// Perform simple substitution: s/word/replacement
			var rgx = new Regex(@"^s\/([^\/]{1,})\/([^\/]*)", RegexOptions.IgnoreCase);
			Match match;
			if ((match = rgx.Match(message.Message)).Success)
			{
				var needle = match.Groups[1].Value;
				var replacement = match.Groups[2].Value;

				var haystack = Enumerable.Reverse(recentMessages.ToArray()).First(msg => msg.Contains(needle) && msg != message.Message);

				ircInterface.SendMessage(message.Channel, message.Sender.Nick + ", " + haystack.Replace(needle, replacement));
				return;
			}*/

			// TODO: Process query console messages directly inside the commandhandler
			// It is obvious now that the CommandHandler class will have to parse all messages, and determine for itself whether it needs to act on them or not.
			// Handle query console messages
			/*if (ControlVariables.QueryConsole && message.Channel == ConfigManager.Config.Operators.First().Nick && !message.Message.StartsWith("-py"))
			{
				Logger.Log(this, "Processing Query Console python command");
				message.Message = "-py " + message.Message;
				commandHandler.ProcessMessage(message);
				return;
			}*/

			// Handle regular commands and messages
			if (message.Message.StartsWith(Bot.CommandIdentifier))
			{
				commandHandler.ProcessMessage(message);
			}
			else
			{
				statsHandler.ProcessMessage(message);
			}
		}

		internal void ProcessNotice(IrcUser sender, string notice)
		{
			Logger.Log(this, notice, LogLevel.Irc);
			sender.Client.StatsDatabase.AddIrcMessage(DateTime.Now, -1, CHANNEL_NOTICE, sender.Nick, notice);
		}

		/// <summary>
		/// Custom code for checking whether a user has registered with NickServ. Ugly, but it works.
		/// </summary>
		internal void ProcessFormattedLine(IrcLine line)
		{
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

		internal void HandleJoin(IrcUser user, string channel)
		{
			var message = $"{user} has joined {channel}";
			DisplayEvent(message, user, channel);
		}
		internal void HandlePart(IrcUser user, string channel)
		{
			var message = $"{user} has left {channel}";
			DisplayEvent(message, user, channel);
		}
		internal void HandleKick(string kickee, string channel, string reason, IrcUser kicker)
		{
			var message = $"{kickee} was kicked by {kicker.Nick} from {channel} ({reason})";
			DisplayEvent(message, kicker, channel);
		}
		internal void HandleNickChange(IrcUser user, string newNick)
		{
			var message = $"{user.Nick} is now known as {newNick}";
			user.Client.StatsDatabase.HandleNickChange(user, newNick);
			DisplayEvent(message, user, CHANNEL_NICK_CHANGE);
		}
		internal void DisplayEvent(string message, IrcUser sender, string channel)
		{
			Logger.Log(this, message, LogLevel.Irc);
			if (sender.Client.StatsDatabase.ConnectionState == ConnectionState.Open)
			{
				var uid = sender.Client.StatsDatabase.GetIdFromUser(sender);
				sender.Client.StatsDatabase.AddIrcMessage(DateTime.Now, uid, channel, "NOTICE", message);
			}
		}

		internal void ProcessRawLine(string line)
		{
			Logger.Log(this, "--" + line, LogLevel.Irc);
		}

		internal void HandleQuit(IrcUser user, string reason)
		{
			DisplayEvent(user + " has quit (" + reason + ")", user, CHANNEL_QUIT);
		}
	}
}
