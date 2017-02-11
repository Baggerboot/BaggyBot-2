using System;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.Internal.Slack
{
	class SlackMessageFormatter : MessageFormatter
	{
		protected override string GetFormattingString(FormattingType type, FormattingPosition position)
		{
			switch (type)
			{
				case FormattingType.Italic:
					return "_";
				case FormattingType.Bold:
					return "*";
				case FormattingType.Underline:
					return "";
				case FormattingType.Strikethrough:
					return "~";
				case FormattingType.Quote:
					return ">";
				case FormattingType.Monospace:
					return "`";
				case FormattingType.MonospaceBlock:
					return "```";
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
