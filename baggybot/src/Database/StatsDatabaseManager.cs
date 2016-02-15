using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Database.Model;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Tools;
using LinqToDB;

namespace BaggyBot.Database
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	public class StatsDatabaseManager : IDisposable
	{
		private readonly SqlConnector sqlConnector;
		private readonly LockObject lockObj;
		private readonly bool allowNickservLookup;
		public ConnectionState ConnectionState => sqlConnector.ConnectionState;

		public StatsDatabaseManager(SqlConnector sqlConnector, bool allowNickservLookup)
		{
			this.allowNickservLookup = allowNickservLookup;
			this.sqlConnector = sqlConnector;
			lockObj = new LockObject();
		}

		private void Update<T>(T match) where T : Poco
		{
			sqlConnector.Update(match);
		}

		public void IncrementLineCount(int uid)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();

				var matches =
					from stat in sqlConnector.UserStatistics
					where stat.UserId == uid
					select stat;

				if (matches.Count() != 0)
				{
					var match = matches.First();
					match.Lines++;
					Update(match);
				}
				else
				{
					var nstat = new UserStatistic { UserId = uid, Lines = 1 };
					sqlConnector.Insert(nstat);
					Logger.Log(this, "Created new stats row for " + uid + ".");
				}
			}
			lockObj.LockMessage = "None";
		}

		public int ExecuteStatement(string statement)
		{
			int result;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				result = sqlConnector.ExecuteStatement(statement);
			}
			lockObj.LockMessage = "None";
			return result;
		}

		public List<object[]> ExecuteQuery(string query)
		{
			List<object[]> results = null;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				results = sqlConnector.ExecuteQuery(query);
			}
			lockObj.LockMessage = "None";
			return results;
		}

		public void IncrementWordCount(int uid, int words)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();

				sqlConnector.UserStatistics
					.Where(stat => stat.UserId == uid)
					.Set(stat => stat.Words, stat => stat.Words + 1)
					.Update();
			}
			lockObj.LockMessage = "None";
		}

		public void AddIrcMessage(DateTime time, int? sender, string channel, string nick, string message)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var line = new IrcLog
				{
					SentAt = time,
					SenderId = sender,
					Channel = channel,
					Nick = nick,
					Message = message
				};

				sqlConnector.Insert(line);
			}
			lockObj.LockMessage = "None";
		}

		public List<IrcLog> FindLine(string search, string username = null)
		{
			List<IrcLog> ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				IQueryable<IrcLog> matches;
				if (username == null)
				{
					matches =
						from line in sqlConnector.IrcLog
						where line.Message.ToLower().Contains(search.ToLower())
						select line;
				}
				else
				{
					var uid = GetIdFromNick(username);
					matches =
						from line in sqlConnector.IrcLog
						where line.Message.ToLower().Contains(search.ToLower())
						&& line.SenderId == uid
						select line;
				}
				ret = matches.ToList();
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		public List<Quote> FindQuote(string search)
		{
			List<Quote> ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches =
					from quote in sqlConnector.Quotes
					where quote.Text.ToLower().Contains(search.ToLower())
					select quote;

				var count = matches.Count();
				// TODO: it probably makes more sense to return an empty list instead of null
				ret = count == 0 ? null : matches.ToList();
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		internal void Snag(IrcMessage message)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var uid = GetIdFromUser(message.Sender);
				var q = new Quote();
				q.Text = message.Message;
				q.AuthorId = uid;
				q.TakenAt = DateTime.Now;

				sqlConnector.Insert(q);
				Logger.Log(this, "Added quote for " + message.Sender.Nick + ".");
			}
			lockObj.LockMessage = "None";
		}

		/// <summary>
		/// Attempts to find the User IDs for an IRC user, if the provided credentials combination already exists.
		/// </summary>
		/// <returns>a list of User ID matches</returns>
		public int[] GetUids(IrcUser ircUser)
		{
			int[] ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from res in sqlConnector.UserCredentials
							  where res.Nick == ircUser.Nick
									&& res.Ident == ircUser.Ident
									&& res.Hostmask == ircUser.Hostmask
							  select res.UserId;

				ret = results.Distinct().ToArray();
			}
			lockObj.LockMessage = "None";
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
			// TODO: Split this up into AddCredCombination and AddUser
			int ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				if (uid == -1)
				{
					var results = from c in sqlConnector.UserCredentials
								  select c.UserId;
					if (!results.Any())
					{
						uid = 1;
					}
					else
					{
						uid = results.Max() + 1;
					}
				}

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

				if ((from n in sqlConnector.Users
					 where n.Id == uid
					 select n).Any())
				{
					ret = uid;
				}
				else
				{
					var name = new User();
					name.Id = uid;
					name.Name = user.Nick;

					sqlConnector.Insert(name);
					Logger.Log(this, "Added name row for " + user.Nick + ".");

					ret = uid;
				}
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		internal bool SetPrimary(string oldName, string newName)
		{
			bool ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from name in sqlConnector.Users
							  where name.Name == oldName
							  select name;
				if (results.Any())
				{
					results.First().Name = newName;
					Update(results.First());
					ret = true;
				}
				else
				{
					ret = false;
				}
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		public void SetPrimary(int uid, string name)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from n in sqlConnector.Users
							  where n.Id == uid
							  select n;
				if (results.Any())
				{
					results.First().Name = name;
					Update(results.First());
				}
				else
				{
					var n = new User
					{
						Id = uid,
						Name = name
					};
					sqlConnector.Insert(n);
				}
				Logger.Log(this, $"Changed name for {uid} to {name}.");
			}
			lockObj.LockMessage = "None";
		}

		public string GetNickserv(int uid)
		{
			string ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				ret = (from c in sqlConnector.UserCredentials
					   where c.UserId == uid
					   select c.NickservLogin).First();
			}
			lockObj.LockMessage = "None";
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
			lock (lockObj)
			{
				// Look for a nickname match first
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from n in sqlConnector.Users
							  where string.Equals(n.Name, nick, StringComparison.InvariantCultureIgnoreCase)
							  select n.Id;

				var count = results.Count();

				if (count == 1) ret = results.First();
				else if (count > 1)
				{
					ret = -1;
				}
				else
				{
					// If a nickname match fails, look for a credentials match instead
					results = from c in sqlConnector.UserCredentials
							  where c.Nick.ToLower() == nick.ToLower()
							  select c.UserId;

					if (count == 1) ret = results.First();
					else if (count > 1)
					{
						ret = -1;
					}
					else ret = -2;
				}
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		private delegate int Level();
		public int GetIdFromUser(IrcUser user)
		{
			// TODO: Improve user matching code
			int ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				// Check for a match with ident and hostmask, but only if nickserv fails
				Level l4 = () =>
				{
					var res = GetMatchesFourthLevel(user);
					if (res.Length == 0)
					{
						var uid = AddCredCombination(user);
						return uid;
					}
					if (res.Length == 1)
					{
						AddCredCombination(user, null, res[0]);
						return res[0];
					}
					return -1;
				};

				// Check for a nickserv match
				Level l3 = () =>
				{
					var nickserv = allowNickservLookup ? user.Client.NickservLookup(user.Nick) : null;

					if (nickserv == null) // No nickserv info available, try a level 4 instead
					{
						return l4();
					}

					var res = GetMatchesThirdLevel(nickserv.AccountName);
					if (res.Length == 1)
					{ // Match found trough NickServ, add a credentials combinations for easy access next time.
						AddCredCombination(user, nickserv.AccountName, res[0]);
						return res[0];
					}
					if (res.Length == 0)
					{ // No matches found, not even with NickServ. Most likely a new user, unless you change your hostname and ident, and log in with a different nick than the one you logged out with.
						var uid = AddCredCombination(user, nickserv.AccountName);
						return uid;
					} // Multiple people registered using the same NickServ account? That's most likely an error.
					return -1;
				};

				// Check for a match with nick and ident
				Level l2 = () =>
				{
					var res = GetMatchesSecondLevel(user);
					if (res.Length == 1)
					{
						AddCredCombination(user, null, res[0]);
						return res[0];
					}
					return l3();
				};

				// Check for a match with nick, ident and hostmask
				Level l1 = () =>
				{
					var res = GetMatchesFirstLevel(user);
					if (res.Length == 1)
					{
						Logger.Log(this, "Found user id match at level 1 with user id " + res[0]);
						return res[0];
					}
					if (res.Length == 0) return l2();
					return l3();
				};

				ret = l1();
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		public void IncrementEmoticon(string emoticon, int user)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tEmoticon in sqlConnector.Emoticons
							  where tEmoticon.Emoticon == emoticon
							  select tEmoticon;
				if (!matches.Any())
				{
					var insert = new UsedEmoticon
					{
						Emoticon = emoticon,
						LastUsedById = user,
						Uses = 1
					};
					sqlConnector.Insert(insert);
				}
				else
				{
					matches.First().Uses++;
					matches.First().LastUsedById = user;
					Update(matches.First());
				}
				Logger.Log(this, "Incremented emoticon count with emoticon: " + emoticon + ".");
			}
			lockObj.LockMessage = "None";
		}

		public void SetVar(string key, int amount)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in sqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any())
				{
					var p = new KeyValuePair
					{
						Key = key,
						Value = amount
					};
					sqlConnector.Insert(p);
					Logger.Log(this, "Inserted keyvaluepair with key: " + key + ".");
				}
				else
				{
					matches.First().Value = amount;
					Update(matches.First());
					Logger.Log(this, "Changed keyvaluepair with key: " + key + ".");
				}
			}
			lockObj.LockMessage = "None";
		}

		public void IncrementVar(string key, int amount = 1)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in sqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any())
				{
					var p = new KeyValuePair
					{
						Key = key,
						Value = amount
					};
					sqlConnector.Insert(p);
					Logger.Log(this, "Inserted keyvaluepair with key: " + key + ".");
				}
				else
				{
					matches.First().Value += amount;
					Update(matches.First());
				}
			}
			lockObj.LockMessage = "None";
		}

		/// <summary>
		/// Processes a user's credentials when they change their nickname, adding a new credentials entry when necessary.
		/// </summary>
		public void HandleNickChange(IrcUser user, string newNick)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var newCreds = from c in sqlConnector.UserCredentials
							   where c.Nick == newNick
									 && c.Ident == user.Ident
									 && c.Hostmask == user.Hostmask
							   select c;

				var count = newCreds.Count();

				// Multiple credentials rows were returned for the new user. This is most likely an error.
				if (count > 1)
				{
					Logger.Log(this, $"Multiple credentials found for combination: nick={newNick}, ident={user.Ident}, hostmask={user.Hostmask}", LogLevel.Warning);
				}
				else if (count == 0)
				{
					var uids = GetUids(user);
					// It looks like this user does not have a database entry yet, so we can ignore them.
					if (uids.Length == 0)
					{
						Logger.Log(this, $"Dropped nick change event for {user.Nick} to {newNick} - they do not have a database entry yet.");
					}
					else if (uids.Length > 1)
					{
						Logger.Log(this, $"Unable to handle nick change for {user.Nick} to {newNick}: Invalid amount of Uids received: {uids.Length}", LogLevel.Warning);
					}
					{
						var nickserv = GetNickserv(uids[0]);
						AddCredCombination(new IrcUser(user.Client, newNick, user.Ident, user.Hostmask), nickserv, uids[0]);
					}
				}
				else if (count == 1)
				{
					Logger.Log(this, $"Nick change event for {user.Nick} to {newNick} ignored - they already have a database entry");
				}
			}
			lockObj.LockMessage = "None";
		}

		public void UpsertMiscData(string type, string key, string value)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in sqlConnector.MiscData
							  where pair.Type == type
									&& pair.Key == key
							  select pair;
				if (matches.Any())
				{
					matches.Set(data => data.Value, () => value).Update();
				}
				else
				{
					sqlConnector.Insert(new MiscData
					{
						Type = type,
						Key = key,
						Value = value,
						Enabled = true
					});
				}
			}
			lockObj.LockMessage = "None";
		}

		public void IncrementUrl(string url, int user, string usage)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tUrl in sqlConnector.LinkedUrls
							  where tUrl.Url == url
							  select tUrl;

				if (!matches.Any())
				{
					var u = new LinkedUrl
					{
						LastUsage = usage,
						LastUsedById = user,
						Url = url,
						Uses = 1
					};
					sqlConnector.Insert(u);
				}
				else
				{
					matches.First().Uses++;
					matches.First().LastUsage = usage;
					matches.First().LastUsedById = user;
					Update(matches.First());
				}
				Logger.Log(this, "Incremented URL count with URL: " + url + ".");
			}
			lockObj.LockMessage = "None";
		}

		public void IncrementWord(string word)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();

				var matches = from usedWord in sqlConnector.Words
							  where usedWord.Word == word
							  select usedWord;

				if (matches.Any())
				{
					var match = matches.First();
					match.Uses++;
					Update(match);
				}
				else
				{
					sqlConnector.Insert(new UsedWord
					{
						Uses = 1,
						Word = word
					});
				}
			}
			lockObj.LockMessage = "None";
		}

		public void IncrementProfanities(int sender)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var stat = (from s in sqlConnector.UserStatistics
							where s.UserId == sender
							select s).First();
				stat.Profanities++;
				Update(stat);
				Logger.Log(this, "Incremented profanities for " + sender + ".");
			}
			lockObj.LockMessage = "None";
		}

		public void IncrementActions(int sender)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var stat = (from s in sqlConnector.UserStatistics
							where s.UserId == sender
							select s).First();
				stat.Actions++;
				Update(stat);
				Logger.Log(this, "Incremented actions for " + sender + ".");
			}
			lockObj.LockMessage = "None";
		}

		public void SetNsLogin(int uid, string nickserv)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var cred = (from c in sqlConnector.UserCredentials
							where c.UserId == uid
							select c).First();
				cred.NickservLogin = nickserv;
				Update(cred);
			}
			lockObj.LockMessage = "None";
		}

		public DateTime? GetLastQuotedLine(int userId)
		{
			DateTime? ret;
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var data = from quot in sqlConnector.Quotes
						   where quot.AuthorId == userId
								 && quot.TakenAt != null
						   orderby quot.TakenAt descending
						   select quot;

				if (data.ToList().Count != 0)
				{
					var item = data.First();
					ret = item.TakenAt;
				}
				else
				{
					ret = null;
				}
			}
			lockObj.LockMessage = "None";
			return ret;
		}

		internal void IncrementUserStatistic(UserStatistic changes)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from stat in sqlConnector.UserStatistics
							  where stat.UserId == changes.UserId
							  select stat;
				var match = matches.First();
				match.Actions += changes.Actions;
				match.Lines += changes.Lines;
				match.Profanities += changes.Profanities;
				match.Words += changes.Words;
				Update(match);
				Logger.Log(this, $"Userstats incremented for user #{changes.UserId}: {changes.Actions} action(s), {changes.Lines} line(s), {changes.Words} word(s), {changes.Profanities} swear(s)");
			}
		}

		internal IOrderedEnumerable<Topic> FindTopics(int userId, string channel)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
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

				if (userSentencesQuery.Count == 0)
				{
					return null;
				}

				Logger.Log(this, "filtering commands");
				var userSentences = userSentencesQuery.ToList().Where(s => !s.StartsWith("-")).ToList();

				Logger.Log(this, "finding user words");
				IEnumerable<string> userWords = new List<string>();
				foreach (var sentence in userSentences)
				{
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
							  select new Topic(pair.Item1, userCount, globalCount, userCount / (double)globalCount)).ToList();

				Logger.Log(this, "calculating average usage difference");
				var avgDifference = topics.Average(topic => topic.Score);
				var maxGlobalCount = globalWordCount.Max(pair => pair.Value);
				var avgMultiplier = 1 / avgDifference;

				Logger.Log(this, "multiplying difference with multiplier");
				foreach (var topic in topics)
				{
					topic.Normalise(avgMultiplier);
					topic.ScoreByOccurrence(maxGlobalCount);
				}
				lockObj.LockMessage = "None";
				return topics.OrderByDescending(pair => pair.Score);
			}
		}

		public string GetMiscData(string type, string key)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from pair in sqlConnector.MiscData
							  where pair.Type == type
									&& pair.Key == key
							  select pair.Value;
				if (results.Count() > 1)
				{
					lockObj.LockMessage = "None";
					throw new InvalidOperationException("Multiple values were returned for a single type-key combination.");
				}
				else if (!results.Any())
				{
					lockObj.LockMessage = "None";
					throw new InvalidOperationException($"Value for type {type}, key {key} not found.");
				}
				else
				{
					lockObj.LockMessage = "None";
					return results.First();
				}
			}
		}

		public bool MiscDataContainsKey(string type, string key)
		{
			lock (lockObj)
			{
				lockObj.LockMessage = MiscTools.GetCurrentMethod();
				return (from pair in sqlConnector.MiscData
						where pair.Type == type
							  && pair.Key == key
						select pair).Any();
			}
		}

		public void Dispose()
		{
			sqlConnector.Dispose();
		}
	}

	internal class Topic
	{
		public string Name { get; }
		public int UserCount { get; }
		public int GlobalCount { get; }
		public double Score { get; private set; }

		public void Normalise(double multiplier)
		{
			Score = Score * multiplier;
		}

		public void ScoreByOccurrence(int maxGlobalCount)
		{
			if (UserCount != GlobalCount && GlobalCount < maxGlobalCount / 2.0)
			{
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
