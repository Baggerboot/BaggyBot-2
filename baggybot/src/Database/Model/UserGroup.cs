using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "user_group")]
	public class UserGroup : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "group_name"), NotNull]
		public string Name { get; set; }

		[Column(Name = "single_user"), NotNull]
		public bool SingleUser { get; set; }

		[Column(Name = "created"), NotNull]
		public DateTime Created { get; set; }
	}
}
