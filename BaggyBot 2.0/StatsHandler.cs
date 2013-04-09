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

		// Non-exhaustive list of shared - idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };

		public StatsHandler(DataFunctionSet dm, SqlConnector sc)
		{
			dataFunctionSet = dm;
			sqlConnector = sc;
		}

		private uint[] GetMatchesFirstLevel(IrcUser sender)
		{
			return dataFunctionSet.GetUids(sender);
		}

		private uint[] GetMatchesSecondLevel(IrcUser sender)
		{
			string query = String.Format("SELECT user_id FROM usercreds WHERE nick = '{0}' AND ident = '{1}'", sender.Nick, sender.Ident);
			return sqlConnector.SelectVector<uint>(query);
		}

		public void ProcessMessage(IrcMessage message)
		{
			uint userId = 0;
			uint[] matches = GetMatchesFirstLevel(message.Sender);

			if (matches.Length == 0) {
				if (!sharedIdents.Contains(message.Sender.Ident)) {
					uint[] matches2 = GetMatchesFirstLevel(message.Sender);
					if (matches2.Length == 1)
						userId = matches2[0];
				} else { // Try a nickserv info call first

				}

				dataFunctionSet.AddCredCombination(message.Sender, null, -1);

			} else if (matches.Length > 1) {

			}

			List<string> words = message.Message.Trim().Split(' ').ToList<string>();
			for (int i = 0; i < words.Count; i++) {
				words[i] = words[i].Trim();
				if (words[i] == string.Empty) {
					words.RemoveAt(i);
					i--;
				}
			}

			IncrementLineCount(userId);
			IncrementWordCount(userId, words.Count);
		}
		private void IncrementLineCount(uint uid)
		{
			string statement = String.Format("INSERT INTO userstats VALUES ({0}, 1, 0) ON DUPLICATE KEY UPDATE lines = lines +1", uid);
			sqlConnector.ExecuteStatement(statement);
		}
		private void IncrementWordCount(uint uid, int words)
		{
			string statement = String.Format("UPDATE userstats SET words = words + {0} WHERE user_id = {1}", words, uid);
		}
	}
}
