using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "user_credential")]
	internal class UserCredential : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "user_id"), NotNull]
		public int UserId { get; set; }
		//[Association(ThisKey = "user", OtherKey = "id")]
		//public User User { get; set; }

		[Column(Name = "nick"), NotNull]
		public string Nick { get; set; }

		[Column(Name = "ident"), NotNull]
		public string Ident { get; set; }

		[Column(Name = "hostmask"), NotNull]
		public string Hostmask { get; set; }

		[Column(Name = "nickserv_login")]
		public string NickservLogin { get; set; }
	}
}
