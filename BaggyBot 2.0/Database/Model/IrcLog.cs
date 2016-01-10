using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name =  "irc_log")]
	class IrcLog : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "sent_at"), NotNull]
		public DateTime SentAt { get; set; }

		[Column(Name = "sender"), NotNull]
		public int? SenderId { get; set; }
		public User Sender { get; set; }

		[Column(Name = "channel"), NotNull]
		public string Channel { get; set; }

		[Column(Name = "nick"), NotNull]
		public string Nick { get; set; }

		[Column(Name = "message"), NotNull]
		public string Message { get; set; }

	}
}
