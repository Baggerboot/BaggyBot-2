using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins
{
	/// <summary>
	/// A message formatter formats messages by inserting the correct formatting codes where necessary.
	/// </summary>
	public abstract class MessageFormatter : IMessagePreprocessor
	{
		private readonly IReadOnlyDictionary<string, FormattingType> formattingCodes;

		protected MessageFormatter()
		{
			formattingCodes = new Dictionary<string, FormattingType>
			{
				{Frm.I, FormattingType.Italic},
				{Frm.B, FormattingType.Bold },
				{Frm.M, FormattingType.Monospace },
				{Frm.MMultiline, FormattingType.MonospaceBlock },
				{Frm.Q, FormattingType.Quote },
				{Frm.U, FormattingType.Underline },
				{Frm.S, FormattingType.Strikethrough }
			};
		}

		/// <summary>
		/// Returns the formatting string expected by the chat server for the given formatting type.
		/// </summary>
		/// <param name="type">The type of formatting that should be done.</param>
		/// <param name="position">The position of the formatter in the text block to be formatted.</param>
		/// <returns></returns>
		protected abstract string GetFormattingString(FormattingType type, FormattingPosition position);

		/// <summary>
		/// Handles an incoming message from the server. Should translate any server-specific formatting codes
		/// to <see cref="Frm"/> formatting codes.
		/// </summary>
		public abstract ChatMessage ProcessIncomingMessage(ChatMessage message);

		public string ProcessOutgoingMessage(string message)
		{
			var currentPositions = formattingCodes.Values.ToDictionary(type => type, _ => FormattingPosition.Begin);
			var newMessage = message;

			foreach (var formattingCode in formattingCodes.Keys)
			{
				int index;
				while ((index = newMessage.IndexOf(formattingCode, StringComparison.Ordinal)) > -1)
				{
					// First, remove the formatting code from the message
					var snipped = newMessage.Remove(index, formattingCode.Length);
					// Now figure out what kind of formatting code we're dealing with
					var formattingType = formattingCodes[formattingCode];
					// Now figure out whether we're at the beginning of the formatted text or the end
					var formattingPosition = currentPositions[formattingType];
					// Ask the message formatter for the correct format string for said formatting code type and position
					var formatString = GetFormattingString(formattingType, formattingPosition);
					// Insert the returned format string.
					newMessage = snipped.Insert(index, formatString);
					// Finally, update our position
					switch(formattingPosition)
					{
						case FormattingPosition.Begin:
							currentPositions[formattingType] = FormattingPosition.End;
							break;
						case FormattingPosition.End:
							currentPositions[formattingType] = FormattingPosition.Begin;
							break;
					}
				}
			}
			return newMessage;
		}
	}
}
