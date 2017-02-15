using System.Linq;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Tools;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class ProfanityTrigger
	{
		public bool InternalProfanityList { get; set; }
		public string[] Words { get; set; } = { };

		public UserProfanityTrigger Create()
		{
			return new UserProfanityTrigger
			{
				InternalProfanityList = InternalProfanityList,
				Words = Words
			};
		}
	}

	public class UserProfanityTrigger : ProfanityTrigger, ITriggerable
	{
		public bool Check(MessageEvent ev)
		{
			var words = WordTools.GetWords(ev.Message.Body.ToLower());

			if (InternalProfanityList)
			{
				if (words.Any(WordTools.IsProfanity)) return true;
			}
			return words.Any(Words.Contains);
		}

		public void Initialise()
		{
		}
	}
}