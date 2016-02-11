using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.Monitoring;
using IRCSharp;
using IRCSharp.IRC;

namespace BaggyBot.DataProcessors
{
	public class IrcInterface
	{
		private const int messageLengthLimit = 510;

		private readonly List<string> whoisCalls = new List<string>();
		private readonly List<string> nickservCalls = new List<string>(); // Holds information about which users are currently being looked up
		private readonly Dictionary<string, string> nickservCallResults = new Dictionary<string, string>();
		private readonly Dictionary<string, IrcUser> whoisCallResults = new Dictionary<string, IrcUser>();

		private readonly IrcEventHandler ircEventHandler;
		public DataFunctionSet DataFunctionSet { private get; set; }
		private IrcClient client;
		
		private bool canDoNickservCall = true;

		public bool HasNickservCall => nickservCalls.Count > 0;
		public bool HasWhoisCall => whoisCalls.Count > 0;
		public int ChannelCount => client.ChannelCount;
		public string Password => client.Password;
		public int TotalUserCount => client.TotalUserCount;
		public bool Connected => client.Connected;

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

		public IrcInterface(IrcClient client)
		{
			this.client = client;
		}

		public bool InChannel(string channel)
		{
			return client.InChannel(channel);
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
			canDoNickservCall = false;
		}

		public void AddNickserv(string nick, string nickserv)
		{
			if (nickservCallResults.ContainsKey(nick))
			{
				if (nickservCallResults[nick] == nickserv)
				{
					Logger.Log(this, $"Dropped NickServ reply for {nick}:{nickserv} as an entry already exists.", LogLevel.Warning);
				}
				else {
					Logger.Log(this, $"Invalid NickServ reply stored for {nick}:{nickserv}. The stored value was {nickservCallResults[nick]}.", LogLevel.Error);
					nickservCallResults[nick] = nick;
				}
				return;
			}
			nickservCallResults.Add(nick, nickserv);
		}

		public string DoNickservCall(string nick)
		{
			if (!canDoNickservCall) return null;

			Logger.Log(this, "Nickserv call requested for " + nick, LogLevel.Debug);

			if (!nickservCalls.Contains(nick))
			{
				nickservCalls.Add(nick);
				Logger.Log(this, "Calling NickServ for " + nick, LogLevel.Info);
				SendMessage("NickServ", "INFO " + nick);
			}
			else {
				Logger.Log(this, "An entry already exists for " + nick, LogLevel.Debug);
			}
			nick = nick.ToLower();

			var waitTime = 0;
			while (!nickservCallResults.ContainsKey(nick))
			{
				Thread.Sleep(20);
				waitTime += 20;
				if (waitTime == 6000)
				{
					Logger.Log(this, $"No nickserv reply received for {nick} after 6 seconds", LogLevel.Warning);
					return null;
				}
			}
			nickservCalls.Remove(nick);
			return nickservCallResults[nick];
		}

		private int GetMaxMessageLength(string target)
		{
			return messageLengthLimit - GeneratePrefix(target).Length;
		}

		private string GeneratePrefix(string target)
		{
			return $":{client.Nick}!{client.Ident}@{client.LocalHost} PRIVMSG {target} :";
		}

		private string GenerateFullMessage(string target, string message)
		{
			return $":{client.Nick}!{client.Ident}@{client.LocalHost} PRIVMSG {target} :{message}";
		}

		/// <summary>
		/// Sends a message to the IRC server. Checks to ensure it doesn't exceed the length limit for IRC commands,
		/// breaking it up into multiple messages if it does.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="message"></param>
		/// <param name="recursionDepth"></param>
		/// <returns></returns>
		private bool SendMessageChunk(string target, string message, int recursionDepth)
		{
			var floodLimit = ConfigManager.Config.FloodLimit;

			if (recursionDepth >= floodLimit)
			{
				client.SendMessage(target, "Flood limit triggered. The remaining part of the message has been discarded.");
				return true;
			}
			string cutoff = null;
			if (GenerateFullMessage(target, message).Length > messageLengthLimit)
			{
				cutoff = message.Substring(GetMaxMessageLength(target));
				message = message.Substring(0, GetMaxMessageLength(target));
			}

			var result = client.SendMessage(target, message);
			if (result && DataFunctionSet.ConnectionState != ConnectionState.Closed)
			{
				DataFunctionSet.AddIrcMessage(DateTime.Now, 0, target, client.Nick, message);
			}
			if (cutoff != null)
			{
				return result && SendMessageChunk(target, cutoff, ++recursionDepth);
			}
			else
			{
				return result;
			}
		}

		public bool SendMessage(string target, string message)
		{
			return SendMessageChunk(target, message, 0);
		}

		public bool SendMessage(IEnumerable<string> targets, string message)
		{
			var success = true;
			foreach (var target in targets)
			{
				success = success && SendMessageChunk(target, message, 0);
			}
			return success;
		}

		/// <summary>
		/// Sends a message directly to the IRC client, without trying to make sure the message is sent in chunks.
		/// </summary>
		public void SendMessageDirectly(string target, string message)
		{
			if (client.SendMessage(target, message))
			{
				DataFunctionSet.AddIrcMessage(DateTime.Now, 0, target, client.Nick, message);
			}
		}

		public void SendRaw(string line)
		{
			client.SendRaw(line);
		}

		public void NotifyOperator(string message)
		{
			SendMessage(ConfigManager.Config.Operators.Select(op => op.Nick), message);
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

			while (!whoisCallResults.ContainsKey(nick))
			{
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
