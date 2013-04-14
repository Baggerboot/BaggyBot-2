using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IRCSharp;
using MySql.Data;
using MySql.Data.MySqlClient;

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

		/// <summary>
		/// Turns an 'unsafe' string into a safe string, by escaping all escape characters, and adding single quotes around the string.
		/// This means that you do NOT have to use single quotes in your insert statements anymore.
		/// </summary>
		internal string Safe(string sqlString)
		{
			string newString = sqlString.Replace(@"\", @"\\");
			newString = newString.Replace("'", "''");
			newString = newString.Insert(0, "'");
			newString += "'";
			return newString;
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

		internal void Snag(IrcMessage message)
		{
			int uid = GetIdFromUser(message.Sender);
			string query = string.Format("INSERT INTO quotes VALUES(NULL, {0}, {1})", uid, Safe(message.Message));
			sqlConnector.ExecuteStatement(query);
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		internal int[] GetUids(IrcUser ircUser)
		{
			string query = string.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = {0} AND ident = {1} AND hostmask = {2}", Safe(ircUser.Nick), Safe(ircUser.Ident), Safe(ircUser.Hostmask));
			return sqlConnector.SelectVector<int>(query);
		}

		/// <summary>
		/// Purges the database, removing all tables, allowing the bot to regenerate them.
		/// </summary>
		internal void PurgeDatabase()
		{
			string statement =
					@"drop table `stats_bot`.`emoticons`;
					drop table `stats_bot`.`quotes`;
					drop table `stats_bot`.`urls`;
					drop table `stats_bot`.`usercreds`;
					drop table `stats_bot`.`userstats`;
					drop table `stats_bot`.`var`;
					drop table `stats_bot`.`words`;";
			sqlConnector.ExecuteStatement(statement);
			Console.WriteLine("Database purged");
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

			if (nickserv == null) nickserv = "NULL";
			else nickserv = Safe(nickserv);

			string query = string.Format("INSERT INTO usercreds VALUES (NULL, {0}, {1}, {2}, {3}, {4})", uid, Safe(user.Nick), Safe(user.Ident), Safe(user.Hostmask), nickserv);
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
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = {0} AND ident = {1}", Safe(sender.Nick), Safe(sender.Ident));
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE ns_login = {0}", Safe(nickserv));
			return sqlConnector.SelectVector<int>(query);
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE ident = {0} AND hostmask = {1}", Safe(sender.Ident), Safe(sender.Hostmask));
			return sqlConnector.SelectVector<int>(query);
		}

		/// <summary>
		/// Attempts to retrieve the user ID for a given nickname from the database.
		/// Will not work if other users have used that nickname at some point.
		/// </summary>
		/// <returns>The user ID of the user if successful, -1 if there were multiple matches, -2 if there were no matches.</returns>
		internal int GetIdFromNick(string nick)
		{
			string query = String.Format("SELECT DISTINCT user_id FROM usercreds WHERE nick = {0}", Safe(nick));
			int[] results = sqlConnector.SelectVector<int>(query);
			if (results.Length > 1) {
				return -1;
			} if (results.Length == 0) {
				return -2;
			} else {
				return results[0];
			}
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
			string statement = String.Format("INSERT INTO emoticons VALUES (NULL, {0}, 1, {1}) ON DUPLICATE KEY UPDATE `uses` = `uses` + 1, `last_used_by` = {1}", Safe(emoticon), user);
			sqlConnector.ExecuteStatement(statement);
			Logger.Log("Added emoticon: " + emoticon);
		}

		internal void IncrementVar(string id, int amount = 1)
		{
			string statement = String.Format("INSERT INTO var VALUES (NULL, {0}, {1}) ON DUPLICATE KEY UPDATE `value` = `value` + {1}", Safe(id), amount);
			sqlConnector.ExecuteStatement(statement);
		}

		/// <summary>
		/// Processes a user's credentials when they change their nickname, adding a new credentials entry when necessary.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNick"></param>
		internal void HandleNickChange(IrcUser user, string newNick)
		{
			
			int count = (int)sqlConnector.SelectOne<long>(String.Format("SELECT COUNT(*) FROM usercreds WHERE nick = {0} AND ident = {1} AND hostmask = {2}", Safe(newNick), Safe(user.Ident), Safe(user.Hostmask)));
			if (count == 1) { // No need to add an entry if an identical set of credentials already exists
				return;
			} else if (count > 1) {
				Logger.Log(String.Format("Multiple credentials found for combination: nick={0}, ident={1}, hostmask={2}", newNick, user.Ident, user.Hostmask), LogLevel.Warning);
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
			string statement = String.Format("INSERT INTO urls VALUES (NULL, {0}, 1, {1}, {2}) ON DUPLICATE KEY UPDATE `uses` = `uses` + 1, `last_used_by` = {1}, `last_usage` = {2}", Safe(url), user, Safe(usage));
			sqlConnector.ExecuteStatement(statement);
		}

		internal void IncrementWord(string word)
		{
			string statement = String.Format("INSERT INTO words VALUES (NULL, {0}, 1) ON DUPLICATE KEY UPDATE `uses` = `uses` + 1", Safe(word));
			sqlConnector.ExecuteStatement(statement);
		}

		internal void IncrementProfanities(int sender)
		{
			string statement = String.Format("UPDATE userstats SET profanities = profanities +1 WHERE user_id = {0}", sender);
			sqlConnector.ExecuteStatement(statement);
		}


		internal void IncrementActions(int sender)
		{
			string statement = "UPDATE userstats SET actions = actions + 1 WHERE user_id = " + sender;
			sqlConnector.ExecuteStatement(statement);
		}
	}
}
