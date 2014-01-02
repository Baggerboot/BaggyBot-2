using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IRCSharp;
using BaggyBot.Database;
using System.Data.Linq;

#if postgresql
using BaggyBot.Database.PostgreSQL;
#endif
#if mssql
using BaggyBot.Database.MS_SQL;
#endif

namespace BaggyBot.DataProcessors
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	public class DataFunctionSet
	{
		private SqlConnector sqlConnector;
		private IrcInterface ircInterface;
		private LockObject Lock;

		public DataFunctionSet(SqlConnector sqlConnector, IrcInterface inter)
		{
			this.sqlConnector = sqlConnector;
			ircInterface = inter;
			Lock = new LockObject();

		}

		public void IncrementLineCount(int uid)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public int ExecuteStatement(string statement)
		{
			return sqlConnector.ExecuteStatement(statement);
		}

		public List<object> ExecuteQuery(string query)
		{
			return sqlConnector.ExecuteQuery(query);
		}

		public void IncrementWordCount(int uid, int words)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public void AddIrcMessage(DateTime time, int? sender, string channel, string nick, string message)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				IrcLog line = new IrcLog();
				line.Time = time;
				line.Sender = sender;
				line.Channel = channel;
				line.Nick = nick;
				line.Message = message;

				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				sqlConnector.IrcLog.InsertOnSubmit(line);
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public List<IrcLog> FindLine(string search)
		{
			List<IrcLog> ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var matches =
					from line in sqlConnector.IrcLog
					where line.Message.ToLower().Contains(search.ToLower())
					select line;
				ret = matches.ToList();
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public List<Quote> FindQuote(string search)
		{
			List<Quote> ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var matches =
					from quote in sqlConnector.Quotes
					where quote.Quote1.ToLower().Contains(search.ToLower())
					select quote;

				int count = matches.Count();

				if (count == 0) {
					ret = null;
				} else {
					ret = matches.ToList();
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public void Snag(IrcMessage message)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				int uid = GetIdFromUser(message.Sender);
				Quote q = new Quote();
				q.Quote1 = message.Message;
				q.UserId = uid;
				q.SnaggedAt = DateTime.Now;

				sqlConnector.Quotes.InsertOnSubmit(q);
				Logger.Log("Added quote for " + message.Sender.Nick + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		public int[] GetUids(IrcUser ircUser)
		{
			int[] ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var results = (from res in sqlConnector.UserCreds
							   where res.Nick == ircUser.Nick
							   && res.Ident == ircUser.Ident
							   && res.HostMask == ircUser.Hostmask
							   select res);

				int[] uids = results.Select(r => r.UserId).ToArray();

				ret = uids.Distinct().ToArray();
			}
			Lock.LockMessage = "None";
			return ret;
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
			int ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
					 select n).Any()) {
					ret = uid;
				} else {
					Name name = new Name();
					name.UserId = uid;
					name.Name1 = user.Nick;

					sqlConnector.UserNames.InsertOnSubmit(name);
					Logger.Log("Added name row for " + user.Nick + ".");
					SubmitChanges();

					ret = uid;
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public void SubmitChanges()
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				sqlConnector.SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		internal bool SetPrimary(string oldName, string newName)
		{
			bool ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var results = (from name in sqlConnector.UserNames
							   where name.Name1 == oldName
							   select name);
				if (results.Any()) {
					results.First().Name1 = newName;
					sqlConnector.SubmitChanges();
					ret = true;
				} else {
					ret = false;
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public void SetPrimary(int uid, string name)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public string GetNickserv(int uid)
		{
			string ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				ret = (from c in sqlConnector.UserCreds
					   where c.UserId == uid
					   select c.NsLogin).First();
			}
			Lock.LockMessage = "None";
			return ret;
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
			int ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var results = (from n in sqlConnector.UserNames
							   where n.Name1 == nick
							   select n.UserId);

				int count = results.Count();

				if (count == 1) ret = results.First();
				else if (count > 1) ret = -1;
				else {
					results = (from c in sqlConnector.UserCreds
							   where c.Nick == nick
							   select c.UserId);

					if (count == 1) ret = results.First();
					else if (count > 1) ret = -1;
					else ret = -2;
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		delegate int Level();
		public int GetIdFromUser(IrcUser user)
		{
			int ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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

				ret = l1();
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public void IncrementEmoticon(string emoticon, int user)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public void SetVar(string key, int amount)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public void IncrementVar(string key, int amount = 1)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		/// <summary>
		/// Processes a user's credentials when they change their nickname, adding a new credentials entry when necessary.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNick"></param>
		public void HandleNickChange(IrcUser user, string newNick)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var newCreds = (from c in sqlConnector.UserCreds
								where c.Nick == newNick
								&& c.Ident == user.Ident
								&& c.HostMask == user.Hostmask
								select c);

				int count = newCreds.Count();

				// Multiple credentials rows were returned for the new user. This is most likely an error.
				if (count > 1) {
					Logger.Log(String.Format("Multiple credentials found for combination: nick={0}, ident={1}, hostmask={2}", newNick, user.Ident, user.Hostmask), LogLevel.Warning);
				} else if (count == 0) {
					int[] uids = GetUids(user);
					// It looks like this user does not have a database entry yet, so we can ignore them.
					if (uids.Length == 0) {
						Logger.Log("Dropped nick change event for " + user.Nick + " to " + newNick + " - they do not have a database entry yet.", LogLevel.Debug);
					} else if (uids.Length > 1) {
						Logger.Log("Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
					} else {
						string nickserv = GetNickserv(uids[0]);
						AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), nickserv, uids[0]);
					}
				} else if (count == 1) {
					Logger.Log("Nick change event for " + user.Nick + " to " + newNick + " ignored - they already have a database entry", LogLevel.Debug);
				}
			}
			Lock.LockMessage = "None";
		}

		public void IncrementUrl(string url, int user, string usage)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public void IncrementWord(string word)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
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
			Lock.LockMessage = "None";
		}

		public void IncrementProfanities(int sender)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				(from s in sqlConnector.UserStats
				 where s.UserId == sender
				 select s).First().Profanities++;

				Logger.Log("Incremented profanities for " + sender + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void IncrementActions(int sender)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				(from s in sqlConnector.UserStats
				 where s.UserId == sender
				 select s).First().Actions++;

				Logger.Log("Incremented actions for " + sender + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void SetNsLogin(int uid, string nickserv)
		{
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				(from cred in sqlConnector.UserCreds
				 where cred.UserId == uid
				 select cred).First().NsLogin = nickserv;

				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public DateTime? GetLastSnaggedLine(int userId)
		{
			DateTime? ret;
			lock (Lock) {
				Lock.LockMessage = Tools.MiscTools.GetCurrentMethod();
				var data = (from quot in sqlConnector.Quotes
							where quot.UserId == userId
							&& quot.SnaggedAt != null
							select quot
				).OrderBy(q => q.SnaggedAt);
				if (data.ToList().Count != 0) {
					ret = data.Last().SnaggedAt;
					var item = data.Last();
					ret = item.SnaggedAt;
				} else {
					Logger.Log("No last snagged line available for user #" + userId);
					ret = null;
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		internal void IncrementUserStatistics(UserStatistics changes)
		{
			var matches = from stat in sqlConnector.UserStats
						  where stat.UserId == changes.UserId
						  select stat;
			var match = matches.First();
			match.Actions += changes.Actions;
			match.Lines += changes.Lines;
			match.Profanities += changes.Profanities;
			match.Words += changes.Words;
			SubmitChanges();
			Logger.Log("Userstats incremented for user #{0}: {1} action(s), {2} line(s), {3} word(s), {4} swear(s)", LogLevel.Debug, true, changes.UserId, changes.Actions, changes.Lines, changes.Words, changes.Profanities);
		}
	}
}
