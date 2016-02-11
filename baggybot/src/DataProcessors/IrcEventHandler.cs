using BaggyBot.Collections.Generic;
using BaggyBot.Configuration;
using IRCSharp.IRC;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using BaggyBot.Database;
using BaggyBot.Monitoring;
using Microsoft.Scripting.Utils;

namespace BaggyBot.DataProcessors
{
	internal class IrcEventHandler
	{
		private readonly DataFunctionSet dataFunctionSet;
		private readonly CommandHandler commandHandler;
		private readonly IrcInterface ircInterface;
		private readonly StatsHandler statsHandler;
		private FixedSizeQueue<string> recentMessages = new FixedSizeQueue<string>(15);

		private readonly string[] IgnoredCommands = { "004" /*RPL_MYINFO*/, "005" /*RPL_ISUPPORT*/, "251" /*RPL_LUSERCLIENT*/, "254" /*RPL_LUSERCHANNELS*/, "252" /*RPL_LUSEROP*/, "255" /*RPL_LUSERME*/, "265", "266", "250" };

		public IrcEventHandler(DataFunctionSet dataFunctionSet, IrcInterface ircInterface, Bot bot)
		{
			this.dataFunctionSet = dataFunctionSet;
			this.ircInterface = ircInterface;
			this.commandHandler = new CommandHandler(this.ircInterface, this.dataFunctionSet, bot);
			this.statsHandler = new StatsHandler(this.dataFunctionSet, this.ircInterface);
		}

