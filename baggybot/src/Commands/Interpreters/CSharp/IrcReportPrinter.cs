using System.Collections.Generic;
using Mono.CSharp;

namespace BaggyBot.Commands.Interpreters.CSharp
{
	internal class IrcReportPrinter : ReportPrinter
	{
		private readonly Queue<AbstractMessage> unreadMessages = new Queue<AbstractMessage>();

		public bool HasMessage
		{
			get
			{
				return unreadMessages.Count > 0;
			}
		}

		public AbstractMessage GetNextMessage()
		{
			if (unreadMessages.Count == 0)
				return null;
			return unreadMessages.Dequeue();
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			base.Print(msg, showFullPath);
			unreadMessages.Enqueue(msg);
		}
	}
}
