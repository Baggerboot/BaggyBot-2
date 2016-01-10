using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.Model
{
	class KeyValuePair : Poco
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public int Value { get; set; }
	}
}
