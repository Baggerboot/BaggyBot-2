using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	public class LoginResponse
	{
		public int Status { get; set; }
		public string StatusMessage { get; set; }
		public LoginSession Session { get; set; }
		public long Timestamp { get; set; }
	}
}
