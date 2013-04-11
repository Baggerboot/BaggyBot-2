using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot
{
	static class WordTools
	{

		// Direct copy from BaggyBot 1.0 wheee
		private static string[] profanities = { "fuck", "cock", "dick", "cunt", "bitch", "shit", "piss", "nigger", "asshole", "faggot", "wank" };
		private static string[] conjunctions = { "and", "but", "or", "yet", "for", "nor", "so" };
		private static string[] snagMessages = { "THERE'S BUTTER ON MY FACE!", "Snagged the shit outta that one!", "What a lame quote. Snagged!", "Imma stash those words for you.", "Snagged, motherfucker!", "Everything looks great out of context. Snagged!", "Yoink!", "That'll look nice on the stats page." };
		private static string[] ignoredWords = { "you", "its", "not", "was", "are", "can", "now", "all", "how", "that", "this", "what", "thats", "they", "then", "there", "when", "with", "well", "from", "will", "here" };
		private static string[] articles = { "the", "an", "a" };

		internal static List<string> GetWords(string message)
		{
			List<string> words = message.Trim().Split(' ').ToList<string>();
			for (int i = 0; i < words.Count; i++) {
				words[i] = words[i].Trim();
				if (words[i] == string.Empty) {
					words.RemoveAt(i);
					i--;
				}
			}
			return words;
		}

		internal static bool IsIgnoredWord(string word)
		{
			return (conjunctions.Contains(word) || ignoredWords.Contains(word) || articles.Contains(word));
		}
		internal static bool IsProfanity(string word)
		{
			return profanities.Select(x => word.Contains(x)).Contains(true);
		}


	}
}
