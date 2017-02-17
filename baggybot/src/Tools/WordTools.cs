using System.Collections.Generic;
using System.Linq;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	internal static class WordTools
	{
		// Direct copy from BaggyBot 1.0 wheee
		public static readonly string[] Profanities = { "fuck", "cock", "dick", "bitch", "shit", "nigger", "asshole", "faggot", "wank", "cunt", "piss" };
		public static readonly string[] Conjunctions = { "and", "but", "or", "yet", "for", "nor", "so" };
		public static readonly string[] IgnoredWords = { "you", "its", "not", "was", "are", "can", "now", "all", "how", "that", "this", "what", "thats", "they", "then", "there", "when", "with", "well", "from", "will", "here", "out", "dont" };
		public static readonly string[] Articles = { "the", "an", "a" };

		public static List<string> GetWords(string message)
		{
			var words = message.Trim().Split(' ')
				.Select(w => w.TrimEnd(',', '.'))
				.Where(w => w != string.Empty).ToList();
			return words;
		}

		public static string[] GetProfanities()
		{
			return Profanities;
		}

		public static bool IsIgnoredWord(string word)
		{
			word = word.ToLower();
			return Conjunctions.Contains(word) || IgnoredWords.Contains(word) || Articles.Contains(word);
		}
		public static bool IsProfanity(string word)
		{
			return Profanities.Any(word.Contains);
		}
	}
}
