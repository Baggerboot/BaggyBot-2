using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	public class IrcInterface
	{
		private Dictionary<string, string> nickservCallResults = new Dictionary<string, string>();
		private Dictionary<string, IrcUser> whoisCallResults = new Dictionary<string, IrcUser>();
		public DataFunctionSet dataFunctionSet { private get; set; }
		private IrcClient client;

		private List<string> whoisCalls = new List<string>();
		private List<string> nickservCalls = new List<string>();
		private bool CanDoNickservCall = true;
		public bool HasNickservCall { get { return nickservCalls.Count > 0; } }
		public bool HasWhoisCall { get { return whoisCalls.Count > 0; } }

		public int ChannelCount { get { return client.ChannelCount; } }
		public int TotalUserCount { get { return client.TotalUserCount; } }

		public IrcInterface(IrcClient client)
		{
			this.client = client;
		}

		public void DisableNickservCalls()
		{
			CanDoNickservCall = false;
		}

		public void AddNickserv(string nick, string nickserv)
		{
			nickservCallResults.Add(nick, nickserv);
		}

		public string DoNickservCall(string nick)
		{
			if (!CanDoNickservCall) return null;

			Logger.Log("Calling NickServ for " + nick, LogLevel.Info);

			if (!nickservCalls.Contains(nick)) {
				nickservCalls.Add(nick);
				var t = new System.Threading.Thread(() => SendMessage("NickServ", "INFO " + nick));
				t.Start();
			}
			nick = nick.ToLower();

			

			while (!nickservCallResults.ContainsKey(nick)) {
				System.Threading.Thread.Sleep(20);
			}
			nickservCalls.Remove(nick);
			return nickservCallResults[nick];
		}

		public void SendMessage(string target, string message)
		{
			dataFunctionSet.AddIrcMessage(DateTime.Now, 0, target, Settings.Instance["irc_nick"], message);
			client.SendMessage(target, message);
		}

		public void JoinChannel(string channel)
		{
			client.JoinChannel(channel);
		}

		public void TestNickServ()
		{
			client.SendMessage("NickServ", "INFO");
		}

		public IrcUser DoWhoisCall(string nick)
		{
			whoisCalls.Add(nick);
			var t = new System.Threading.Thread(() => client.SendRaw("WHOIS " + nick));
			t.Start();

			while (!whoisCallResults.ContainsKey(nick)) {
				System.Threading.Thread.Sleep(20);
			}
			whoisCalls.Remove(nick);
			IrcUser result = whoisCallResults[nick];
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
	}
}
