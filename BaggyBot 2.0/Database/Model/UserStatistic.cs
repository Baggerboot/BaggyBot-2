using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "user_statistic")]
	internal class UserStatistic : Poco
	{
		[Column(Name = "user_id"), PrimaryKey, NotNull]
		public int UserId { get; set; }
		//[Association(ThisKey = "user_id", OtherKey = "idf")]
		//public User User { get; set; }

		[Column(Name = "lines"), NotNull]
		public int Lines { get; set; }

		[Column(Name = "words"), NotNull]
		public int Words { get; set; }

		[Column(Name = "actions"), NotNull]
		public int Actions { get; set; }

		[Column(Name = "profanities"), NotNull]
		public int Profanities { get; set; }
	}
}
