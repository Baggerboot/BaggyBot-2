using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IRCSharp;
using MySql.Data;
using MySql.Data.MySqlClient;
using BaggyBot.Database;
using System.Data.Linq;

#if postgresql
using BaggyBot.Database.PostgreSQL;
#endif
#if mssql
using BaggyBot.Database.MS_SQL;
#endif

namespace BaggyBot
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	public class DataFunctionSet
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
		/*public string Safe(string sqlString)
		{
			string newString = sqlString.Replace(@"\", @"\\");
			newString = newString.Replace("'", "''");
			newString = newString.Insert(0, "'");
			newString += "'";
			return newString;
		}*/

		public void IncrementLineCount(int uid)
		{
			Logger.Log("Preparing to increment lines.");
			UserStatistics nstat = null;

			var matches =
				from stat in sqlConnector.UserStats
				where stat.UserId == uid
				select stat;

			int line = 0;
			UserStatistics match = null;

			if (matches.Count() != 0) {
				match = matches.First();
				line = match.Lines;
				match.Lines++;
				Logger.Log("Incremented lines for " + uid + ".");
				SubmitChanges();
			} else {
				nstat = new UserStatistics();
				nstat.UserId = uid;
				nstat.Lines = 1;
				sqlConnector.UserStats.InsertOnSubmit(nstat);
				Logger.Log("Created new stats row for " + uid + ".");
				SubmitChanges();
			}
		}

		public void IncrementWordCount(int uid, int words)
		{
			var matches =
				from stat in sqlConnector.UserStats
				where stat.UserId == uid
				select stat;

			var match = matches.First();
			int originalWords = match.Words;
			match.Words += words;

			Logger.Log("Incremented words for " + uid + ".");
			SubmitChanges();
		}

		public void AddIrcMessage(DateTime time, int? sender, string channel, string nick, string message)
		{
			IrcLog line = new IrcLog();
			line.Time = time;
			line.Sender = sender;
			line.Channel = channel;
			line.Nick = nick;
			line.Message = message;

			lock (Lock) {
				sqlConnector.IrcLog.InsertOnSubmit(line);
				SubmitChanges();
			}

		}

		public List<Quote> FindQuote(string search)
		{
			var matches =
				from quote in sqlConnector.Quotes
				where quote.Quote1.ToLower().Contains(search.ToLower())
				select quote;

			int count = matches.Count();

			if (count == 0) {
				return null;
			}
			return matches.ToList();
		}

		public void Snag(IrcMessage message)
		{
			int uid = GetIdFromUser(message.Sender);
			Quote q = new Quote();
			q.Quote1 = message.Message;
			q.UserId = uid;

			sqlConnector.Quotes.InsertOnSubmit(q);
			Logger.Log("Added quote for " + message.Sender.Nick + ".");
			SubmitChanges();
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		public int[] GetUids(IrcUser ircUser)
		{
			var results = (from res in sqlConnector.UserCreds
						   where res.Nick == ircUser.Nick
						   && res.Ident == ircUser.Ident
						   && res.HostMask == ircUser.Hostmask
						   select res);

			int[] uids = results.Select(r => r.UserId).ToArray();

			uids = uids.Distinct().ToArray();

			int count = results.Count();
			try {
				return uids;
			} catch (InvalidOperationException e) {
				throw new Exception("InvalidOperationException thrown while trying to return user ids.", e);
				/*
				int[] arr = new int[1];
				arr[0] = results.First();

				return arr;*/
			}
		}

		/// <summary>
		/// Adds a user credentials combination to the dbo.usercreds table.
		/// </summary>
		/// <param name="user">The user to draw the credentials from</param>
		/// <param name="nickserv">The nickserv account used by the user</param>
		/// <param name="uid">The user ID to be used. Set to -1 (default value) if the credentials belong to a new user</param>
		/// <returns>The user id belonging to the new credentials combination</returns>
		public int AddCredCombination(IrcUser user, string nickserv = null, int uid = -1)
		{
			if (uid == -1) {
				var results = (from c in sqlConnector.UserCreds
					   select c.UserId);
				if (!results.Any()) {
					uid = 1;
				} else {
					uid = results.Max() + 1;
				}
			}

			/*if (nickserv == null) nickserv = "NULL";
			else nickserv = Safe(nickserv);*/

			UserCredentials cred = new UserCredentials();
			cred.Nick = user.Nick;
			cred.Ident = user.Ident;
			cred.HostMask = user.Hostmask;
			cred.NsLogin = nickserv;
			cred.UserId = uid;

			sqlConnector.UserCreds.InsertOnSubmit(cred);
			Logger.Log("Addecd credentials row for " + user.Nick + ".");
			SubmitChanges();

			if ((from n in sqlConnector.UserNames
				 where n.UserId == uid
				 select n).Any()) return uid;

			Name name = new Name();
			name.UserId = uid;
			name.Name1 = user.Nick;

			sqlConnector.UserNames.InsertOnSubmit(name);
			Logger.Log("Added name row for " + user.Nick + ".");
			SubmitChanges();

			return uid;
		}

		public void SubmitChanges()
		{
			sqlConnector.SubmitChanges();
		}

		internal bool SetPrimary(string oldName, string newName)
		{
			var results = (from name in sqlConnector.UserNames
						   where name.Name1 == oldName
						   select name);
			if (results.Any()) {
				results.First().Name1 = newName;
				sqlConnector.SubmitChanges();
				return true;
			} else {
				return false;
			}
		}

		public void SetPrimary(int uid, string name)
		{
			var results = (from n in sqlConnector.UserNames
				where n.UserId == uid
				select n);
			if (results.Any()) {
				results.First().Name1 = name;
			} else {
				Name n = new Name();
				n.UserId = uid;
				n.Name1 = name;
				sqlConnector.UserNames.InsertOnSubmit(n);
			}
			Logger.Log("Changed name for " + uid + ".");
			SubmitChanges();
		}

		public string GetNickserv(int uid)
		{
			return (from c in sqlConnector.UserCreds
					where c.UserId == uid
					select c.NsLogin).First();
		}

		#region Match Levels
		private int[] GetMatchesFirstLevel(IrcUser sender)
		{
			return GetUids(sender);
		}
		private int[] GetMatchesSecondLevel(IrcUser sender)
		{
			return (from c in sqlConnector.UserCreds
					where c.Nick == sender.Nick
					&& c.Ident == sender.Ident
					select c.UserId).Distinct().ToArray();
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			return (from c in sqlConnector.UserCreds
					where c.NsLogin == nickserv
					select c.UserId).Distinct().ToArray();
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			return (from c in sqlConnector.UserCreds
					where c.HostMask == sender.Hostmask
					&& c.Ident == sender.Ident
					select c.UserId).Distinct().ToArray();
		}
		#endregion

		/// <summary>
		/// Attempts to retrieve the user ID for a given nickname from the database.
		/// Will not work if other users have used that nickname at some point and that nickname is not in the table of primary nicks.
		/// </summary>
		/// <returns>The user ID of the user if successful, -1 if there were multiple matches, -2 if there were no matches.</returns>
		public int GetIdFromNick(string nick)
		{
			var results = (from n in sqlConnector.UserNames
						   where n.Name1 == nick
						   select n.UserId);

			int count = results.Count();

			if (count == 1) return results.First();
			else if (count > 1) return -1;

			results = (from c in sqlConnector.UserCreds
					   where c.Nick == nick
					   select c.UserId);

			if (count == 1) return results.First();
			else if (count > 1) return -1;
			else return -2;
		}

		delegate int Level();
		public int GetIdFromUser(IrcUser user)
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
				if (res.Length == 1) {
					Logger.Log("Found user id match at level 1 with user id " + res[0]);
					return res[0];
				} else if (res.Length == 0) return l2();
				else return l3();
			};

			return l1();
		}

		public void IncrementEmoticon(string emoticon, int user)
		{
			var matches = from tEmoticon in sqlConnector.Emoticons
						  where tEmoticon.Emoticon1 == emoticon
						  select tEmoticon;
			if (matches.Count() == 0) {
				Emoticon insert = new Emoticon();
				insert.Emoticon1 = emoticon;
				insert.LastUsedBy = user;
				insert.Uses = 1;
				sqlConnector.Emoticons.InsertOnSubmit(insert);
			} else {
				matches.First().Uses++;
				matches.First().LastUsedBy = user;
			}
			Logger.Log("Incremented emoticon count with emoticon: " + emoticon + ".");
			SubmitChanges();
		}

		public void SetVar(string key, int amount)
		{
			var matches = from pair in sqlConnector.KeyValuePairs
						  where pair.Key == key
						  select pair;

			if (matches.Count() == 0) {
				KeyValuePair p = new KeyValuePair();
				p.Key = key;
				p.Value = amount;
				sqlConnector.KeyValuePairs.InsertOnSubmit(p);
				Logger.Log("Inserted keyvaluepair with key: " + key + ".");
			} else {
				matches.First().Value = amount;
				Logger.Log("Changed keyvaluepair with key: " + key + ".");
			}
			SubmitChanges();
		}

		public void IncrementVar(string key, int amount = 1)
		{
			var matches = from pair in sqlConnector.KeyValuePairs
						  where pair.Key == key
						  select pair;

			if (matches.Count() == 0) {
				KeyValuePair p = new KeyValuePair();
				p.Key = key;
				p.Value = amount;
				sqlConnector.KeyValuePairs.InsertOnSubmit(p);
				Logger.Log("Inserted keyvaluepair with key: " + key + ".");
			} else {
				matches.First().Value += amount;
				Logger.Log("Incremented keyvaluepair with key: " + key + ".");
			}
			SubmitChanges();
		}

		/// <summary>
		/// Processes a user's credentials when they change their nickname, adding a new credentials entry when necessary.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNick"></param>
		public void HandleNickChange(IrcUser user, string newNick)
		{
			var newCreds = (from c in sqlConnector.UserCreds
							where c.Nick == newNick
							&& c.Ident == user.Ident
							&& c.HostMask == user.Hostmask
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

		public void IncrementUrl(string url, int user, string usage)
		{
			var matches = from tUrl in sqlConnector.Urls
						  where tUrl.Url1 == url
						  select tUrl;

			if (matches.Count() == 0) {
				Url u = new Url();
				u.LastUsage = usage;
				u.LastUsedBy = user;
				u.Url1 = url;
				u.Uses = 1;
				sqlConnector.Urls.InsertOnSubmit(u);
			} else {
				matches.First().Uses++;
				matches.First().LastUsage = usage;
				matches.First().LastUsedBy = user;
			}
			Logger.Log("Incremented URL count with URL: " + url + ".");
			SubmitChanges();
		}

		public void IncrementWord(string word)
		{
			var matches = from tWord in sqlConnector.Words
						  where tWord.Word1 == word
						  select tWord;

			if (matches.Count() == 0) {
				Word w = new Word();
				w.Word1 = word;
				w.Uses = 1;
				sqlConnector.Words.InsertOnSubmit(w);
			} else {
				matches.First().Uses++;
			}
			Logger.Log("Incremented word count for word: " + word + ".");
			SubmitChanges();
		}

		public void IncrementProfanities(int sender)
		{
			(from s in sqlConnector.UserStats
			 where s.UserId == sender
			 select s).First().Profanities++;

			Logger.Log("Incremented profanities for " + sender + ".");
			SubmitChanges();
		}


		public void IncrementActions(int sender)
		{
			(from s in sqlConnector.UserStats
			 where s.UserId == sender
			 select s).First().Actions++;

			Logger.Log("Incremented actions for " + sender + ".");
			SubmitChanges();
		}

		public void SetNsLogin(int uid, string nickserv)
		{
			(from cred in sqlConnector.UserCreds
			 where cred.UserId == uid
			 select cred).First().NsLogin = nickserv;

			SubmitChanges();
		}
	}
}
