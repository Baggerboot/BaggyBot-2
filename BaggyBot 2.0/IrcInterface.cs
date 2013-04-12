﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	class IrcInterface
	{
		private IrcClient 
			client;
		private Dictionary<string, string> nickservCallResults = new Dictionary<string, string>();
		private List<string> nickservCalls = new List<string>();
		private bool CanDoNickservCall = true;
		internal bool HasNickservCall { get { return nickservCalls.Count > 0; } }


		public IrcInterface(IrcClient client)
		{
			this.client = client;
		}

		internal void DisableNickservCalls()
		{
			CanDoNickservCall = false;
		}

		internal void AddNickserv(string nick, string nickserv)
		{
			nickservCallResults.Add(nick, nickserv);
		}

		internal string DoNickservCall(string nick)
		{
			if (!CanDoNickservCall) return null;

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

		internal void SendMessage(string target, string message)
		{
			client.SendMessage(target, message);
		}

		internal void JoinChannel(string channel)
		{
			client.JoinChannel(channel);
		}

		internal void TestNickServ()
		{
			client.SendMessage("NickServ", "INFO");
		}
	}
}
