using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using BaggyBot.Tools;
using IRCSharp;
using BaggyBot.Database;
using IRCSharp.IRC;
using BaggyBot.Database.Model;

namespace BaggyBot.DataProcessors
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	class DataFunctionSet
	{
		private readonly SqlConnector sqlConnector;
		private readonly IrcInterface ircInterface;
		private readonly LockObject Lock;
		public ConnectionState ConnectionState { get { return sqlConnector.ConnectionState; } }

		public DataFunctionSet(SqlConnector sqlConnector, IrcInterface inter)
		{
			this.sqlConnector = sqlConnector;
			ircInterface = inter;
			Lock = new LockObject();

		}

		public void IncrementLineCount(int uid)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();

				var matches =
					from stat in sqlConnector.UserStatistics
					where stat.UserId == uid
					select stat;

				if (matches.Count() != 0) {
					var match = matches.First();
					match.Lines++;
					Logger.Log(this, "Incremented lines for " + uid + ".");
					SubmitChanges();
				} else {
					var nstat = new UserStatistic {UserId = uid, Lines = 1};
					sqlConnector.Insert(nstat);
					Logger.Log(this, "Created new stats row for " + uid + ".");
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();

				var statement = "UPDATE dbo.UserStatistic SET words = words + 1 WHERE user_id = " + uid + ";";

				sqlConnector.ExecuteStatement(statement);

				/*var matches =
					from stat in sqlConnector.UserStatistics
					where stat.Id == uid
					select stat;

				var match = matches.First();
				int originalWords = match.Words;
				match.Words += words;

				Logger.Log(this, "Incremented words for " + uid + ".");
				SubmitChanges();*/
			}
			Lock.LockMessage = "None";
		}

		public void AddIrcMessage(DateTime time, int? sender, string channel, string nick, string message)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var line = new IrcLog();
				line.Time = time;
				line.SenderId = sender;
				line.Channel = channel;
				line.Nick = nick;
				line.Message = message;

				sqlConnector.Insert(line);
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public List<IrcLog> FindLine(string search, string username = null)
		{

			List<IrcLog> ret;
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				IQueryable<IrcLog> matches;
				if (username == null) {
					matches =
						from line in sqlConnector.IrcLog
						where line.Message.ToLower().Contains(search.ToLower())
						select line;
				} else {
					var uid = GetIdFromNick(username);
					matches =
						from line in sqlConnector.IrcLog
						where line.Message.ToLower().Contains(search.ToLower())
						&& line.SenderId == uid
						select line;
				}
				ret = matches.ToList();
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public List<Quote> FindQuote(string search)
		{
			List<Quote> ret;
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var matches =
					from quote in sqlConnector.Quotes
					where quote.Text.ToLower().Contains(search.ToLower())
					select quote;

				var count = matches.Count();

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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var uid = GetIdFromUser(message.Sender);
				var q = new Quote();
				q.Text = message.Message;
				q.UserId = uid;
				q.TakenAt = DateTime.Now;

				sqlConnector.Insert(q);
				Logger.Log(this, "Added quote for " + message.Sender.Nick + ".");
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var results = (from res in sqlConnector.UserCredentials
							   where res.Nick == ircUser.Nick
							   && res.Ident == ircUser.Ident
							   && res.Hostmask == ircUser.Hostmask
							   select res);

				var uids = results.Select(r => r.UserId).ToArray();

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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				if (uid == -1) {
					var results = (from c in sqlConnector.UserCredentials
								   select c.UserId);
					if (!results.Any()) {
						uid = 1;
					} else {
						uid = results.Max() + 1;
					}
				}

				/*if (nickserv == null) nickserv = "NULL";
				else nickserv = Safe(nickserv);*/

				var cred = new UserCredential
				{
					Nick = user.Nick,
					Ident = user.Ident,
					Hostmask = user.Hostmask,
					NickservLogin = nickserv,
					UserId = uid
				};

				sqlConnector.Insert(cred);
				Logger.Log(this, "Addecd credentials row for " + user.Nick + ".");
				SubmitChanges();

				if ((from n in sqlConnector.Users
					 where n.Id == uid
					 select n).Any()) {
					ret = uid;
				} else {
					var name = new User();
					name.Id = uid;
					name.Name = user.Nick;

					sqlConnector.Insert(name);
					Logger.Log(this, "Added name row for " + user.Nick + ".");
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				sqlConnector.SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		internal bool SetPrimary(string oldName, string newName)
		{
			bool ret;
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var results = (from name in sqlConnector.Users
							   where name.Name == oldName
							   select name);
				if (results.Any()) {
					results.First().Name = newName;
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var results = (from n in sqlConnector.Users
							   where n.Id == uid
							   select n);
				if (results.Any()) {
					results.First().Name = name;
				} else {
					var n = new User();
					n.Id = uid;
					n.Name = name;
					sqlConnector.Insert(n);
				}
				Logger.Log(this, "Changed name for " + uid + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public string GetNickserv(int uid)
		{
			string ret;
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				ret = (from c in sqlConnector.UserCredentials
					   where c.UserId == uid
					   select c.NickservLogin).First();
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
			return (from c in sqlConnector.UserCredentials
					where c.Nick == sender.Nick
					&& c.Ident == sender.Ident
					select c.UserId).Distinct().ToArray();
		}
		private int[] GetMatchesThirdLevel(string nickserv)
		{
			return (from c in sqlConnector.UserCredentials
					where c.NickservLogin == nickserv
					select c.UserId).Distinct().ToArray();
		}
		private int[] GetMatchesFourthLevel(IrcUser sender)
		{
			return (from c in sqlConnector.UserCredentials
					where c.Hostmask == sender.Hostmask
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var results = (from n in sqlConnector.Users
							   where n.Name.ToLower() == nick.ToLower()
							   select n.Id);

				var count = results.Count();

				if (count == 1) ret = results.First();
				else if (count > 1) {
					ret = -1;
				} else {
					results = (from c in sqlConnector.UserCredentials
							   where c.Nick.ToLower() == nick.ToLower()
							   select c.UserId);

					if (count == 1) ret = results.First();
					else if (count > 1) {
						ret = -1;
					} else ret = -2;
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				// Check for a match with ident and hostmask, but only if nickserv fails
				Level l4 = () =>
				{
					var res = GetMatchesFourthLevel(user);
					if (res.Length == 0) {
						var uid = AddCredCombination(user);
						return uid;
					}
					if (res.Length == 1) {
						AddCredCombination(user, null, res[0]);
						return res[0];
					}
					return -1;
				};

				// Check for a nickserv match
				Level l3 = () =>
				{
					var nickserv = ircInterface.DoNickservCall(user.Nick);

					if (nickserv == null) // No nickserv info available, try a level 4 instead
				{
						return l4();
					}

					var res = GetMatchesThirdLevel(nickserv);
					if (res.Length == 1) { // Match found trough NickServ, add a credentials combinations for easy access next time.
						AddCredCombination(user, nickserv, res[0]);
						return res[0];
					}
					if (res.Length == 0) { // No matches found, not even with NickServ. Most likely a new user, unless you change your hostname and ident, and log in with a different nick than the one you logged out with.
						var uid = AddCredCombination(user, nickserv);
						return uid;
					} // Multiple people registered using the same NickServ account? That's most likely an error.
					return -1;
				};

				// Check for a match with nick and ident
				Level l2 = () =>
				{
					var res = GetMatchesSecondLevel(user);
					if (res.Length == 1) {
						AddCredCombination(user, null, res[0]);
						return res[0];
					}
					return l3();
				};

				// Check for a match with nick, ident and hostmask
				Level l1 = () =>
				{
					var res = GetMatchesFirstLevel(user);
					if (res.Length == 1) {
						Logger.Log(this, "Found user id match at level 1 with user id " + res[0]);
						return res[0];
					}
					if (res.Length == 0) return l2();
					return l3();
				};

				ret = l1();
			}
			Lock.LockMessage = "None";
			return ret;
		}

		public void IncrementEmoticon(string emoticon, int user)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tEmoticon in sqlConnector.Emoticons
							  where tEmoticon.Emoticon == emoticon
							  select tEmoticon;
				if (!matches.Any()) {
					var insert = new UsedEmoticon();
					insert.Emoticon = emoticon;
					insert.LastUsedById = user;
					insert.Uses = 1;
					sqlConnector.Insert(insert);
				} else {
					matches.First().Uses++;
					matches.First().LastUsedById = user;
				}
				Logger.Log(this, "Incremented emoticon count with emoticon: " + emoticon + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void SetVar(string key, int amount)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in sqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any()) {
					var p = new KeyValuePair();
					p.Key = key;
					p.Value = amount;
					sqlConnector.Insert(p);
					Logger.Log(this, "Inserted keyvaluepair with key: " + key + ".");
				} else {
					matches.First().Value = amount;
					Logger.Log(this, "Changed keyvaluepair with key: " + key + ".");
				}
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void IncrementVar(string key, int amount = 1)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in sqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any()) {
					var p = new KeyValuePair();
					p.Key = key;
					p.Value = amount;
					sqlConnector.Insert(p);
					Logger.Log(this, "Inserted keyvaluepair with key: " + key + ".");
				} else {
					matches.First().Value += amount;
					//Logger.Log(this, "Incremented keyvaluepair with key: " + key + ".");
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
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var newCreds = (from c in sqlConnector.UserCredentials
								where c.Nick == newNick
								&& c.Ident == user.Ident
								&& c.Hostmask == user.Hostmask
								select c);

				var count = newCreds.Count();

				// Multiple credentials rows were returned for the new user. This is most likely an error.
				if (count > 1) {
					Logger.Log(this, String.Format("Multiple credentials found for combination: nick={0}, ident={1}, hostmask={2}", newNick, user.Ident, user.Hostmask), LogLevel.Warning);
				} else if (count == 0) {
					var uids = GetUids(user);
					// It looks like this user does not have a database entry yet, so we can ignore them.
					if (uids.Length == 0) {
						Logger.Log(this, "Dropped nick change event for " + user.Nick + " to " + newNick + " - they do not have a database entry yet.");
					} else if (uids.Length > 1) {
						Logger.Log(this, "Unable to handle nick change for " + user.Nick + " to " + newNick + ": Invalid amount of Uids received: " + uids.Length, LogLevel.Warning);
					} else {
						var nickserv = GetNickserv(uids[0]);
						AddCredCombination(new IrcUser(newNick, user.Ident, user.Hostmask), nickserv, uids[0]);
					}
				} else if (count == 1) {
					Logger.Log(this, "Nick change event for " + user.Nick + " to " + newNick + " ignored - they already have a database entry");
				}
			}
			Lock.LockMessage = "None";
		}

		public void IncrementUrl(string url, int user, string usage)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tUrl in sqlConnector.Urls
							  where tUrl.Url == url
							  select tUrl;

				if (!matches.Any()) {
					var u = new LinkedUrl();
					u.LastUsage = usage;
					u.LastUsedById = user;
					u.Url = url;
					u.Uses = 1;
					sqlConnector.Insert(u);
				} else {
					matches.First().Uses++;
					matches.First().LastUsage = usage;
					matches.First().LastUsedById = user;
				}
				Logger.Log(this, "Incremented URL count with URL: " + url + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void IncrementWord(string word)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();

				var statement = "UPDATE dbo.words SET uses = uses + 1 WHERE word = '" + word + "'; INSERT INTO dbo.words (word, uses) SELECT '" + word + "', 1 WHERE NOT EXISTS (SELECT 1 FROM dbo.words WHERE word = '" + word + "');";

				sqlConnector.ExecuteStatement(statement);
				//Logger.Log(this, "Incremented word count for word: " + word + ".");

				/*var matches = from tWord in sqlConnector.Words
							  where tWord.Word == word
							  select tWord;

				if (matches.Count() == 0) {
					Word w = new Word();
					w.Word = word;
					w.Uses = 1;
					sqlConnector.Insert(w);
				} else {
					matches.First().Uses++;
				}
				Logger.Log(this, "Incremented word count for word: " + word + ".");
				SubmitChanges();*/
			}
			Lock.LockMessage = "None";
		}

		public void IncrementProfanities(int sender)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				(from s in sqlConnector.UserStatistics
				 where s.UserId == sender
				 select s).First().Profanities++;

				Logger.Log(this, "Incremented profanities for " + sender + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void IncrementActions(int sender)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				(from s in sqlConnector.UserStatistics
				 where s.UserId == sender
				 select s).First().Actions++;

				Logger.Log(this, "Incremented actions for " + sender + ".");
				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public void SetNsLogin(int uid, string nickserv)
		{
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				(from cred in sqlConnector.UserCredentials
				 where cred.UserId == uid
				 select cred).First().NickservLogin = nickserv;

				SubmitChanges();
			}
			Lock.LockMessage = "None";
		}

		public DateTime? GetLastSnaggedLine(int userId)
		{
			DateTime? ret;
			lock (Lock) {
				Lock.LockMessage = MiscTools.GetCurrentMethod();
				var data = (from quot in sqlConnector.Quotes
							where quot.UserId == userId
							&& quot.TakenAt != null
							select quot
				).OrderBy(q => q.TakenAt);
				if (data.ToList().Count != 0) {
					var item = data.Last();
					ret = item.TakenAt;
				} else {
					Logger.Log(this, "No last snagged line available for user #" + userId);
					ret = null;
				}
			}
			Lock.LockMessage = "None";
			return ret;
		}

		internal void IncrementUserStatistic(UserStatistic changes)
		{
			var matches = from stat in sqlConnector.UserStatistics
						  where stat.UserId == changes.UserId
						  select stat;
			var match = matches.First();
			match.Actions += changes.Actions;
			match.Lines += changes.Lines;
			match.Profanities += changes.Profanities;
			match.Words += changes.Words;
			SubmitChanges();
			Logger.Log(this, "Userstats incremented for user #{0}: {1} action(s), {2} line(s), {3} word(s), {4} swear(s)", LogLevel.Debug, true, changes.UserId, changes.Actions, changes.Lines, changes.Words, changes.Profanities);
		}

		internal IOrderedEnumerable<Topic>FindTopics(int userId, string channel)
		{
			Logger.Log(this, "finding words");
			var words = from word in sqlConnector.Words
						where word.Uses > 1
					   select word;
			Logger.Log(this, "building dictionary");
			var globalWordCount = words.ToDictionary(word => word.Word, word => word.Uses);

			Logger.Log(this, "finding sentences");
			var userSentencesQuery = (from sentence in sqlConnector.IrcLog
								where sentence.SenderId == userId
								&& sentence.Channel == channel
								select sentence.Message).ToList();

			if (userSentencesQuery.Count == 0) {
				return null;
			}

			Logger.Log(this, "filtering commands");
			var userSentences = userSentencesQuery.ToList().Where(s => !s.StartsWith("-")).ToList();

			Logger.Log(this, "finding user words");
			IEnumerable<string> userWords = new List<string>();
			foreach (var sentence in userSentences) {
				userWords = userWords.Concat(sentence.Split(' '));
			}

			Logger.Log(this, "grouping user words");
			var userWordCount = userWords.GroupBy(word => word).Select(group => Tuple.Create(group.Key, group.Count()));

			Logger.Log(this, "calculating usage difference of " + userWordCount.Count() + " words");

			var topics = (from pair in userWordCount 
						  where globalWordCount.ContainsKey(pair.Item1) 
						  let userCount = pair.Item2 
						  let globalCount = globalWordCount[pair.Item1] 
						  where userCount <= globalCount 
						  select new Topic(pair.Item1, userCount, globalCount, userCount/(double) globalCount)).ToList();

			Logger.Log(this, "\ncalculating average usage difference");
			var avgDifference = topics.Average(topic => topic.Score);
			var maxGlobalCount = globalWordCount.Max(pair => pair.Value);
			var avgMultiplier = 1 / avgDifference;

			Logger.Log(this, "multiplying difference with multiplier");
			foreach (var topic in topics) {
				topic.Normalize(avgMultiplier);
				topic.ScoreByOccurrence(maxGlobalCount);
			}

			return topics.OrderByDescending(pair => pair.Score);
		}
	}
	class Topic
	{
		public string Name;
		public int UserCount;
		public int GlobalCount;
		public double Score;

		public void Normalize(double multiplier)
		{
			Score = Score * multiplier;
		}
		public void ScoreByOccurrence(int maxGlobalCount)
		{

			var maxGlobalPart =  (GlobalCount / (double)maxGlobalCount);

			Score = Score + (maxGlobalPart * 4);

			if (UserCount != GlobalCount && GlobalCount < (maxGlobalCount / 2.0)) {
				Score += 1.5;
			}
			
		}

		public Topic(string name, int userCount, int globalCount, double difference)
		{
			Name = name;
			UserCount = userCount;
			GlobalCount = globalCount;
			Score = difference;
		}
	}
}
