using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.Internal.Irc
{
	class IrcMessageFormatter : MessageFormatter
	{
		protected override string GetFormattingString(FormattingType type, FormattingPosition position)
		{
			switch (type)
			{
				case FormattingType.Italic:
					return  "\x1d";
				case FormattingType.Bold:
					return "\x02";
				case FormattingType.Underline:
					return "\x1f";
				case FormattingType.Strikethrough:
					return "~";
				case FormattingType.Quote:
					return ">";
				case FormattingType.Monospace:
					return "`";
				case FormattingType.MonospaceBlock:
					return "";
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public override ChatMessage ProcessIncomingMessage(ChatMessage message)
		{
			return message;
		}
	}
}
