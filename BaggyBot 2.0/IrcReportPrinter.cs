using System.Collections.Generic;
using Mono.CSharp;

namespace BaggyBot
{
	class IrcReportPrinter : ReportPrinter
	{
		private readonly Queue<AbstractMessage> UnreadMessages = new Queue<AbstractMessage>();

		public bool HasMessage
		{
			get
			{
				return UnreadMessages.Count > 0;
			}
		}

		public AbstractMessage GetNextMessage()
		{
			if (UnreadMessages.Count == 0)
				return null;
			return UnreadMessages.Dequeue();
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			base.Print(msg, showFullPath);
			UnreadMessages.Enqueue(msg);
		}
	}
}
