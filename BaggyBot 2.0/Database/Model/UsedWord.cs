using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name="used_word")]
	class UsedWord : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "word"), NotNull]
		public string Word { get; set; }

		[Column(Name = "uses"), NotNull]
		public int Uses { get; set; }
	}
}
