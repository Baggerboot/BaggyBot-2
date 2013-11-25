using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	class IrcEventHandler
	{
		private DataFunctionSet dataFunctionSet;
		private IrcInterface ircInterface;
		private CommandHandler commandHandler;
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
			Logger.Log(message.Sender.Nick + ": " + message.Message, LogLevel.Message);

			int userId;
			lock (dataFunctionSet.Lock) {
				userId = dataFunctionSet.GetIdFromUser(message.Sender);
			}
			if (message.Action) {
				dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, "*" + message.Sender.Nick + " " + message.Message + "*");
			} else {
				dataFunctionSet.AddIrcMessage(DateTime.Now, userId, message.Channel, message.Sender.Nick, message.Message);
			}

			if (message.Message.StartsWith(Program.commandIdentifier)) {
				commandHandler.ProcessCommand(message);
			} else {
				statsHandler.ProcessMessage(message);
			}
		}
		internal void ProcessNotice(IrcUser sender, string notice)
		{
			if (ircInterface.HasNickservCall && sender.Ident.Equals("NickServ")) {
				if (notice.StartsWith("Information on")) {
					string data = notice.Substring("Information on  ".Length);
					string nick = data.Substring(0, data.IndexOf(" ") - 1);
					data = data.Substring(nick.Length + 2 + "(account  ".Length);
					string nickserv = data.Substring(0, data.Length - 3);
					ircInterface.AddNickserv(nick.ToLower(), nickserv);
				} else if (notice.EndsWith("is not registered.")) {
					string nick = notice.Substring(1, notice.Length - 2);
					nick = nick.Substring(0, nick.IndexOf(' ') - 1);
					ircInterface.AddNickserv(nick, null);
				}
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
				Logger.Log(line.ToString(), LogLevel.Irc);
			}
		}
		internal readonly string[] ignoredCommands = { "004", "005", "251", "254", "252", "255", "265", "266", "250", "375", "376" };

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
		internal void HandleKick(string user, string channel)
		{
			string message = user + " was kicked from " + channel;
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
	}
}
