using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	class IrcInterface
	{
		private IrcClient client;
		
		public IrcInterface(IrcClient client)
		{
			this.client = client;
		}

		private Dictionary<string, string> nickservCallResults = new Dictionary<string, string>();
		private List<string> nickservCalls = new List<string>();

		public bool HasNickservCall
		{
			get
			{
				return nickservCalls.Count > 0;
			}
		}

		public void AddNickserv(string nick, string nickserv)
		{
			nickservCallResults.Add(nick, nickserv);
		}

		public string DoNickservCall(string nick)
		{
			if (!nickservCalls.Contains(nick)) {
				nickservCalls.Add(nick);
				var t = new System.Threading.Thread(() => SendMessage("NickServ", "INFO " + nick));
				t.Start();
			}
			//while (!nickservCallResults.ContainsKey(nick)) {
			//	System.Threading.Thread.Sleep(20);
			//}
			//nickservCalls.Remove(nick);
			//return nickservCallResults[nick];
			return null;
		}

		public void SendMessage(string target, string message)
		{
			client.SendMessage(target, message);
		}
	}
}
