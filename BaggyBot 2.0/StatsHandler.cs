using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;
using BaggyBot.Tools;
using System.Text.RegularExpressions;

namespace BaggyBot
{
	class StatsHandler
	{
		private DataFunctionSet dataFunctionSet;
		private IrcInterface ircInterface;
		private Random rand;

		// Non-exhaustive list of shared idents that are commonly used by multiple people, often because they are standard values for their respective IRC clients.
		private string[] sharedIdents = { "webchat", "~quassel", "~AndChat12", "AndChat66", "~chatzilla", "~IceChat77", "~androirc", "Mibbit", "~PircBotX" };
		private string[] snagMessages = { "Snagged the shit outta that one!", "What a lame quote. Snagged!", "Imma stash those words for you.", "Snagged, motherfucker!", "Everything looks great out of context. Snagged!", "Yoink!", "That'll look nice on the stats page." };
		

		public StatsHandler(DataFunctionSet dm, IrcInterface inter)
		{
			dataFunctionSet = dm;
			ircInterface = inter;
			rand = new Random();
		}
		internal void ProcessMessage(IrcMessage message)
		{
			lock (dataFunctionSet.Lock) {
				int userId = dataFunctionSet.GetIdFromUser(message.Sender);

				List<string> words = WordTools.GetWords(message.Message);
				words = words.Select(s => s.Replace("'", "''")).ToList();

				dataFunctionSet.IncrementLineCount(userId);
				dataFunctionSet.IncrementWordCount(userId, words.Count);
				dataFunctionSet.IncrementVar("global_line_count");
				dataFunctionSet.IncrementVar("global_word_count", words.Count);
				GenerateRandomQuote(message, words);
				ProcessRandomEvents(message, words);
				GetEmoticons(userId, words);
				foreach (string word in words) {
					ProcessWord(message, word, userId);
				}
			}
		}

		private void ProcessRandomEvents(IrcMessage message, List<string> words)
		{
			if (message.Sender.Nick == "Ralph" && message.Message.ToLower().Contains("baggybot")) {
				ircInterface.SendMessage(message.Channel, "Shut up you fool");
			}
		}

		private void ProcessWord(IrcMessage message, string word, int sender)
		{
			string lword = word.ToLower();
			string cword = textOnly.Replace(lword, "");
			if (word.StartsWith("http://") || word.StartsWith("https://")) {
				dataFunctionSet.IncrementUrl(word, sender, message.Message.Replace("'", "''"));
			} else if (WordTools.IsProfanity(lword)) {
				dataFunctionSet.IncrementProfanities(sender);
			} else if (!WordTools.IsIgnoredWord(cword) && cword.Length >= 3) {
				dataFunctionSet.IncrementWord(cword);
			} 
		}

		Regex textOnly = new Regex("[^a-z]");

		private void GetEmoticons(int userId, List<string> words)
		{
			foreach (string word in words) {
				if (Emoticons.List.Contains(word)) {
					dataFunctionSet.IncrementEmoticon(word, userId);
				}
			}
		}

		private void GenerateRandomQuote(IrcMessage message, List<string> words)
		{
			if (ControlVariables.SnagNextLine) {
				ControlVariables.SnagNextLine = false;
				dataFunctionSet.Snag(message);
				ircInterface.SendMessage(message.Channel, "Snagged line on request.");
				return;
			} else if (ControlVariables.SnagNextLineBy != null && ControlVariables.SnagNextLineBy == message.Sender.Nick) {
				ControlVariables.SnagNextLineBy = null;
				dataFunctionSet.Snag(message);
				ircInterface.SendMessage(message.Channel, "Snagged line on request.");
				return;
			}
			double chance = 0.03;
			if (words.Count > 6) {
				if (rand.NextDouble() <= chance) {
					int randint = rand.Next(snagMessages.Length * 2);
					if (randint < snagMessages.Length) {
						ircInterface.SendMessage(message.Channel, snagMessages[randint]);
						dataFunctionSet.Snag(message);
					}else{
						ircInterface.SendMessage(message.Channel, "Snagged!");
						dataFunctionSet.Snag(message);
					}
				}
			}
		}

	
		


	}
}
