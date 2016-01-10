using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.Model
{
	class Quote : Poco
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Text { get; set; }
		public DateTime TakenAt { get; set; }
	}
}
