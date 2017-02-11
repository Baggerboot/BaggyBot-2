using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "chat_log")]
	public class IrcLog : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "sent_at"), NotNull]
		public DateTime SentAt { get; set; }
		
		public string MessageType { get; set; }

		/// <summary>
		/// The User Id of the sender, or null if the message was not sent by a user.
		/// </summary>
		[Column(Name = "sender_id")]
		public int? SenderId { get; set; }
		/// <summary>
		/// The friendly name of the sender at the time of sending the message.
		/// </summary>
		[Column(Name = "sender"), NotNull]
		public string Nick { get; set; }

		/// <summary>
		/// The unique ID of the channel the message was posted to.
		/// </summary>
		[Column(Name = "channel_id"), NotNull]
		public string ChannelId { get; set; }
		/// <summary>
		/// The friendly name of the channel the message was posted to.
		/// </summary>
		[Column(Name = "channel"), NotNull]
		public string Channel { get; set; }

		/// <summary>
		/// The text contents of the message.
		/// </summary>
		[Column(Name = "message"), NotNull]
		public string Message { get; set; }

		public override string ToString()
		{
			return $"[{Channel}] <{Nick}>: {Message}";
        }
	}
}
