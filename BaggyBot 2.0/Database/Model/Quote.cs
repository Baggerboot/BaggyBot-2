using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "quote")]
	class Quote : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "user"), NotNull]
		public int UserId { get; set; }

		[Column(Name = "text"), NotNull]
		public string Text { get; set; }

		[Column(Name = "taken_at"), NotNull]
		public DateTime TakenAt { get; set; }
	}
}
