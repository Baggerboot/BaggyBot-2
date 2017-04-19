using System;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface.Events
{
	public class MessageEvent
	{
		public ChatMessage Message { get; }
		internal Func<string, Attachment[], MessageSendResult> ReplyCallback { get; }
		internal Func<string, Attachment[], MessageSendResult> ReturnMessageCallback { get; }

		public MessageEvent(ChatMessage message, Func<string, Attachment[], MessageSendResult> replyCallback, Func<string, Attachment[], MessageSendResult> returnMessageCallback)
		{
			Message = message;
			ReplyCallback = replyCallback;
			ReturnMessageCallback = returnMessageCallback;
		}

		public void Reply(string message, params Attachment[] attachments)
		{
			ReplyCallback(message, attachments);
		}

		public void ReturnMessage(string message, params Attachment[] attachments)
		{
			ReturnMessageCallback(message, attachments);
		}
	}
}