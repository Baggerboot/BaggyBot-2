using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.SocketModel
{
	public class LoginResponse : ResponseBody
	{
		public int Status { get; set; }
		public DateTime ServerTime { get; set; }
		public object EncryptedSessionKey { get; set; }
	}

}
