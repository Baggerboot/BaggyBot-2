using System;
using System.Linq;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;
using BaggyBot.Tools;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class SimilarityTrigger
	{
		public string Percentage { get; set; }

		public UserSimilarityTrigger Create()
		{
			return new UserSimilarityTrigger
			{
				Percentage = MiscTools.GetPercentage(Percentage).Value
			};
		}
	}

	public class UserSimilarityTrigger : SimilarityTrigger, ITriggerable
	{
		private MessageEvent previous;
		public new double Percentage { get; set; }

		public bool Check(MessageEvent ev)
		{
			if (previous == null)
			{
				previous = ev;
				return false;
			}

			var a = ev.Message.Body.ToLower();
			var b = previous.Message.Body.ToLower();

			var similarity = GetSimilarity(a, b);

			previous = ev;
			return similarity >= Percentage;
		}

		/// <summary>
		/// Computes the similarity between two strings as a percentage.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static double GetSimilarity(string a, string b)
		{
			var length = a.Length;
			var distance = LevenshteinDistance(a, b, length);
			var similarCharacters = length - distance;
			var similarity = similarCharacters/(double) length;
			Logger.Log(null, $"Similarity: {similarity * 100 :###}%", LogLevel.Trace);
			return similarity;
		}

		/// <summary>
		/// Computes the Damerau-Levenshtein Distance between two strings.
		/// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
		/// </summary>
		private static int LevenshteinDistance(string a, string b, int maxDistance)
		{
			var aCodePoints = a.Select(c => (int)c).ToArray();
			var bCodePoints = b.Select(c => (int)c).ToArray();
			var distance = DamerauLevenshteinDistance(aCodePoints, bCodePoints, int.MaxValue);
			return distance > maxDistance ? maxDistance : distance;
		}

		/// <summary>
		/// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
		/// integers, where each integer represents the code point of a character in the source string.
		/// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
		/// </summary>
		/// <param name="source">An array of the code points of the first string</param>
		/// <param name="target">An array of the code points of the second string</param>
		/// <param name="threshold">Maximum allowable distance</param>
		/// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
		private static int DamerauLevenshteinDistance(int[] source, int[] target, int threshold)
		{

			int length1 = source.Length;
			int length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2)
			{
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			int maxi = length1;
			int maxj = length2;

			int[] dCurrent = new int[maxi + 1];
			int[] dMinus1 = new int[maxi + 1];
			int[] dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (int j = 1; j <= maxj; j++)
			{

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				int minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (int i = 1; i <= maxi; i++)
				{

					int cost = source[im1] == target[jm1] ? 0 : 1;

					int del = dCurrent[im1] + 1;
					int ins = dMinus1[i] + 1;
					int sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
						min = Math.Min(min, dMinus2[im2] + cost);

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			int result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}

		private static void Swap<T>(ref T arg1, ref T arg2)
		{
			T temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}

		public void Initialise()
		{
			
		}
	}
}