using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.Model
{
	class UsedWord : Poco
	{
		public int Id { get; set; }
		public string Word { get; set; }
		public int Uses { get; set; }
	}
}
