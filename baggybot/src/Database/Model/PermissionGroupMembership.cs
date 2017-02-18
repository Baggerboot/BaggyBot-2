using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "permission_group_membership")]
	public class PermissionGroupMembership : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "enabled"), NotNull]
		public bool Enabled { get; set; }

		[Column(Name = "group_id"), NotNull]
		public int GroupId { get; set; }

		[Column(Name = "permission_name"), NotNull]
		public string PermissionName { get; set; }

		[Column(Name = "added"), NotNull]
		public DateTime Added { get; set; }
	}
}