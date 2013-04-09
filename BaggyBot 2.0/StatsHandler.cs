using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot
{
	class StatsHandler
	{
		private DataFunctionSet dataFunctionSet;
		private SqlConnector sqlConnector;
		private IrcClient client;

		// Non-exhaustive list of shared - idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };

		public StatsHandler(DataFunctionSet dm, SqlConnector sc, IrcClient c)
		{
			dataFunctionSet = dm;
			sqlConnector = sc;
			client = c;
		}

		public void HandleNickChange(IrcUser user, string newNick)
		{
			int[] uids = dataFunctionSet.GetUids(user);
			if (uids.Length != 1) {
				Logger.Log("Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
				return;
			}
			dataFunctionSet.AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), null, uids[0]);
		}

		private int[] GetMatchesFirstLevel(IrcUser sender)
		{
			return dataFunctionSet.GetUids(sender);
		}
		private int[] GetMatchesSecondLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = '{0}' AND ident = '{1}'", sender.Nick, sender.Ident);
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nickserv = '{0}'", nickserv);
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE ident = '{0}' AND hostmask = '{1}'", sender.Ident, sender.Hostmask);
			return sqlConnector.SelectVector<int>(query);
		}

		delegate int Level();
		private int GetIdFromUser(IrcUser user)
		{
			Level l4 = () =>
			{
				var res = GetMatchesFourthLevel(user);
				if (res.Length == 0) {
					var uid = dataFunctionSet.AddCredCombination(user);
					return uid;
				} else if (res.Length == 1) {
					dataFunctionSet.AddCredCombination(user, null, res[0]);
					return res[0];
				} else {
					return -1;
				}
			};

			Level l3 = () =>
			{
				var nickserv = DoNickservCall(user.Nick);

				if(nickserv == null) // No nickserv info available, try a level 4 instead
				{
					return l4();
				}

				var res = GetMatchesThirdLevel(nickserv);
				if (res.Length == 1) { // Match found trough NickServ, add a credentials combinations for easy access next time.
					dataFunctionSet.AddCredCombination(user, nickserv, res[0]);
					return res[0];
				} else if (res.Length == 0) { // No matches found, not even with NickServ. Most likely a new user, unless you change your hostname and ident, and log in with a different nick than the one you logged out with.
					var uid = dataFunctionSet.AddCredCombination(user, nickserv);
					return uid;
				} else { // Multiple people registered using the same NickServ account? That's most likely an error.
					return -1;
				}
			};

			Level l2 = () =>
			{
				var res = GetMatchesSecondLevel(user);
				if (res.Length == 1) {
					dataFunctionSet.AddCredCombination(user, null, res[0]);
					return res[0];
				} else {
					return l3();
				}
			};

			Level l1 = () =>
			{
				var res = GetMatchesFirstLevel(user);
				if (res.Length == 1) return res[0];
				else if (res.Length == 0) return l2();
				else return l3();
			};

			return l1();
		}

		public void ProcessMessage(IrcMessage message)
		{
			int userId = GetIdFromUser(message.Sender);

			List<string> words = GetWords(message.Message);

			IncrementLineCount(userId);
			IncrementWordCount(userId, words.Count);
		}

		private string DoNickservCall(string nick)
		{
			client.SendMessage("NickServ", "INFO " + nick);
			return null;
		}

		private List<string> GetWords(string message)
		{
			List<string> words = message.Trim().Split(' ').ToList<string>();
			for (int i = 0; i < words.Count; i++) {
				words[i] = words[i].Trim();
				if (words[i] == string.Empty) {
					words.RemoveAt(i);
					i--;
				}
			}
			return words;
		}

		private void IncrementLineCount(int uid)
		{
			string statement = String.Format("INSERT INTO userstats VALUES ({0}, 1, 0, 0, 0) ON DUPLICATE KEY UPDATE `lines` = `lines` +1", uid);
			sqlConnector.ExecuteStatement(statement);
		}
		private void IncrementWordCount(int uid, int words)
		{
			string statement = String.Format("UPDATE userstats SET words = words + {0} WHERE user_id = {1}", words, uid);
		}
	}
}