		private void AddMessageToIrcLog(IrcMessage message, int userId)
		{
			if (message.Action)
			{
				dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, "*" + message.Sender.Nick + " " + message.Message + "*");
			}
			else
			{
				dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, message.Message);
			}
		}

		// This is what we call a God Method. It's like a God Object, only it's a method.
		// It does way too fucking much, this really needs to be cleaned up sometime.
		// TODO: Clean up ProcessMessage()
		private void ProcessMessage(IrcMessage message)
		{
			try
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

				// Add the message to the IRC log
				var userId = 0;
				if (dataFunctionSet.ConnectionState != ConnectionState.Closed)
				{
					userId = dataFunctionSet.GetIdFromUser(message.Sender);
					AddMessageToIrcLog(message, userId);
				}

				// Perform simple substitution
				var rgx = new Regex(@"^s\/([^\/]{1,})\/([^\/]*)", RegexOptions.IgnoreCase);
				Match match;
				if ((match = rgx.Match(message.Message)).Success)
				{
					var needle = match.Groups[1].Value;
					var replacement = match.Groups[2].Value;

					var haystack = Enumerable.Reverse(recentMessages.ToArray()).First(msg => msg.Contains(needle) && msg != message.Message);

					ircInterface.SendMessage(message.Channel, message.Sender.Nick + ", " + haystack.Replace(needle, replacement));
					return;
				}

				// Handle query console messages
				// TODO: Allow validation of multiple operators
				if (ControlVariables.QueryConsole && message.Channel == ConfigManager.Config.Operators.First().Nick && !message.Message.StartsWith("-py"))
				{
					Logger.Log(this, "Processing Query Console python command");
					message.Message = "-py " + message.Message;
					commandHandler.ProcessMessage(message);
					return;
				}

				// Handle regular commands and messages
				if (message.Message.StartsWith(Bot.CommandIdentifier))
				{
					commandHandler.ProcessMessage(message);
				}
				else
				{
					statsHandler.ProcessMessage(message, userId);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				Logger.Log(this, "\r\nArgumentOutOfRangeException occurred while attempting to process a message", LogLevel.Error);
				Logger.Log(this, "The message contained the following bytes of data: {"
					+ string.Join(", ", message.Message.ToCharArray()
						.Select(c => $"0x{(byte)c:X2}")) + "}", LogLevel.Error);
				Logger.Log(this, "This message has been discarded.", LogLevel.Warning);
			}
		}
		internal void ProcessNotice(IrcUser sender, string notice)
		{
			if (ircInterface.HasNickservCall && sender.Ident.Equals("NickServ"))
			{
				Logger.Log(this, "Received NickServ reply");
				if (notice.StartsWith("Information on"))
				{
					Logger.Log(this, "User is registered. Processing reply");
					var data = notice.Substring("Information on  ".Length);
					var nick = data.Substring(0, data.IndexOf(" ") - 1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					var nickserv = data.Substring(0, data.Length - 3);
					ircInterface.AddNickserv(nick.ToLower(), nickserv);
				}
				else if (notice.EndsWith("is not registered."))
				{
					var nick = notice.Substring(1, notice.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ') - 1);
					Logger.Log(this, $"The username \"{nick}\" does not appear to be registered with NickServ.");
					ircInterface.AddNickserv(nick.ToLower(), null);
				}
			}
			else if (sender.Ident.Equals("NickServ"))
			{
				Logger.Log(this, "Received an unexpected NickServ response: " + notice, LogLevel.Warning);
			}
		}

		/// <summary>
		/// Custom code for checking whether a user has registered with NickServ. Ugly, but it works.
		/// </summary>
		internal void ProcessFormattedLine(IrcLine line)
		{
			// This IRC server does not have a NickServ service.
			if (line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick"))
			{
				ircInterface.DisableNickservCalls();
				// Proess reply to WHOIS call.
			}
			else if (line.Command.Equals("311") && ircInterface.HasWhoisCall)
			{
				var user = new IrcUser(line.Arguments[1], line.Arguments[2], line.Arguments[3]);
				ircInterface.AddUser(user.Nick, user);
			}
			else if (line.Command.Equals("464"))
			{
				//throw new NotImplementedException("Unable to supply the password from the IRC Event Handler.");
				Logger.Log(this, "Password required by server.", LogLevel.Info);
				var msg = "PASS " + ircInterface.Password;
				Logger.Log(this, "Replying with " + msg, LogLevel.Info);
				ircInterface.SendRaw(msg);
			}
			if (!IgnoredCommands.Contains(line.Command))
			{
				switch (line.Command)
				{
					case "001":
					case "002":
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "003":
						Logger.Log(this, $"{line.Sender}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "332":
						Logger.Log(this, $"Topic for {line.Arguments[1]}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "333": // Ignore names list
					case "366":
						break;
					case "375": // RPL_MOTDSTART
					case "376": // RPL_ENDOFMOTD
					case "372": // RPL_MOTD
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "MODE":
						// TODO: Figure out the difference between these two and document it.
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
						Logger.Log(this, line.ToString(), LogLevel.Irc);
						break;
				}
			}
		}

		internal void HandleJoin(IrcUser user, string channel)
		{
			var message = user + " has joined " + channel;
			DisplayEvent(message, user, channel);
		}
		internal void HandlePart(IrcUser user, string channel)
		{
			var message = user + " has left " + channel;
			DisplayEvent(message, user, channel);
		}
		internal void HandleKick(string user, string channel, string reason, IrcUser sender)
		{
			var message = user + " was kicked by " + sender.Nick + " from " + channel + " (" + reason + ")";
			DisplayEvent(message, sender, channel);
		}
		internal void HandleNickChange(IrcUser user, string newNick)
		{
			var message = user.Nick + " is now known as " + newNick;
			DisplayEvent(message, user);
		}
		internal void DisplayEvent(string message, IrcUser sender, string channel = "ALL")
		{
			Logger.Log(this, message, LogLevel.Irc);
			var uid = dataFunctionSet.GetIdFromUser(sender);
			dataFunctionSet.AddIrcMessage(DateTime.Now, uid, channel, "NOTICE", message);
		}

		internal void ProcessRawLine(string line)
		{
			Logger.Log(this, "--" + line, LogLevel.Irc);
		}

		internal void HandleQuit(IrcUser user, string reason)
		{
			DisplayEvent(user + " has quit (" + reason + ")", user);
		}
	}
}
