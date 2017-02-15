using System.Text.RegularExpressions;
using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class RegexTrigger
	{
		public string Expression { get; set; }
		public bool CaseSensitive { get; set; }

		public UserRegexTrigger Create()
		{
			return new UserRegexTrigger
			{
				CaseSensitive = CaseSensitive,
				Expression = Expression
			};
		}
	}

	public class UserRegexTrigger : RegexTrigger, ITriggerable
	{
		private Regex regex;

		public void Initialise()
		{
			var opts = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
			regex = new Regex(Expression, opts);
		}

		public bool Check(MessageEvent ev)
		{
			return regex.IsMatch(ev.Message.Body);
		}

	}
}