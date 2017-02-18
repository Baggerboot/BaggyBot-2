using System;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "permission_group")]
	public class PermissionGroup : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "group_name"), NotNull]
		public string Name { get; set; }

		[Column(Name = "single_permission"), NotNull]
		public bool SinglePermission { get; set; }

		[Column(Name = "created"), NotNull]
		public DateTime Created { get; set; }
	}
}