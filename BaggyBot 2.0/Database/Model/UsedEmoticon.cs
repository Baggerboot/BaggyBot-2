using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "used_emoticon")]
	class UsedEmoticon : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "emoticon"), NotNull]
		public string Emoticon { get; set; }

		[Column(Name = "uses"), NotNull]
		public int Uses { get; set; }

		[Column(Name = "last_used_by"), NotNull]
		public int LastUsedById { get; set; }
		public User LastUsedBy { get; set; }
	}
}
