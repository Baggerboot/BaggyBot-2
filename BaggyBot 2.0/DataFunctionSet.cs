using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IRCSharp;
using MySql.Data;
using MySql.Data.MySqlClient;

using System.Data.Linq;
using BaggyBot.Database;

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
			Lock = sqlConnector;

		}

		public object Lock;

		/// <summary>
		/// Turns an 'unsafe' string into a safe string, by escaping all escape characters, and adding single quotes around the string.
		/// This means that you do NOT have to use single quotes in your insert statements anymore.
		/// </summary>
		/*internal string Safe(string sqlString)
		{
			string newString = sqlString.Replace(@"\", @"\\");
			newString = newString.Replace("'", "''");
			newString = newString.Insert(0, "'");
			newString += "'";
			return newString;
		}*/

		internal void IncrementLineCount(int uid)
		{
			UserStatistics nstat = null;

			var matches =
				from stat in sqlConnector.UserStats
				where stat.user_id == uid
				select stat;

			if (matches.Count() != 0) {
				matches.First().lines++;
				sqlConnector.SubmitChanges();
			} else {
				nstat = new UserStatistics();
				nstat.user_id = uid;
				nstat.lines = 1;
				sqlConnector.UserStats.InsertOnSubmit(nstat);
				sqlConnector.SubmitChanges();
			}
			
		}

		internal void InitializeDatabase()
		{
			Logger.Log("Initializing database...");

			List<string> createStmts = new List<string>();


			createStmts.Add(
			@"CREATE  TABLE usercreds (
			  id INT NOT NULL IDENTITY ,
			  user_id INT NOT NULL ,
			  nick NVARCHAR(45) NOT NULL ,
			  ident NVARCHAR(16) NOT NULL ,
			  hostmask NVARCHAR(128) NOT NULL ,
			  ns_login NVARCHAR(45) NULL ,
			  PRIMARY KEY (id) 
			);");

			createStmts.Add(
			@"CREATE  TABLE userstats (
			  user_id INT NOT NULL ,
			  lines INT NOT NULL ,
			  words INT NOT NULL ,
			  actions INT NOT NULL ,
			  profanities INT NOT NULL ,
			  PRIMARY KEY (user_id) 
			);");

			createStmts.Add(
			@"CREATE TABLE quotes (
			  id INT NOT NULL IDENTITY,
			  user_id INT NOT NULL ,
			  quote TEXT NOT NULL ,
			  PRIMARY KEY (id) 
			);");

			createStmts.Add(
			@"CREATE  TABLE var (
			  id INT NOT NULL IDENTITY ,
			  [key] NVARCHAR(45) NOT NULL ,
			  value INT NOT NULL ,
			  PRIMARY KEY (id) ,
			  CONSTRAINT unq_key UNIQUE ([key]) 
			);");

			createStmts.Add(
			@"CREATE  TABLE emoticons (
			  id INT NOT NULL IDENTITY ,
			  emoticon NVARCHAR(45) NOT NULL ,
			  uses INT NOT NULL ,
			  last_used_by INT NOT NULL ,
			  PRIMARY KEY (id) ,
			  CONSTRAINT unq_emoticon UNIQUE (emoticon) 
			);");

			createStmts.Add(
			@"CREATE  TABLE urls (
			  id INT NOT NULL IDENTITY ,
			  url NVARCHAR(220) NOT NULL ,
			  uses INT NOT NULL ,
			  last_used_by INT NOT NULL ,
			  last_usage TEXT NOT NULL ,
			  PRIMARY KEY (id) ,
			  CONSTRAINT unq_url UNIQUE (url) 
			);");

			createStmts.Add(
			@"CREATE  TABLE words (
			  id INT NOT NULL IDENTITY ,
			  word NVARCHAR(220) NOT NULL ,
			  uses INT NOT NULL ,
			  PRIMARY KEY (id) ,
			  CONSTRAINT unq_word UNIQUE (word) 
			);");

			createStmts.Add(
			@"CREATE TABLE names (
			  user_id INT NOT NULL ,
			  name NVARCHAR(90) NOT NULL ,
			  PRIMARY KEY (user_id) 
			);");

			foreach (string str in createStmts) {
				sqlConnector.ExecuteStatement(str);
			}

			Logger.Log("Done.");
		}

		internal void IncrementWordCount(int uid, int words)
		{
			var matches =
				from stat in sqlConnector.UserStats
				where stat.user_id == uid
				select stat;

			matches.First().words += words;

			sqlConnector.SubmitChanges();
			//string statement = String.Format("UPDATE userstats SET words = words + {0} WHERE user_id = {1}", words, uid);
			//sqlConnector.ExecuteStatement(statement);
		}

		internal void Snag(IrcMessage message)
		{
			int uid = GetIdFromUser(message.Sender);
			Quote q = new Quote();
			q.quote1 = message.Message;
			q.user_id = uid;

			sqlConnector.Quotes.InsertOnSubmit(q);
			sqlConnector.SubmitChanges();
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		internal int[] GetUids(IrcUser ircUser)
		{
			var results = (from res in sqlConnector.UserCreds
						   where res.nick == ircUser.Nick
						   && res.ident == ircUser.Ident
						   && res.hostmask == ircUser.Hostmask
						   select res.user_id).Distinct();

			int count = results.Count();
			try {
				return results.ToArray();
			} catch (InvalidOperationException) {
				int[] arr = new int[1];
				arr[0] = results.First();

				return arr;
			}
		}

		/// <summary>
		/// Purges the database, removing all tables, allowing the bot to regenerate them.
		/// </summary>
		internal void PurgeDatabase()
		{
			string statement =
					@"drop table stats_bot.dbo.emoticons;
					drop table stats_bot.dbo.quotes;
					drop table stats_bot.dbo.urls;
					drop table stats_bot.dbo.usercreds;
					drop table stats_bot.dbo.userstats;
					drop table stats_bot.dbo.var;
					drop table stats_bot.dbo.words;
					drop table stats_bot.dbo.names";
			sqlConnector.ExecuteStatement(statement);
			Console.WriteLine("Database purged");
		}

		/// <summary>
		/// Adds a user credentials combination to the dbo.usercreds table.
		/// </summary>
		/// <param name="user">The user to draw the credentials from</param>
		/// <param name="nickserv">The nickserv account used by the user</param>
		/// <param name="uid">The user ID to be used. Set to -1 (default value) if the credentials belong to a new user</param>
		/// <returns>The user id belonging to the new credentials combination</returns>
		internal int AddCredCombination(IrcUser user, string nickserv = null, int uid = -1)
		{
			if (uid == -1) {
				var results = (from c in sqlConnector.UserCreds
					   select c.user_id);
				if (!results.Any()) {
					uid = 1;
				} else {
					uid = results.Max() + 1;
				}
			}

			/*if (nickserv == null) nickserv = "NULL";
			else nickserv = Safe(nickserv);*/

			UserCredentials cred = new UserCredentials();
			cred.nick = user.Nick;
			cred.ident = user.Ident;
			cred.hostmask = user.Hostmask;
			cred.ns_login = nickserv;
			cred.user_id = uid;

			sqlConnector.UserCreds.InsertOnSubmit(cred);
			sqlConnector.SubmitChanges();

			if ((from n in sqlConnector.UserNames
				 where n.user_id == uid
				 select n).Any()) return uid;

			Name name = new Name();
			name.user_id = uid;
			name.name1 = user.Nick;

			sqlConnector.UserNames.InsertOnSubmit(name);
			sqlConnector.SubmitChanges();

			return uid;
		}

		internal void SubmitChanges()
		{
			sqlConnector.SubmitChanges();
		}

		internal void SetPrimary(int uid, string name)
		{
			var results = (from n in sqlConnector.UserNames
						   where n.user_id == uid
						   select n);
			if (results.Any()) {
				results.First().name1 = name;
			} else {
				Name n = new Name();
				n.user_id = uid;
				n.name1 = name;
				sqlConnector.UserNames.InsertOnSubmit(n);
			}

			sqlConnector.SubmitChanges();
		}

		internal string GetNickserv(int uid)
		{
			return (from c in sqlConnector.UserCreds
					where c.user_id == uid
					select c.ns_login).First();
		}

		private int[] GetMatchesFirstLevel(IrcUser sender)
		{
			return GetUids(sender);
		}
		private int[] GetMatchesSecondLevel(IrcUser sender)
		{
			return (from c in sqlConnector.UserCreds
					where c.nick == sender.Nick
					&& c.ident == sender.Ident
					select c.user_id).Distinct().ToArray();
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			return (from c in sqlConnector.UserCreds
					where c.ns_login == nickserv
					select c.user_id).Distinct().ToArray();
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			return (from c in sqlConnector.UserCreds
					where c.hostmask == sender.Hostmask
					&& c.ident == sender.Ident
					select c.user_id).Distinct().ToArray();
		}

		/// <summary>
		/// Attempts to retrieve the user ID for a given nickname from the database.
		/// Will not work if other users have used that nickname at some point and that nickname is not in the table of primary nicks.
		/// </summary>
		/// <returns>The user ID of the user if successful, -1 if there were multiple matches, -2 if there were no matches.</returns>
		internal int GetIdFromNick(string nick)
		{
			var results = (from n in sqlConnector.UserNames
						   where n.name1 == nick
						   select n.user_id);

			int count = results.Count();

			if (count == 1) return results.First();
			else if (count > 1) return -1;

			results = (from c in sqlConnector.UserCreds
					   where c.nick == nick
					   select c.user_id);

			if (count == 1) return results.First();
			else if (count > 1) return -1;
			else return -2;
		}

		delegate int Level();
		internal int GetIdFromUser(IrcUser user)
		{

			// Check for a match with ident and hostmask, but only if nickserv fails
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

			// Check for a nickserv match
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

			// Check for a match with nick and ident
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

			// Check for a match with nick, ident and hostmask
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
			var matches = from tEmoticon in sqlConnector.Emoticons
						  where tEmoticon.emoticon1 == emoticon
						  select tEmoticon;
			if (matches.Count() == 0) {
				Emoticon insert = new Emoticon();
				insert.emoticon1 = emoticon;
				insert.last_used_by = user;
				insert.uses = 1;
				sqlConnector.Emoticons.InsertOnSubmit(insert);
			} else {
				matches.First().uses++;
				matches.First().last_used_by = user;
			}
			sqlConnector.SubmitChanges();
		}

		internal void IncrementVar(string key, int amount = 1)
		{
			var matches = from pair in sqlConnector.KeyValuePairs
						  where pair.key == key
						  select pair;

			if (matches.Count() == 0) {
				KeyValuePair p = new KeyValuePair();
				p.key = key;
				p.value = amount;
				sqlConnector.KeyValuePairs.InsertOnSubmit(p);
			} else {
				matches.First().value += amount;
			}
			sqlConnector.SubmitChanges();
		}

		/// <summary>
		/// Processes a user's credentials when they change their nickname, adding a new credentials entry when necessary.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNick"></param>
		internal void HandleNickChange(IrcUser user, string newNick)
		{
			var newCreds = (from c in sqlConnector.UserCreds
							where c.nick == newNick
							&& c.ident == user.Ident
							&& c.hostmask == user.Hostmask
							select c);

			int count = newCreds.Count();

			if (count != 0) {
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
			var matches = from tUrl in sqlConnector.Urls
						  where tUrl.url1 == url
						  select tUrl;

			if (matches.Count() == 0) {
				Url u = new Url();
				u.last_usage = usage;
				u.last_used_by = user;
				u.url1 = url;
				sqlConnector.Urls.InsertOnSubmit(u);
			} else {
				matches.First().uses++;
				matches.First().last_usage = usage;
				matches.First().last_used_by = user;
			}
			sqlConnector.SubmitChanges();
		}

		internal void IncrementWord(string word)
		{
			var matches = from tWord in sqlConnector.Words
						  where tWord.word1 == word
						  select tWord;

			if (matches.Count() == 0) {
				Word w = new Word();
				w.word1 = word;
				w.uses = 1;
				sqlConnector.Words.InsertOnSubmit(w);
			} else {
				matches.First().uses++;
			}
			sqlConnector.SubmitChanges();
		}

		internal void IncrementProfanities(int sender)
		{
			(from s in sqlConnector.UserStats
			 where s.user_id == sender
			 select s).First().profanities++;

			sqlConnector.SubmitChanges();
		}


		internal void IncrementActions(int sender)
		{
			(from s in sqlConnector.UserStats
			 where s.user_id == sender
			 select s).First().actions++;

			sqlConnector.SubmitChanges();
		}
	}
}
