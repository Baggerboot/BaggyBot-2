using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name="user")]
	class User : Poco
	{
		[Column(Name="id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "name"), NotNull]
		public string Name { get; set; }
	}
}
