using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Tools;
using System.Text.RegularExpressions;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.EmbeddedData;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;

#if postgresql

#endif
#if mssql
using BaggyBot.Database.MS_SQL;
#endif

namespace BaggyBot.DataProcessors
{
	internal class StatsHandler
	{
		private readonly Random rand;
		private readonly Regex textOnly = new Regex("[^a-z]");

		// Non-exhaustive list of shared idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		/*
				private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };
		*/
		// TODO: move these to EmbeddedData
		private readonly string[] snagMessages = { "Snagged the shit outta that one!", "What a lame quote. Snagged!", "Imma stash those words for you.", "Snagged, motherfucker!", "Everything looks great out of context. Snagged!", "Yoink!", "That'll look nice on the stats page." };

		public StatsHandler()
		{
			rand = new Random();
		}
		public void ProcessMessage(ChatMessage message)
		{
			// Can't save statistics if we don't have a DB connection!
			if (message.Client.StatsDatabase.ConnectionState == ConnectionState.Closed) return;

			var user = message.Client.StatsDatabase.UpsertUser(message.Sender);
			var userId = user.Id;
			Logger.Log(this, $"Processing message for {message.Sender.Nickname} (uid: {userId})");
			AddMessageToIrcLog(message, userId);

			if (message.Action)
				message.Client.StatsDatabase.IncrementActions(userId);
			else
				message.Client.StatsDatabase.IncrementLineCount(userId);

			var words = WordTools.GetWords(message.Message);
			message.Client.StatsDatabase.IncrementWordCount(userId, words.Count);

			message.Client.StatsDatabase.IncrementVar("global_line_count");
			message.Client.StatsDatabase.IncrementVar("global_word_count", words.Count);
			GenerateRandomQuote(message, words, userId);
			ProcessRandomEvents(message);
			GetEmoticons(message.Client.StatsDatabase, userId, words);
			foreach (var word in words)
			{
				ProcessWord(message, word, userId);
			}
		}

		private void AddMessageToIrcLog(ChatMessage message, int userId)
		{
			message.Client.StatsDatabase.AddIrcMessage(DateTime.Now, userId, message.Channel.Identifier, message.Sender.Nickname,
				message.Action
				? $"*{message.Sender.Nickname} {message.Message}*"
				: message.Message);
		}

		private void ProcessRandomEvents(ChatMessage message)
		{
			if (message.Sender.Nickname == "Ralph" && message.Message.ToLower().Contains("baggybot"))
			{
				message.ReturnMessage("Shut up you fool");
			}
			else if (message.Message.ToLower().Contains("fuck you baggybot"))
			{
				message.ReturnMessage("pls ;___;");
			}
		}

		private void ProcessWord(ChatMessage message, string word, int sender)
		{
			// In order to count word occurrence correctly, all words are transformed to
			// lowercase, and all non-latin characters are removed. This is done because
			// people commonly say "its" when they mean "it's", and it is very difficult
			// to determine which one they meant, so instead, they are considered equal.
			var lword = word.ToLower();
			var cword = textOnly.Replace(lword, string.Empty);
			if (word.StartsWith("http://") || word.StartsWith("https://"))
			{
				message.Client.StatsDatabase.IncrementUrl(word, sender, message.Message);
			}
			else
			{
				var ignored = WordTools.IsIgnoredWord(cword) || cword.Length < 3;
					message.Client.StatsDatabase.IncrementWord(cword);
			}
			if (WordTools.IsProfanity(lword))
			{
				message.Client.StatsDatabase.IncrementProfanities(sender);
			}
		}

		private void GetEmoticons(StatsDatabaseManager statsDb, int userId, IEnumerable<string> words)
		{
			foreach (var word in words)
			{
				if (Emoticons.List.Contains(word))
				{
					statsDb.IncrementEmoticon(word, userId);
				}
			}
		}
		private void GenerateRandomQuote(ChatMessage message, List<string> words, int userId)
		{
			if (message.Action)
			{
				message.Message = "*" + message.Sender.Nickname + " " + message.Message + "*";
			}

			if (ControlVariables.SnagNextLine)
			{
				ControlVariables.SnagNextLine = false;
				message.Client.StatsDatabase.Snag(message);
				message.ReturnMessage("Snagged line on request.");
				return;
			}
			if (ControlVariables.SnagNextLineBy != null && ControlVariables.SnagNextLineBy == message.Sender.Nickname)
			{
				ControlVariables.SnagNextLineBy = null;
				message.Client.StatsDatabase.Snag(message);
				message.ReturnMessage("Snagged line on request.");
				return;
			}

			TryTakeQuote(message, words, userId);
		}

		private void TryTakeQuote(ChatMessage message, List<string> words, int userId)
		{
			var last = message.Client.StatsDatabase.GetLastQuotedLine(userId);
			if (last.HasValue)
			{
				if ((DateTime.Now - last.Value).Hours < ConfigManager.Config.Quotes.MinDelayHours)
				{
					return;
				}
			}

			var snagChance = ConfigManager.Config.Quotes.Chance;
			var silenceChance = ConfigManager.Config.Quotes.SilentQuoteChance;

			if (words.Count > 6)
			{ // Do not snag if the amount of words to be snagged is less than 7
				if (rand.NextDouble() <= snagChance)
				{
					var allowSnagMessage = ConfigManager.Config.Quotes.AllowQuoteNotifications;
					var hideSnagMessage = rand.NextDouble() <= silenceChance;
					if (!allowSnagMessage || hideSnagMessage)
					{ // Check if snag message should be displayed
						Logger.Log(this, "Silently snagging this message");
						message.Client.StatsDatabase.Snag(message);
					}
					else
					{
						var randint = rand.Next(snagMessages.Length * 2); // Determine whether to simply say "Snagged!" or use a randomized snag message.
						if (randint < snagMessages.Length)
						{
							TakeQuote(message, snagMessages[randint]);
						}
						else
						{
							TakeQuote(message, "Snagged!");
						}
					}
				}
			}
		}

		private void TakeQuote(ChatMessage message, string snagMessage)
		{
			message.ReturnMessage(snagMessage);
			message.Client.StatsDatabase.Snag(message);
		}
	}
}
