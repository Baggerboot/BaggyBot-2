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
		private IrcInterface ircInterface;

		// Non-exhaustive list of shared - idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };

		public StatsHandler(DataFunctionSet dm, SqlConnector sc, IrcInterface inter)
		{
			dataFunctionSet = dm;
			sqlConnector = sc;
			ircInterface = inter;
		}

		public void HandleNickChange(IrcUser user, string newNick)
		{
			int count = (int) sqlConnector.SelectOne<long>(String.Format("SELECT COUNT(*) FROM usercreds WHERE nick = '{0}' AND ident = '{1}' AND hostmask = '{2}'", newNick, user.Ident, user.Hostmask));
			if (count == 1) {
				return;
			} else if (count > 1) {
				Logger.Log(String.Format("Multiple credentials found for combination nick:{0}, ident{1}, hostmask{2}", newNick, user.Ident, user.Hostmask), LogLevel.Warning);
				return;
			}
			int[] uids = dataFunctionSet.GetUids(user);
			if (uids.Length != 1) {
				Logger.Log("Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
				return;
			}
			string nickserv = dataFunctionSet.GetNickserv(uids[0]);
			dataFunctionSet.AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), nickserv, uids[0]);
		}

		

		public void ProcessMessage(IrcMessage message)
		{
			int userId = dataFunctionSet.GetIdFromUser(message.Sender);

			List<string> words = GetWords(message.Message);

			IncrementLineCount(userId);
			IncrementWordCount(userId, words.Count);
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
