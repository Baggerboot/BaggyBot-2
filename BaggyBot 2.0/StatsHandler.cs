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

		private string nickServCallResult;

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
			uint[] uids = dataFunctionSet.GetUids(user);
			if (uids.Length != 1) {
				Logger.Log("Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
				return;
			}
			dataFunctionSet.AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), null, checked((int)uids[0]));
		}

		private uint[] GetMatchesFirstLevel(IrcUser sender)
		{
			return dataFunctionSet.GetUids(sender);
		}
		private uint[] GetMatchesSecondLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = '{0}' AND ident = '{1}'", sender.Nick, sender.Ident);
			return sqlConnector.SelectVector<uint>(query);
		}
		private uint[] GetMatchesThirdLevel(string nickserv)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nickserv = '{0}'", nickserv);
			return sqlConnector.SelectVector<uint>(query);
		}

		public void ProcessMessage(IrcMessage message)
		{
			uint userId = 0;

			// Check if a user with credentials of message.Sender already exists
			uint[] matches = GetMatchesFirstLevel(message.Sender);

			if (matches.Length == 1) {
				userId = matches[0];
			} else if (matches.Length == 0) {
				if (!sharedIdents.Contains(message.Sender.Ident)) {
					uint[] matches2 = GetMatchesFirstLevel(message.Sender);
					if (matches2.Length == 1) {// Found a match
						userId = matches2[0];
						dataFunctionSet.AddCredCombination(message.Sender, null, checked((int)userId));
					} else { // Try a nickserv info call
						Logger.Log("User not found.", LogLevel.Warning);
						return;
					}
				} else { // Try a nickserv info call first
					Logger.Log("User not found.", LogLevel.Warning);
					return;
				}
				dataFunctionSet.AddCredCombination(message.Sender, null, -1);
			} else { // Try a nickserv info call
				string nickserv = DoNickservCall(message.Sender.Nick);
				uint[] matches2 = GetMatchesThirdLevel(nickserv);
				if (matches2.Length == 1) {
					userId = matches2[0];
					dataFunctionSet.AddCredCombination(message.Sender, nickserv, checked((int)userId));
				} else {
					Logger.Log("Could not make a distinction between these userIDs: " + matches.ToString(), LogLevel.Error);
				}
			}

			List<string> words = GetWords(message.Message);

			IncrementLineCount(userId);
			IncrementWordCount(userId, words.Count);
		}

		private string DoNickservCall(string nick)
		{
			client.SendMessage("NickServ", "INFO " + nick);
			while (nickServCallResult == null) {
				System.Threading.Thread.Sleep(20);
			}
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

		private void IncrementLineCount(uint uid)
		{
			string statement = String.Format("INSERT INTO userstats VALUES ({0}, 1, 0, 0, 0) ON DUPLICATE KEY UPDATE `lines` = `lines` +1", uid);
			sqlConnector.ExecuteStatement(statement);
		}
		private void IncrementWordCount(uint uid, int words)
		{
			string statement = String.Format("UPDATE userstats SET words = words + {0} WHERE user_id = {1}", words, uid);
		}
	}
}
