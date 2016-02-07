using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "linked_url")]
	internal class LinkedUrl : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "url"), NotNull]
		public string Url { get; set; }

		[Column(Name = "uses"), NotNull]
		public int Uses { get; set; }

		[Column(Name = "last_used_by"), NotNull]
		public int LastUsedById { get; set; }
		[Association(ThisKey = "last_used_by", OtherKey = "id")]
		public User LastUsedBy { get; set; }

		[Column(Name = "last_usage"), NotNull]
		public string LastUsage { get; set; }
	}
}
