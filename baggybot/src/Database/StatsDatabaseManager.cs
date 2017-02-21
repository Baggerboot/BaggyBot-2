using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Database.Model;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Tools;
using LinqToDB;
using Mono.CSharp.Linq;

namespace BaggyBot.Database
{
	/// <summary>
	/// Provides an abstraction layer for commonly used database interactions.
	/// </summary>
	public class StatsDatabaseManager : DatabaseManager
	{
		public StatsDatabaseManager(SqlConnector sqlConnector) : base(sqlConnector) { }

		/// <summary>
		/// Gets a user by their database userid.
		/// </summary>
		public User GetUserById(int uid)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = SqlConnector.Users.Where(u => u.Id == uid).ToArray();
				if (matches.Length == 0) throw new ArgumentException("Nonexistent userid");
				if (matches.Length == 1) return matches[0];
				throw new CorruptedDatabaseException($"Userid {uid} is not unique");
			}
		}

		/// <summary>
		/// Finds users by their nickname.
		/// </summary>
		public User[] GetUsersByNickname(string nickname)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = SqlConnector.Users.Where(u => u.Nickname == nickname).ToArray();
				return matches;
			}
		}

		/// <summary>
		/// Tries to find a user by their nickname.
		/// Fails if the nickname is not unique, or if no user with that name exists.
		/// </summary>
		public User GetUserByNickname(string nickname)
		{
			var matches = GetUsersByNickname(nickname);
			if (matches.Length == 0) throw new ArgumentException($"No user with nickname \"{nickname}\" known.");
			if (matches.Length == 1) return matches[0];
			throw new ArgumentException($"Multiple matches found for nickname \"{nickname}\"");
		}

		/// <summary>
		/// Maps a ChatUser to an existing database user.
		/// </summary>
		public User MapUser(ChatUser user)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				if (user.HasTemporallyUniqueId)
				{
					// The user's UniqueId is guaranteed to be correct, so we can simply match on that
					var matches = SqlConnector.Users.Where(u => u.UniqueId == user.UniqueId).ToArray();
					if (matches.Length == 0) throw new ArgumentException("Nonexistent UniqueId");
					if (matches.Length == 1) return matches[0];
					throw new CorruptedDatabaseException($"Multiple users with the same UniqueId (\"{user.UniqueId}\") found");
				}
				else
				{
					throw new NotImplementedException("IRC user mapping is not supported yet");
				}
			}
		}

		/// <summary>
		/// Returns the matching database user for the given ChatUser.
		/// Will create a database user if one does not exist, and updates
		/// it if the user's nickname or AddressableName have changed.
		/// </summary>
		public User UpsertUser(ChatUser user)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				// TODO: implement IRC user mapping
				if (user.HasTemporallyUniqueId)
				{
					// The user's UniqueId is guaranteed to be correct, so we can simply match on that
					var matches = SqlConnector.Users.Where(u => u.UniqueId == user.UniqueId).ToArray();
					if (matches.Length == 0)
					{
						Logger.Log(this, $"Adding new user {user} to the database");
						var dbUser = new User
						{
							AddressableName = user.AddressableName,
							Nickname = user.Nickname,
							OriginalNickname = user.Nickname,
							UniqueId = user.UniqueId
						};
						SqlConnector.Insert(dbUser);
						// Instead of returning dbUser, grab the newly added user from the database,
						// which will set their userId to the generated value.
						return SqlConnector.Users.First(u => u.UniqueId == user.UniqueId);
					}
					if (matches.Length == 1)
					{
						var dbUser = matches[0];
						if (dbUser.AddressableName != user.AddressableName)
						{
							Logger.Log(this, $"Updating AddressableName for {dbUser} to {user.AddressableName}");
							dbUser.AddressableName = user.AddressableName;
						}
						if (dbUser.Nickname != user.Nickname)
						{
							Logger.Log(this, $"Updating Nickname for {dbUser} to {user.Nickname}");
							dbUser.Nickname = user.Nickname;
						}
						Update(dbUser);
						return matches[0];
					}
					throw new CorruptedDatabaseException($"Multiple users with the same UniqueId (\"{user.UniqueId}\") found");
				}
				else
				{
					throw new NotImplementedException("IRC user mapping is not supported yet");
				}
			}
		}

		public void UpdateUser(User newUser)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				Update(newUser);
			}
		}

		public void IncrementLineCount(int uid)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				var matches =
					from stat in SqlConnector.UserStatistics
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
					SqlConnector.Insert(nstat);
					Logger.Log(this, $"Created new stats row for {uid}.");
				}
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementWordCount(int uid, int words)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				SqlConnector.UserStatistics
					.Where(stat => stat.UserId == uid)
					.Set(stat => stat.Words, stat => stat.Words + words)
					.Update();
			}
			LockObj.LockMessage = "None";
		}

		public void AddMessage(ChatMessage message)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var line = new ChatLog
				{
					SentAt = message.SentAt,
					MessageType = MessageTypes.ChatMessage,
					ChannelId = message.Channel.Identifier,
					Channel = message.Channel.Name,
					SenderId = message.Sender.DbUser.Id,
					Nick = message.Sender.Nickname,
					Message = message.Body
				};

				SqlConnector.Insert(line);
			}
			LockObj.LockMessage = "None";
		}

		public long Import(IEnumerable<ChatMessage> messages)
		{
			long count;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				var chatLogs = messages.Select(message => new ChatLog
				{
					Flags = "C_IMPORT",
					SentAt = message.SentAt,
					MessageType = MessageTypes.ChatMessage,
					ChannelId = message.Channel.Identifier,
					Channel = message.Channel.Name,
					SenderId = message.Sender.DbUser?.Id,
					Nick = message.Sender.Nickname,
					Message = message.Body
				});

				count = SqlConnector.InsertMultiple(chatLogs);

			}
			LockObj.LockMessage = "None";
			return count;
		}


		public List<ChatLog> FindLine(string query, int uid = -1, string nickname = null)
		{
			List<ChatLog> matches;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				matches =
					(from line in SqlConnector.ChatLog
					 where line.Message.ToLower().Contains(query.ToLower())
						 && (uid == -1 || line.SenderId == uid)
						 && (nickname == null || line.Nick == nickname)
					 select line).ToList();
			}
			LockObj.LockMessage = "None";
			return matches;
		}

		public List<Quote> FindQuote(string search)
		{
			List<Quote> matches;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				matches =
					(from quote in SqlConnector.Quotes
					 where quote.Text.ToLower().Contains(search.ToLower())
					 select quote).ToList();

			}
			LockObj.LockMessage = "None";
			return matches;
		}

		internal void Snag(ChatMessage message)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var q = new Quote
				{
					Text = message.Body,
					AuthorId = message.Sender.DbUser.Id,
					TakenAt = DateTime.Now
				};

				SqlConnector.Insert(q);
				Logger.Log(this, $"Added quote for {message.Sender.Nickname}.");
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementEmoticon(string emoticon, int uid)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tEmoticon in SqlConnector.Emoticons
							  where tEmoticon.Emoticon == emoticon
							  select tEmoticon;
				if (!matches.Any())
				{
					var insert = new UsedEmoticon
					{
						Emoticon = emoticon,
						LastUsedById = uid,
						Uses = 1
					};
					SqlConnector.Insert(insert);
				}
				else
				{
					matches.Set(m => m.Uses, m => m.Uses + 1)
						.Set(m => m.LastUsedById, uid).Update();
				}
				Logger.Log(this, $"Incremented emoticon count with emoticon: {emoticon}.");
			}
			LockObj.LockMessage = "None";
		}

		public void SetVar(string key, int amount)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in SqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any())
				{
					var p = new KeyValuePair
					{
						Key = key,
						Value = amount
					};
					SqlConnector.Insert(p);
					Logger.Log(this, $"Inserted keyvaluepair with key: {key}.");
				}
				else
				{
					matches.Set(m => m.Value, amount).Update();
					Logger.Log(this, $"Changed keyvaluepair with key: {key}.");
				}
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementVar(string key, int amount = 1)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in SqlConnector.KeyValuePairs
							  where pair.Key == key
							  select pair;

				if (!matches.Any())
				{
					var p = new KeyValuePair
					{
						Key = key,
						Value = amount
					};
					SqlConnector.Insert(p);
					Logger.Log(this, $"Inserted keyvaluepair with key: {key}.");
				}
				else
				{
					var m = matches.First();
					m.Value += amount;
					Update(m);
				}
			}
			LockObj.LockMessage = "None";
		}

		public void UpsertMiscData(string type, string key, string value)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from pair in SqlConnector.MiscData
							  where pair.Type == type
									&& pair.Key == key
							  select pair;
				if (matches.Any())
				{
					matches.Set(data => data.Value, () => value).Update();
				}
				else
				{
					SqlConnector.Insert(new MiscData
					{
						Type = type,
						Key = key,
						Value = value,
						Enabled = true
					});
				}
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementUrl(string url, int uid, string usage)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from tUrl in SqlConnector.LinkedUrls
							  where tUrl.Url == url
							  select tUrl;

				if (!matches.Any())
				{
					var u = new LinkedUrl
					{
						LastUsage = usage,
						LastUsedById = uid,
						Url = url,
						Uses = 1
					};
					SqlConnector.Insert(u);
				}
				else
				{
					matches.Set(m => m.Uses, m => m.Uses + 1)
						.Set(m => m.LastUsage, usage)
						.Set(m => m.LastUsedById, uid)
						.Update();
				}
				Logger.Log(this, "Incremented URL count with URL: " + url + ".");
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementWord(string word)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				var matches = from usedWord in SqlConnector.Words
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
					SqlConnector.Insert(new UsedWord
					{
						Uses = 1,
						Word = word
					});
				}
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementProfanities(int uid)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var stat = (from s in SqlConnector.UserStatistics
							where s.UserId == uid
							select s).First();
				stat.Profanities++;
				Update(stat);
				Logger.Log(this, "Incremented profanities for " + uid + ".");
			}
			LockObj.LockMessage = "None";
		}

		public void IncrementActions(int uid)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var stat = (from s in SqlConnector.UserStatistics
							where s.UserId == uid
							select s).First();
				stat.Actions++;
				Update(stat);
				Logger.Log(this, "Incremented actions for " + uid + ".");
			}
			LockObj.LockMessage = "None";
		}

		public DateTime? GetLastQuotedLine(int userId)
		{
			DateTime? ret;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var data = from quot in SqlConnector.Quotes
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
			LockObj.LockMessage = "None";
			return ret;
		}

		internal void IncrementUserStatistic(UserStatistic changes)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var matches = from stat in SqlConnector.UserStatistics
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

		internal IOrderedEnumerable<Topic> FindTopics(int uid, string channelId)
		{
			List<string> userSentences;
			Dictionary<string, int> globalWordCount;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				Logger.Log(this, "finding words");
				var words = from word in SqlConnector.Words
				            group word by word.Word.ToLower() into grouped
							select new
							{
								Word = grouped.Key,
								Uses = grouped.Sum(w => w.Uses)
							};
				Logger.Log(this, "building dictionary");

				globalWordCount = words.ToDictionary(word => word.Word, word => word.Uses);

				

				Logger.Log(this, "finding sentences");
				userSentences = (from sentence in SqlConnector.ChatLog
								 where sentence.SenderId == uid
									   && sentence.ChannelId == channelId
									   && !sentence.Message.StartsWith(Bot.CommandIdentifiers.First())
								 select sentence.Message).ToList();

				LockObj.LockMessage = "None";
			}
			if (userSentences.Count == 0)
			{
				return null;
			}

			Logger.Log(this, "finding user words");
			var userWords = userSentences.SelectMany(WordTools.GetWords);

			Logger.Log(this, "grouping user words");
			var userWordCount = userWords.GroupBy(word => word.ToLower()).Where(g => g.Count() > 1).ToDictionary(group => group.MostFrequent(), group => group.Count());

			Logger.Log(this, "calculating usage difference of " + userWordCount.Count + " words");

			var topics = userWordCount.Where(pair => globalWordCount.ContainsKey(pair.Key.ToLower())).Select(pair => new Topic(pair.Key, pair.Value, globalWordCount[pair.Key.ToLower()])).ToList();

			if (topics.Count == 0)
			{
				// Topics.Count can be 0 if the user only used a few words.
				return null;
			}

			Logger.Log(this, "calculating average usage difference");
			// First, we need to normalise each word, so that a score of 1 means it's used just as often as the average user uses it.
			// This is done by taking the average usage percentage for a word, calculating the multiplier that will turn this score into a score of 1,
			// and then multiplying all words by that multiplier.

			var totalUsage = globalWordCount.Sum(w => w.Value);
			var userUsage = userWordCount.Sum(w => w.Value);

			var multiplier = (double)totalUsage / userUsage;

			var maxGlobalCount = globalWordCount.Max(pair => pair.Value);

			Logger.Log(this, "multiplying difference with multiplier");
			foreach (var topic in topics)
			{
				topic.Normalise(multiplier);
				topic.ScoreByOccurrence(maxGlobalCount);
			}
			return topics.OrderByDescending(topic => topic.Score);

		}

		public string GetMiscData(string type, string key)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				var results = from pair in SqlConnector.MiscData
							  where pair.Type == type
									&& pair.Key == key
							  select pair.Value;
				if (results.Count() > 1)
				{
					LockObj.LockMessage = "None";
					throw new InvalidOperationException("Multiple values were returned for a single type-key combination.");
				}
				else if (!results.Any())
				{
					LockObj.LockMessage = "None";
					throw new InvalidOperationException($"Value for type {type}, key {key} not found.");
				}
				else
				{
					LockObj.LockMessage = "None";
					return results.First();
				}
			}
		}

		public bool MiscDataContainsKey(string type, string key)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				return (from pair in SqlConnector.MiscData
						where pair.Type == type
							  && pair.Key == key
						select pair).Any();
			}
		}

		public IEnumerable<ChatLog> GetMessages()
		{
			List<ChatLog> lines;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				lines = SqlConnector.ChatLog.Select(line => line).OrderBy(line => line.SentAt).ToList();
				LockObj.LockMessage = "None";
			}
			return lines;
		}

		public void ResetWords(Dictionary<string, int> usages)
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				SqlConnector.Words.Delete(w => true);

				//var currentUsages = SqlConnector.Words.Select( w => w);
				var rows = usages.Select(pair => new UsedWord
				{
					Word = pair.Key,
					Uses = pair.Value
				}).OrderByDescending(w => w.Uses);

				SqlConnector.InsertMultiple(rows);

				LockObj.LockMessage = "None";
			}
		}

		private UserGroup[] GetUserMembership(User dbUser)
		{
			UserGroup[] membership;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				membership = (from entry in SqlConnector.UserGroupMembership
							  join userGroup in SqlConnector.UserGroups on entry.GroupId equals userGroup.Id
							  where entry.Enabled
							  where entry.UserId == dbUser.Id
							  select userGroup).ToArray();


				LockObj.LockMessage = "None";
			}
			return membership;
		}

		private PermissionGroup[] GetPermissionMembership(string[] nodes)
		{
			PermissionGroup[] membership;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				membership = (from entry in SqlConnector.PermissionGroupMembership
							  join permGroup in SqlConnector.PermissionGroups on entry.GroupId equals permGroup.Id
							  where entry.Enabled
							  where nodes.Contains(entry.PermissionName)
							  select permGroup).ToArray();

				LockObj.LockMessage = "None";
			}
			return membership;
		}

		public PermissionEntry[] GetPermissionEntries(User dbUser, ChatChannel channel, string[] nodes)
		{
			var userMembership = GetUserMembership(dbUser);
			if (userMembership.Any())
			{
				Logger.Log(this, $"User {dbUser} is a member of {string.Join(", ", userMembership.Select(g => $"\"{g.Name}\""))}.", LogLevel.Trace);
			}

			var permMembership = GetPermissionMembership(nodes);

			var userGroupIds = userMembership.Select(g => g.Id);
			var permGroupIds = permMembership.Select(g => g.Id);

			PermissionEntry[] entries;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();

				entries = (from entry in SqlConnector.PermissionEntries
						   where entry.Enabled
						   where (entry.ChannelId == null || (channel != null && entry.ChannelId == channel.Identifier))
						   where entry.UserGroup == null || userGroupIds.Contains(entry.UserGroup.Value)
						   where entry.PermissionGroup == null || permGroupIds.Contains(entry.PermissionGroup.Value)
						   select entry).ToList().OrderBy(entry => entry.Specificity).ToArray();

				LockObj.LockMessage = "None";
			}
			return entries;
		}

		public PermissionGroup CreatePermissionGroup(string name)
		{
			lock (LockObj)
			{
				if (SqlConnector.PermissionGroups.Any(p => p.Name == name))
				{
					throw new ArgumentException("A group with the given name already exists");
				}
				var group = new PermissionGroup
				{
					Name = name,
					Created = DateTime.Now
				};
				SqlConnector.Insert(group);
				return SqlConnector.PermissionGroups.First(g => g.Name == name);
			}
		}


		public PermissionGroup GetPermissionGroup(string groupName)
		{
			lock (LockObj)
			{
				var matches = SqlConnector.PermissionGroups.Where(g => g.Name == groupName);
				if (matches.Count() <= 1) return matches.FirstOrDefault();
				throw new CorruptedDatabaseException("Multiple permission groups with the same name defined");
			}
		}

		/// <summary>
		/// Retrieves the single-user usergroup belonging to the given user,
		/// creating it if it does not exist.
		/// </summary>
		public PermissionGroup GetSinglePermissionGroup(string name)
		{
			var uniqueGroupName = "perm-group-" + name;

			lock (LockObj)
			{
				var matches = SqlConnector.PermissionGroups.Where(g => g.Name == uniqueGroupName && g.SinglePermission).ToList();
				if (matches.Count == 0)
				{
					var group = new PermissionGroup
					{  
						Name = uniqueGroupName,
						SinglePermission = true,
						Created = DateTime.Now
					};
					SqlConnector.Insert(group);
					var created = SqlConnector.PermissionGroups.First(g => g.Name == uniqueGroupName && g.SinglePermission);
					var membership = new PermissionGroupMembership
					{
						GroupId = created.Id,
						PermissionName = name,
						Enabled = true,
						Added = DateTime.Now
					};
					SqlConnector.Insert(membership);
					Logger.Log(this, $"Created new single-permission group \"{uniqueGroupName}\" for {name}");
					return created;
					;
				}
				if (matches.Count == 1) return matches.First();
				throw new CorruptedDatabaseException("Multiple permission groups with the same name defined");
			}
		}

		/// <summary>
		/// Creates a new, empty usergroup with the given name.
		/// </summary>
		public UserGroup CreateUserGroup(string name)
		{
			lock (LockObj)
			{
				if (SqlConnector.UserGroups.Any(g => g.Name == name))
				{
					throw new ArgumentException("A group with the given name already exists");
				}
				var group = new UserGroup
				{
					Name = name,
					Created = DateTime.Now
				};
				SqlConnector.Insert(group);
				return SqlConnector.UserGroups.First(g => g.Name == name);
			}
		}

		/// <summary>
		/// Retrieves the usergroup with the given name, or null if it does not exist.
		/// </summary>
		public UserGroup GetUserGroup(string groupName)
		{
			lock (LockObj)
			{
				var matches = SqlConnector.UserGroups.Where(g => g.Name == groupName);
				if (matches.Count() <= 1) return matches.FirstOrDefault();
				throw new CorruptedDatabaseException("Multiple user groups with the same name defined");
			}
		}

		/// <summary>
		/// Retrieves the single-user usergroup belonging to the given user,
		/// creating it if it does not exist.
		/// </summary>
		public UserGroup GetSingleUserGroup(User user)
		{
			var uniqueGroupName = "user-group-" + user.UniqueId;

			lock (LockObj)
			{
				var matches = SqlConnector.UserGroups.Where(g => g.Name == uniqueGroupName && g.SingleUser).ToList();
				if (matches.Count == 0)
				{
					var group = new UserGroup
					{
						Name = uniqueGroupName,
						SingleUser = true,
						Created = DateTime.Now
					};
					SqlConnector.Insert(group);
					var created = SqlConnector.UserGroups.First(g => g.Name == uniqueGroupName && g.SingleUser);
					var membership = new UserGroupMembership
					{
						GroupId = created.Id,
						UserId = user.Id,
						Enabled = true,
						Added = DateTime.Now
					};
					SqlConnector.Insert(membership);
					Logger.Log(this, $"Created new single-user group \"{uniqueGroupName}\" for {user}");
					return created;
					;
				}
				if (matches.Count == 1) return matches.First();
				throw new CorruptedDatabaseException("Multiple user groups with the same name defined");
			}
		}

		public void AddPermissionEntry(PermissionEntry entry)
		{
			lock (LockObj)
			{
				if(SqlConnector.UserGroups.Any(g => g.Name == entry.Name)) throw new ArgumentException("A permissions entry with the given name already exists.");
				SqlConnector.Insert(entry);
				Logger.Log(this, $"Added new permissions entry with name \"{entry.Name}\" to the database.");
			}
		}

		public ChatLog[] GetChatLog(ChatChannel channel = null)
		{
			ChatLog[] log;
			lock (LockObj)
			{
				if (channel == null)
				{
					log = SqlConnector.ChatLog.ToArray();
				}
				else
				{
					log = SqlConnector.ChatLog.Where(l => l.ChannelId == channel.Identifier).ToArray();
				}
			}
			return log;
		}
	}

	internal class Topic
	{
		public string Name { get; }
		public int UserCount { get; }
		public int GlobalCount { get; }

		public double UsagePercentage { get; }
		/// <summary>
		/// How many times the specified user used this word relative to their average word usage.
		/// </summary>
		public double UsageFactor { get; private set; }

		public double ScoredByOccurrence { get; private set; }
		public double Score { get; private set; }

		public void Normalise(double multiplier)
		{
			UsageFactor = UsagePercentage * multiplier;
		}

		public void ScoreByOccurrence(int maxGlobalCount)
		{
			var countPercentage = (double)GlobalCount/maxGlobalCount;

			ScoredByOccurrence = UsageFactor + countPercentage / 2;
			Score = ScoredByOccurrence;

			if (UserCount == GlobalCount)
			{
				Score = Score - 0.1;
			}


			// Todo: find a better way to do this
			// This biases less common words.
			if (UserCount != GlobalCount && GlobalCount < maxGlobalCount / 2.0)
			{
				//Score += 1.5;
			}
		}

		public Topic(string name, int userCount, int globalCount)
		{
			Name = name;
			UserCount = userCount;
			GlobalCount = globalCount;
			UsagePercentage = (double)UserCount / GlobalCount;
		}

		public override string ToString() => $"{Name}: {UserCount} / {GlobalCount} -- Score: {Score}";
	}
}
