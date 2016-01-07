using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using IRCSharp;
using BaggyBot.Tools;
using System.Text.RegularExpressions;
#if postgresql

#endif
#if mssql
using BaggyBot.Database.MS_SQL;
#endif
using IRCSharp.IRC;

namespace BaggyBot.DataProcessors
{
	class StatsHandler
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


			// It might seem confusing that we're creating a variable (snagChance) here without actually using it.
			// The reason for this is that we don't actually need it right now.
			// The value of snag_chance will be read from the Settings class each time it is required, so that, when it updates,
			// the updated value will be used right away.
			// This requires us to parse the variable each time it is read.
			// However, we do not want to log an error every time the parsing fails. For this reason, we parse it initially,
			// and, should a parse error occur, log the error.
			// The downside to this approach is that, if the variable is assigned an incorrect value at runtime,
			// the parser will fail silently, without logging its fallback to the default value.
			double snagChance;
			if (!double.TryParse(Settings.Instance["snag_chance"], out snagChance)) {
				Logger.Log(this, "Invalid settings value for snag_chance. Default value will be used.", LogLevel.Warning);
			}
		}
		public void ProcessMessage(IrcMessage message, int userId)
		{
			if (dataFunctionSet.ConnectionState == ConnectionState.Closed) return;

			Logger.Log(this, "Processing message for " + message.Sender.Nick);

			var words = WordTools.GetWords(message.Message);

			// FIXME: Is this necessary?
			words = words.Select(s => s.Replace("'", "''")).ToList();

			//UserStatistics changes = new UserStatistics();
			//changes.UserId = userId;

			if (message.Action) {
				dataFunctionSet.IncrementActions(userId);
				//changes.Actions++;
			} else {
				dataFunctionSet.IncrementLineCount(userId);
				//changes.Lines++;
			}
			dataFunctionSet.IncrementWordCount(userId, words.Count);
			//changes.Words += words.Count;
			/*foreach (string word in words) {
				if (WordTools.IsProfanity(word.ToLower())) {
					changes.Profanities++;
				}
			}*/
			//dataFunctionSet.IncrementUserStatistics(changes);

			dataFunctionSet.IncrementVar("global_line_count");
			dataFunctionSet.IncrementVar("global_word_count", words.Count);
			GenerateRandomQuote(message, words, userId);
			ProcessRandomEvents(message);
			GetEmoticons(userId, words);
			foreach (var word in words) {
				ProcessWord(message, word, userId);
			}
		}

		private void ProcessRandomEvents(IrcMessage message)
		{
			if (message.Sender.Nick == "Ralph" && message.Message.ToLower().Contains("baggybot")) {
				ircInterface.SendMessage(message.Channel, "Shut up you fool");
			}
		}

		private void ProcessWord(IrcMessage message, string word, int sender)
		{
			var lword = word.ToLower();
			var cword = textOnly.Replace(lword, "");
			if (word.StartsWith("http://") || word.StartsWith("https://")) {
				dataFunctionSet.IncrementUrl(word, sender, message.Message.Replace("'", "''"));
			} else if (!WordTools.IsIgnoredWord(cword) && cword.Length >= 3) {
				dataFunctionSet.IncrementWord(cword);
			} else if (WordTools.IsProfanity(lword)) {
				dataFunctionSet.IncrementProfanities(sender);
			}
		}

		readonly Regex textOnly = new Regex("[^a-z]");

		private void GetEmoticons(int userId, IEnumerable<string> words)
		{
			foreach (var word in words) {
				if (Emoticons.List.Contains(word)) {
					dataFunctionSet.IncrementEmoticon(word, userId);
				}
			}
		}
		private void GenerateRandomQuote(IrcMessage message, List<string> words, int userId)
		{
			if (message.Action) {
				message.Message =  "*" + message.Sender.Nick + " " + message.Message + "*";
			}

			if (ControlVariables.SnagNextLine) {
				ControlVariables.SnagNextLine = false;
				dataFunctionSet.Snag(message);
				ircInterface.SendMessage(message.Channel, "Snagged line on request.");
				return;
			}
			if (ControlVariables.SnagNextLineBy != null && ControlVariables.SnagNextLineBy == message.Sender.Nick) {
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
			if (last.HasValue) {
				if ((DateTime.Now - last.Value).Hours < int.Parse(Settings.Instance["snag_min_wait"])) {
					Logger.Log(this, "Dropped a snag as this user has recently been snagged already");
					return;
				}
			} else {
				Logger.Log(this, "This user hasn't been snagged before");
			}

			double snagChance;
			if (!double.TryParse(Settings.Instance["snag_chance"], out snagChance)) { // Set the base snag chance
				snagChance = 0.015;
			}

			double silenceChance;
			if (!double.TryParse(Settings.Instance["snag_silence_chance"], out silenceChance)) { // Set the chance for a silent snag
				silenceChance = 0.6;
			}


			if (words.Count > 6) { // Do not snag if the amount of words to be snagged is less than 7
				if (rand.NextDouble() <= snagChance) {
					bool allowSnagMessage;
					bool.TryParse(Settings.Instance["display_snag_message"], out allowSnagMessage);
					var hideSnagMessage = rand.NextDouble() <= silenceChance;
					if (!allowSnagMessage || hideSnagMessage) { // Check if snag message should be displayed
						Logger.Log(this, "Silently snagging this message");
						dataFunctionSet.Snag(message);
					} else {
						var randint = rand.Next(snagMessages.Length * 2); // Determine whether to simply say "Snagged!" or use a randomized snag message.
						if (randint < snagMessages.Length) {
							SnagMessage(message, snagMessages[randint]);
						} else {
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
