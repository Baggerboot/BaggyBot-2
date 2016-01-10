using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "key_value_pair")]
	class KeyValuePair : Poco
	{
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		[Column(Name = "key"), NotNull]
		public string Key { get; set; }

		[Column(Name = "value"), NotNull]
		public int Value { get; set; }
	}
}
