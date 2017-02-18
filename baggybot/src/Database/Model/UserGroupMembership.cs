using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "user_group_membership")]
	public class UserGroupMembership : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "enabled"), NotNull]
		public bool Enabled { get; set; }

		[Column(Name = "group_id"), NotNull]
		public int GroupId { get; set; }

		[Column(Name = "user_id"), NotNull]
		public int UserId { get; set; }

		[Column(Name = "added"), NotNull]
		public DateTime Added { get; set; }
	}
}