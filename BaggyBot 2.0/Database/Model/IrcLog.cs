using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.Model
{
	class IrcLog : Poco
	{
		public DateTime Time { get; set; }
		public int? SenderId { get; set; }
		public User Sender { get; set; }
		public string Channel { get; set; }
		public string Nick { get; set; }
		public string Message { get; set; }

	}
}
