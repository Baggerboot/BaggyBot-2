using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IRCSharp;

namespace BaggyBot
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	class DataFunctionSet
	{
		private SqlConnector sqlConnector;
		private IrcInterface ircInterface;

		public DataFunctionSet(SqlConnector sqlConnector, IrcInterface inter)
		{
			this.sqlConnector = sqlConnector;
			ircInterface = inter;
		}
		internal void IncrementLineCount(int uid)
		{
			string statement = String.Format("INSERT INTO userstats VALUES ({0}, 1, 0, 0, 0) ON DUPLICATE KEY UPDATE `lines` = `lines` +1", uid);
			sqlConnector.ExecuteStatement(statement);
		}
		internal void IncrementWordCount(int uid, int words)
		{
			string statement = String.Format("UPDATE userstats SET words = words + {0} WHERE user_id = {1}", words, uid);
			sqlConnector.ExecuteStatement(statement);
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		internal int[] GetUids(IrcUser ircUser)
		{
			string query = string.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = '{0}' AND ident = '{1}' AND hostmask = '{2}'", ircUser.Nick,ircUser.Ident,ircUser.Hostmask);
			return sqlConnector.SelectVector<int>(query);
		}

		/// <summary>
		/// Adds a user credentials combination to the usercreds table.
		/// </summary>
		/// <param name="user">The user to draw the credentials from</param>
		/// <param name="nickserv">The nickserv account used by the user</param>
		/// <param name="uid">The user ID to be used. Set to -1 (default value) if the credentials belong to a new user</param>
		/// <returns>The user id belonging to the new credentials combination</returns>
		internal int AddCredCombination(IrcUser user, string nickserv = null, int uid = -1)
		{
			if (uid == -1) {
				try {
					int result = sqlConnector.SelectOne<int>("SELECT MAX(user_id) FROM usercreds");
					uid = result;
					uid++;
				} catch (RecordNullException) {
					uid = 1;
				}
			}

			// Required since NULL must be passed without single quotes or SQL will see it as a string literal
			nickserv = nickserv == null ? "NULL" : string.Format("'{0}'",nickserv);

			string query = string.Format("INSERT INTO usercreds VALUES (NULL, {0},'{1}','{2}','{3}', {4})", uid, user.Nick, user.Ident, user.Hostmask, nickserv);
			sqlConnector.ExecuteStatement(query);
			return uid;
		}

		internal string GetNickserv(int uid)
		{
			string query = string.Format("SELECT DISTINCT ns_login FROM usercreds WHERE user_id = {0} LIMIT 1", uid);
			return sqlConnector.SelectOne<string>(query);
		}

		private int[] GetMatchesFirstLevel(IrcUser sender)
		{
			return GetUids(sender);
		}
		private int[] GetMatchesSecondLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = '{0}' AND ident = '{1}'", sender.Nick, sender.Ident);
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE ns_login = '{0}'", nickserv);
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE ident = '{0}' AND hostmask = '{1}'", sender.Ident, sender.Hostmask);
			return sqlConnector.SelectVector<int>(query);
		}

		delegate int Level();
		internal int GetIdFromUser(IrcUser user)
		{
			Level l4 = () =>
			{
				var res = GetMatchesFourthLevel(user);
				if (res.Length == 0) {
					var uid = AddCredCombination(user);
					return uid;
				} else if (res.Length == 1) {
					AddCredCombination(user, null, res[0]);
					return res[0];
				} else {
					return -1;
				}
			};

			Level l3 = () =>
			{
				string nickserv = ircInterface.DoNickservCall(user.Nick);

				if (nickserv == null) // No nickserv info available, try a level 4 instead
				{
					return l4();
				}

				var res = GetMatchesThirdLevel(nickserv);
				if (res.Length == 1) { // Match found trough NickServ, add a credentials combinations for easy access next time.
					AddCredCombination(user, nickserv, res[0]);
					return res[0];
				} else if (res.Length == 0) { // No matches found, not even with NickServ. Most likely a new user, unless you change your hostname and ident, and log in with a different nick than the one you logged out with.
					var uid = AddCredCombination(user, nickserv);
					return uid;
				} else { // Multiple people registered using the same NickServ account? That's most likely an error.
					return -1;
				}
			};

			Level l2 = () =>
			{
				var res = GetMatchesSecondLevel(user);
				if (res.Length == 1) {
					AddCredCombination(user, null, res[0]);
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
		internal void IncrementEmoticon(string emoticon, int user)
		{
			string statement = String.Format("INSERT INTO emoticons VALUES (NULL, '{0}', 1, {1}) ON DUPLICATE KEY UPDATE `uses` = `uses` + 1, `last_used_by` = {1}", emoticon, user);
			sqlConnector.ExecuteStatement(statement);
			Logger.Log("Added emoticon: " + emoticon);
		}

		internal void IncrementVar(string id, int amount = 1)
		{
			string statement = String.Format("INSERT INTO var VALUES (NULL, '{0}', {1}) ON DUPLICATE KEY UPDATE `value` = `value` + {1}", id, amount);
			sqlConnector.ExecuteStatement(statement);
		}

		internal void HandleNickChange(IrcUser user, string newNick)
		{
			int count = (int)sqlConnector.SelectOne<long>(String.Format("SELECT COUNT(*) FROM usercreds WHERE nick = '{0}' AND ident = '{1}' AND hostmask = '{2}'", newNick, user.Ident, user.Hostmask));
			if (count == 1) {
				return;
			} else if (count > 1) {
				Logger.Log(String.Format("Multiple credentials found for combination nick:{0}, ident{1}, hostmask{2}", newNick, user.Ident, user.Hostmask), LogLevel.Warning);
				return;
			}
			int[] uids = GetUids(user);
			if (uids.Length != 1) {
				Logger.Log("Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
				return;
			}
			string nickserv = GetNickserv(uids[0]);
			AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), nickserv, uids[0]);
		}

		internal void IncrementUrl(string url, int user, string usage)
		{
			string statement = String.Format("INSERT INTO urls VALUES (NULL, '{0}', 1, {1}, '{2}') ON DUPLICATE KEY UPDATE `uses` = `uses` + 1, `last_used_by` = {1}, `last_usage` = '{2}'", url, user, usage);
			sqlConnector.ExecuteStatement(statement);
		}
		internal void IncrementWord(string word)
		{
			string statement = String.Format("INSERT INTO words VALUES (NULL, '{0}', 1) ON DUPLICATE KEY UPDATE `uses` = `uses` + 1", word);
			sqlConnector.ExecuteStatement(statement);
		}

		internal void IncrementProfanities(int sender)
		{
			string statement = String.Format("UPDATE userstats SET profanities = profanities +1 WHERE user_id = {0}", sender);
			sqlConnector.ExecuteStatement(statement);
		}
		internal void IncrementActions(int sender)
		{
			string statement = "UPDATE userstats SET actions = actions +1";
			sqlConnector.ExecuteStatement(statement);
		}
	}
}
