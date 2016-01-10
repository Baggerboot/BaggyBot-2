using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.Model
{
	class UsedEmoticon : Poco
	{
		public int Id { get; set; }
		public string Emoticon { get; set; }
		public int Uses { get; set; }
		public int LastUsedById { get; set; }
		public User LastUsedBy { get; set; }
	}
}
