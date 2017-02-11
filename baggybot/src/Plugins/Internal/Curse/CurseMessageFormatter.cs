using System;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins.Internal.Curse
{
	internal class CurseMessageFormatter : MessageFormatter
	{
		protected override string GetFormattingString(FormattingType type, FormattingPosition position)
		{
			switch (type)
			{
				case FormattingType.Italic:
					return "~";
				case FormattingType.Bold:
					return "*";
				case FormattingType.Underline:
					return "_";
				case FormattingType.Strikethrough:
					return "-";
				case FormattingType.Quote:
					return string.Empty;
				case FormattingType.Monospace:
					return "`";
				case FormattingType.MonospaceBlock:
					return "```\n";
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
