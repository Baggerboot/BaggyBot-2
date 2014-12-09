using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using IRCSharp;
using BaggyBot.DataProcessors;
using IRCSharp.IRC;

namespace BaggyBot
{
	public class IrcInterface
	{
		private readonly Dictionary<string, string> nickservCallResults = new Dictionary<string, string>();
		private readonly Dictionary<string, IrcUser> whoisCallResults = new Dictionary<string, IrcUser>();
		public DataFunctionSet dataFunctionSet { private get; set; }

		private IrcClient client;

		private const int messageLengthLimit = 510;

		private readonly List<string> whoisCalls = new List<string>();
		private readonly List<string> nickservCalls = new List<string>(); // Holds information about which users are currently being looked up
		private bool CanDoNickservCall = true;
		public bool HasNickservCall { get { return nickservCalls.Count > 0; } }
		public bool HasWhoisCall { get { return whoisCalls.Count > 0; } }

		public int ChannelCount { get { return client.ChannelCount; } }
		public bool ReplyToPings
		{
			get
			{
				return client.ReplyToPings;
			}
			set
			{
				client.ReplyToPings = value;
			}
		}
		public bool InChannel(string channel)
		{
			return client.InChannel(channel);
		}


		public int TotalUserCount { get { return client.TotalUserCount; } }

		public IrcInterface(IrcClient client)
		{
			this.client = client;
		}

		public void UpdateClient(IrcClient client)
		{
			this.client = client;
		}

		public void Reconnect()
		{
			client.DisconnectWithPingTimeout();
		}

		public void DisableNickservCalls()
		{
			CanDoNickservCall = false;
		}

		public void AddNickserv(string nick, string nickserv)
		{
			if (nickservCallResults.ContainsKey(nick)) {
				if (nickservCallResults[nick] == nickserv) {
					Logger.Log(string.Format("Dropped NickServ reply for {0}:{1} as an entry already exists.", nick, nickserv), LogLevel.Warning);
				} else {
					Logger.Log(string.Format("Invalid NickServ reply stored for {0}:{1}. The stored value was {2}.", nick, nickserv, nickservCallResults[nick]), LogLevel.Error);
					nickservCallResults[nick] = nick;
				}
				return;
			}
			nickservCallResults.Add(nick, nickserv);
		}

		public string DoNickservCall(string nick)
		{
			if (!CanDoNickservCall) return null;

			Logger.Log("Nickserv call requested for " + nick, LogLevel.Debug);

			if (!nickservCalls.Contains(nick)) {
				nickservCalls.Add(nick);
				Logger.Log("Calling NickServ for " + nick, LogLevel.Info);
				SendMessage("NickServ", "INFO " + nick);
			} else {
				Logger.Log("An entry already exists for " + nick, LogLevel.Debug);
			}
			nick = nick.ToLower();


			var waitTime = 0;
			while (!nickservCallResults.ContainsKey(nick)) {
				Thread.Sleep(20);
				waitTime += 20;
				if (waitTime == 6000) {
					Logger.Log("No nickserv reply received for {0} after 6 seconds", LogLevel.Warning, true, nick);
				}
			}
			nickservCalls.Remove(nick);
			return nickservCallResults[nick];
		}

		private int CalculateMessageLength(string target, string message)
		{
			return CalculatePrefixLength(target) + message.Length;
		}
		private int CalculatePrefixLength(string target)
		{
			var prefix = string.Format(":{0}!{1}@{2} PRIVMSG {3} :", client.Nick, client.Ident, client.LocalHost, target);
			return prefix.Length;
		}
		private int GetMaxMessageLength(string target)
		{
			return messageLengthLimit - CalculatePrefixLength(target);
		}
		private string GenerateFullMessage(string target, string message)
		{
			return string.Format(":{0}!{1}@{2} PRIVMSG {3} :{4}", client.Nick, client.Ident, client.LocalHost, target, message);
		}

		private void SendMessageChunk(string target, string message, int recursionDepth)
		{
			int floodLimit;
			if (!int.TryParse(Settings.Instance["irc_flood_limit"], out floodLimit))
				floodLimit = 4;

			if (recursionDepth >= floodLimit) {
				client.SendMessage(target, "Flood limit triggered. The remaining part of the message has been discarded.");
				return;
			}
			string cutoff = null;
			if (CalculateMessageLength(target, message) > messageLengthLimit) {
				cutoff = message.Substring(GetMaxMessageLength(target));
				message = message.Substring(0, GetMaxMessageLength(target));
			}

			var fullMsg = GenerateFullMessage(target, message);

			if (fullMsg.Length > messageLengthLimit) {
				Logger.Log("Message prototype exceeds maximum allowed message length! Message prototype: " + fullMsg, LogLevel.Warning);
			}

			var result = client.SendMessage(target, message);
			if (result && dataFunctionSet.ConnectionState != ConnectionState.Closed) {
				dataFunctionSet.AddIrcMessage(DateTime.Now, 0, target, Settings.Instance["irc_nick"], message);
			}
			if (cutoff != null) {
				SendMessageChunk(target, cutoff, ++recursionDepth);
			}
		}

		public void SendMessage(string target, string message)
		{
			SendMessageChunk(target, message, 0);
		}

		/// <summary>
		/// Sends a message directly to the IRC client, without trying to make sure the message is sent in chunks.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="message"></param>
		public void SendMessageDirectly(string target, string message)
		{
			if(client.SendMessage(target, message)){
				dataFunctionSet.AddIrcMessage(DateTime.Now, 0, target, Settings.Instance["irc_nick"], message);
			}
		}

		public void SendRaw(string line)
		{
			client.SendRaw(line);
		}

		public void NotifyOperator(string message)
		{
			SendMessage(Settings.Instance["operator_nick"], message);
		}

		public bool JoinChannel(string channel)
		{
			return client.JoinChannel(channel);
		}

		public void TestNickServ()
		{
			client.SendMessage("NickServ", "INFO");
		}

		public IrcUser DoWhoisCall(string nick)
		{
			whoisCalls.Add(nick);
			var t = new Thread(() => client.SendRaw("WHOIS " + nick));
			t.Start();

			while (!whoisCallResults.ContainsKey(nick)) {
				Thread.Sleep(20);
			}
			whoisCalls.Remove(nick);
			var result = whoisCallResults[nick];
			whoisCallResults.Remove(nick);
			return result;
		}

		public void AddUser(string nick, IrcUser user)
		{
			whoisCallResults.Add(nick, user);
		}

		public void Disconnect(string reason = null)
		{
			client.Quit(reason);
		}

		public void Part(string channel, string reason = null)
		{
			client.Part(channel, reason);
		}

		public bool Connected { get { return client.Connected; } }

		internal void ChangeClient(IrcClient client)
		{
			this.client = client;
		}

		internal List<string> GetUsers(string channel)
		{
			return client.GetUsers(channel);
		}
	}
}
