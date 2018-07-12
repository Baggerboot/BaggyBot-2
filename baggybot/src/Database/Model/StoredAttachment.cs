using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "stored_attachment")]
	public class StoredAttachment : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "author"), NotNull]
		public int AuthorId { get; set; }

		[Column(Name = "key"), NotNull]
		public string Key { get; set; }

		[Column(Name = "stored_at"), NotNull]
		public DateTime TakenAt { get; set; }

		[Column(Name = "content_type"), NotNull]
		public string ContenType { get; set; }

		[Column(Name = "value"), NotNull]
		public byte[] Value { get; set; }
	}
}
