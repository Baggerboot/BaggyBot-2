using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "irc_user")]
	class User : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "name"), NotNull]
		public string Name { get; set; }
	}
}
