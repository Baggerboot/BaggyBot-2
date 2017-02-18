using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "permission_entry")]
	public class PermissionEntry : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "entry_name"), PrimaryKey, NotNull]
		public string Name { get; set; }

		[Column(Name = "enabled"), NotNull]
		public bool Enabled { get; set; }

		[Column(Name = "channel_id"), Nullable]
		public string ChannelId { get; set; }

		[Column(Name = "user_group"), Nullable]
		public int? UserGroup { get; set; }

		[Column(Name = "permission_group"), Nullable]
		public int? PermissionGroup { get; set; }

		[Column(Name = "action"), NotNull]
		public PermissionValue Action { get; set; }

		public int Specificity
		{
			get
			{
				var value = (int)Action;
				if (ChannelId != null) value += 10;
				if (UserGroup != null) value += 100;
				if (PermissionGroup != null) value += 1000;
				return value;
			}
		}
		public bool Value => (int)Action % 2 == 0;

		public override string ToString()
		{
			var disabled = Enabled ? "" : " (disabled)";
			return $"{Name}:{disabled} {Action}";
		}
	}

	public enum PermissionValue
	{
		Deny=1,
		Allow=2,
		ForceDeny=100001,
		ForceAllow=100002
	}
}
