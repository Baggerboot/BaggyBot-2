using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Tools;
using IRCSharp.Annotations;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class Trigger
	{
		public RegexTrigger Regex { get; set; }
		public ProfanityTrigger Profanity { get; set; }
		public string Caps { get; set; }
		public int MinCharacters { get; set; }
		public RepetitionTrigger Repetition { get; set; }
		public SimilarityTrigger Similarity { get; set; }

		public UserTrigger Create()
		{
			var tr = new UserTrigger
			{
				Regex = Regex?.Create(),
				Profanity = Profanity?.Create(),
				Caps = MiscTools.GetPercentage(Caps),
				MinCharacters = MinCharacters,
				Repetition = Repetition?.Create(),
				Similarity = Similarity?.Create(),
			};
			tr.Regex?.Initialise();
			tr.Profanity?.Initialise();
			tr.Repetition?.Initialise();
			tr.Similarity?.Initialise();

			return tr;
		}
	}

	public class UserTrigger
	{
		public UserRegexTrigger Regex { get; set; }
		public UserProfanityTrigger Profanity { get; set; }
		public double? Caps { get; set; }
		public int MinCharacters { get; set; }
		public UserRepetitionTrigger Repetition { get; set; }
		public UserSimilarityTrigger Similarity { get; set; }

		public bool ShouldTrigger(MessageEvent ev)
		{
			var res = (Regex?.Check(ev) ?? true)
					  && (Profanity?.Check(ev) ?? true)
					  && CapsCheck(ev)
					  && MinCharactersCheck(ev)
					  && (Similarity?.Check(ev) ?? true);

			if (res)
			{
				res = (Repetition?.Check(ev) ?? true);
			}

			return res;
		}


		private bool CapsCheck(MessageEvent ev)
		{
			if (Caps == null) return true;

			// TODO: might want to strip non-alpha characters first
			var capsCount = ev.Message.Body.Count(char.IsUpper);
			var percentage = (double)capsCount/ ev.Message.Body.Length;
			return percentage >= Caps;
		}

		private bool MinCharactersCheck(MessageEvent ev)
		{
			return ev.Message.Body.Length >= MinCharacters;
		}
	}
}
