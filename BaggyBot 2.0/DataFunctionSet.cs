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

		public DataFunctionSet(SqlConnector sqlConnector)
		{
			this.sqlConnector = sqlConnector;
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
		public int AddCredCombination(IrcUser user, string nickserv = null, int uid = -1)
		{
			if (uid == -1) {
				uid = sqlConnector.SelectOne<int>("SELECT MAX(user_id) FROM usercreds");
				uid++;
			}

			// Required since NULL must be passed without single quotes or SQL will see it as a string literal
			nickserv = nickserv == null ? "NULL" : string.Format("'{0}'",nickserv);

			string query = string.Format("INSERT INTO usercreds VALUES (NULL, {0},'{1}','{2}','{3}', {4})", uid, user.Nick, user.Ident, user.Hostmask, nickserv);
			sqlConnector.ExecuteStatement(query);
			return uid;
		}
	}
}
