using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot.DataProcessors
{
	class IrcEventHandler
	{
		private DataFunctionSet dataFunctionSet;
		private CommandHandler commandHandler;
		private IrcInterface ircInterface;
		private StatsHandler statsHandler;

		public IrcEventHandler(DataFunctionSet dataFunctionSet, IrcInterface ircInterface, CommandHandler commandHandler, StatsHandler statsHandler)
		{
			this.dataFunctionSet = dataFunctionSet;
			this.ircInterface = ircInterface;
			this.commandHandler = commandHandler;
			this.statsHandler = statsHandler;
		}

		internal void ProcessMessage(IrcMessage message)
		{
			try {
				if (message.Action) {
					Logger.Log("*{0} {1}*", LogLevel.Message, true, message.Sender.Nick, message.Message);
				} else {
					Logger.Log(message.Sender.Nick + ": " + message.Message, LogLevel.Message);
				}
				

				int userId = dataFunctionSet.GetIdFromUser(message.Sender);

				if (message.Action) {
					Logger.Log("Adding action message to the IRC log");
					dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, "*" + message.Sender.Nick + " " + message.Message + "*");
				} else {
					Logger.Log("Adding regular IRC message to the IRC log");
					dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, message.Message);
				}

				if (ControlVariables.QueryConsole && message.Channel == Settings.Instance["operator_nick"] && !message.Message.StartsWith("-py")) {
					Logger.Log("Processing Query Console python command");
					message.Message = "-py " + message.Message;
					commandHandler.ProcessCommand(message);
					return;
				}
				if (message.Message.StartsWith(Bot.commandIdentifier)) {
					commandHandler.ProcessCommand(message);
				} else {
					statsHandler.ProcessMessage(message, userId);
				}
			} catch (ArgumentOutOfRangeException) {
				Logger.Log("\r\nArgumentOutOfRangeException occurred while attempting to process a message", LogLevel.Error);
				Logger.Log("The message contained the following bytes of data: {" + string.Join(", ", message.Message.ToCharArray().Select(c => string.Format("0x{0:X2}", (byte) c))) + "}", LogLevel.Error);
				Logger.Log("This message has been discarded.", LogLevel.Warning);
			}
		}
		internal void ProcessNotice(IrcUser sender, string notice)
		{
			if (ircInterface.HasNickservCall && sender.Ident.Equals("NickServ")) {
				Logger.Log("Received NickServ reply");
				if (notice.StartsWith("Information on")) {
					Logger.Log("User is registered. Processing reply");
					string data = notice.Substring("Information on  ".Length);
					string nick = data.Substring(0, data.IndexOf(" ") - 1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					string nickserv = data.Substring(0, data.Length - 3);
					ircInterface.AddNickserv(nick.ToLower(), nickserv);
				} else if (notice.EndsWith("is not registered.")) {
					string nick = notice.Substring(1, notice.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ') - 1);
					Logger.Log("'{0}' does not appear to be registered with NickServ.", LogLevel.Debug, true, nick);
					ircInterface.AddNickserv(nick.ToLower(), null);
				}
			} else if (sender.Ident.Equals("NickServ")) {
				Logger.Log("Received an unexpected NickServ response: " + notice, LogLevel.Warning);
			}
		}
		/// <summary>
		/// Custom code for checking whether a user has registered with NickServ. Ugly, but it works.
		/// </summary>
		/// <param name="line"></param>
		internal void ProcessFormattedLine(IrcLine line)
		{
			// This IRC server does not have a NickServ service.
			if (line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick")) {
				ircInterface.DisableNickservCalls();
				// Proess reply to WHOIS call.
			} else if (line.Command.Equals("311") && ircInterface.HasWhoisCall) {
				IrcUser user = new IrcUser(line.Arguments[1], line.Arguments[2], line.Arguments[3]);
				ircInterface.AddUser(user.Nick, user);
			}
			//
			if (!ignoredCommands.Contains(line.Command)) {
				switch (line.Command) {
					case "001":
					case "002":
					case "003":
						Logger.Log("{0}: {1}", LogLevel.Irc, true, line.Sender, line.FinalArgument);
						break;
					case "332":
						Logger.Log("Topic for {0}: {1}", LogLevel.Irc, true, line.Arguments[1], line.FinalArgument);
						break;
					case "333": // Ignore names list
					case "366":
						break;
					case "MODE":
						if (line.FinalArgument != null) {
							Logger.Log("{0} sets mode {1} for {2}", LogLevel.Irc, true, line.Sender, line.FinalArgument, line.Arguments[0]);
						} else {
							Logger.Log("{0} sets mode {1} for {2}", LogLevel.Irc, true, line.Sender, line.Arguments[1], line.Arguments[0]);
						}
						break;
					default:
						Logger.Log(line.ToString(), LogLevel.Irc);
						break;
				}
			}
		}
		internal readonly string[] ignoredCommands = { "004" /*RPL_MYINFO*/, "005" /*RPL_ISUPPORT*/, "251" /*RPL_LUSERCLIENT*/, "254" /*RPL_LUSERCHANNELS*/, "252" /*RPL_LUSEROP*/, "255" /*RPL_LUSERME*/, "265", "266", "250", "375", "376" };

		internal void HandleJoin(IrcUser user, string channel)
		{
			string message = user.ToString() + " has joined " + channel;
			DisplayEvent(message);
		}
		internal void HandlePart(IrcUser user, string channel)
		{
			string message = user.ToString() + " has left " + channel;
			DisplayEvent(message);
		}
		internal void HandleKick(string user, string channel, string reason, IrcUser sender)
		{
			string message = user + " was kicked by " + sender.Nick + " from " + channel + " (" + reason + ")";
			DisplayEvent(message);
		}
		internal void HandleNickChange(IrcUser user, string newNick)
		{
			string message = user.Nick + " is now known as " + newNick;
			DisplayEvent(message);
		}
		internal void DisplayEvent(string message)
		{
			Logger.Log(message, LogLevel.Irc);
			dataFunctionSet.AddIrcMessage(DateTime.Now, null, "ALL", "NOTICE", message);
		}

		internal void ProcessRawLine(string line)
		{
			Logger.Log("--" + line, LogLevel.Irc);
		}

		internal void HandleQuit(IrcUser user, string reason)
		{
			DisplayEvent(user.ToString() + " has quit (" + reason + ")"); 
		}
	}
}
