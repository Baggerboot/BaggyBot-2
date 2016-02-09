using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Tools;
using System.Text.RegularExpressions;
using BaggyBot.Configuration;
using BaggyBot.DataProcessors.IO;
#if postgresql

#endif
#if mssql
using BaggyBot.Database.MS_SQL;
#endif
using IRCSharp.IRC;

namespace BaggyBot.DataProcessors
{
	internal class StatsHandler
	{
		private readonly DataFunctionSet dataFunctionSet;
		private readonly IrcInterface ircInterface;
		private readonly Random rand;

		// Non-exhaustive list of shared idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		/*
				private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };
		*/
		private readonly string[] snagMessages = { "Snagged the shit outta that one!", "What a lame quote. Snagged!", "Imma stash those words for you.", "Snagged, motherfucker!", "Everything looks great out of context. Snagged!", "Yoink!", "That'll look nice on the stats page." };
		
		public StatsHandler(DataFunctionSet dm, IrcInterface inter)
		{
			dataFunctionSet = dm;
			ircInterface = inter;
			rand = new Random();
		}
		public void ProcessMessage(IrcMessage message, int userId)
		{
			if (dataFunctionSet.ConnectionState == ConnectionState.Closed) return;

			Logger.Log(this, "Processing message for " + message.Sender.Nick);

			var words = WordTools.GetWords(message.Message);

			if (message.Action)
			{
				dataFunctionSet.IncrementActions(userId);
			}
			else
			{
				dataFunctionSet.IncrementLineCount(userId);
			}
			dataFunctionSet.IncrementWordCount(userId, words.Count);

			dataFunctionSet.IncrementVar("global_line_count");
			dataFunctionSet.IncrementVar("global_word_count", words.Count);
			GenerateRandomQuote(message, words, userId);
			ProcessRandomEvents(message);
			GetEmoticons(userId, words);
			foreach (var word in words)
			{
				ProcessWord(message, word, userId);
			}
		}

		private void ProcessRandomEvents(IrcMessage message)
		{
			if (message.Sender.Nick == "Ralph" && message.Message.ToLower().Contains("baggybot"))
			{
				ircInterface.SendMessage(message.Channel, "Shut up you fool");
			}
			else if (message.Message.ToLower().Contains("fuck you baggybot"))
			{
				ircInterface.SendMessage(message.Channel, "pls ;___;");
			}
		}

		private void ProcessWord(IrcMessage message, string word, int sender)
		{
			var lword = word.ToLower();
			var cword = textOnly.Replace(lword, string.Empty);
			if (word.StartsWith("http://") || word.StartsWith("https://"))
			{
				dataFunctionSet.IncrementUrl(word, sender, message.Message);
			}
			else if (!WordTools.IsIgnoredWord(cword) && cword.Length >= 3)
			{
				dataFunctionSet.IncrementWord(cword);
			}
			else if (WordTools.IsProfanity(lword))
			{
				dataFunctionSet.IncrementProfanities(sender);
			}
		}

		private readonly Regex textOnly = new Regex("[^a-z]");

		private void GetEmoticons(int userId, IEnumerable<string> words)
		{
			foreach (var word in words)
			{
				if (Emoticons.List.Contains(word))
				{
					dataFunctionSet.IncrementEmoticon(word, userId);
				}
			}
		}
		private void GenerateRandomQuote(IrcMessage message, List<string> words, int userId)
		{
			if (message.Action)
			{
				message.Message = "*" + message.Sender.Nick + " " + message.Message + "*";
			}

			if (ControlVariables.SnagNextLine)
			{
				ControlVariables.SnagNextLine = false;
				dataFunctionSet.Snag(message);
				ircInterface.SendMessage(message.Channel, "Snagged line on request.");
				return;
			}
			if (ControlVariables.SnagNextLineBy != null && ControlVariables.SnagNextLineBy == message.Sender.Nick)
			{
				ControlVariables.SnagNextLineBy = null;
				dataFunctionSet.Snag(message);
				ircInterface.SendMessage(message.Channel, "Snagged line on request.");
				return;
			}

			PerformSnagLogic(message, words, userId);
		}

		private void PerformSnagLogic(IrcMessage message, List<string> words, int userId)
		{
			var last = dataFunctionSet.GetLastSnaggedLine(userId);
			if (last.HasValue)
			{
				if ((DateTime.Now - last.Value).Hours < ConfigManager.Config.Quotes.MinDelayHours)
				{
					return;
				}
			}
			else
			{
				Logger.Log(this, "This user hasn't been snagged before");
			}
			
			double snagChance = ConfigManager.Config.Quotes.Chance;
			double silenceChance = ConfigManager.Config.Quotes.SilentQuoteChance;

			if (words.Count > 6)
			{ // Do not snag if the amount of words to be snagged is less than 7
				if (rand.NextDouble() <= snagChance)
				{
					bool allowSnagMessage = ConfigManager.Config.Quotes.AllowQuoteNotifications;
					var hideSnagMessage = rand.NextDouble() <= silenceChance;
					if (!allowSnagMessage || hideSnagMessage)
					{ // Check if snag message should be displayed
						Logger.Log(this, "Silently snagging this message");
						dataFunctionSet.Snag(message);
					}
					else
					{
						var randint = rand.Next(snagMessages.Length * 2); // Determine whether to simply say "Snagged!" or use a randomized snag message.
						if (randint < snagMessages.Length)
						{
							SnagMessage(message, snagMessages[randint]);
						}
						else
						{
							SnagMessage(message, "Snagged!");
						}
					}
				}
			}
		}

		private void SnagMessage(IrcMessage message, string snagMessage)
		{
			ircInterface.SendMessage(message.Channel, snagMessage);
			dataFunctionSet.Snag(message);
		}
	}
}
