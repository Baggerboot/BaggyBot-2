using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	public class WebSocketSession
	{
		public string SessionID { get; set; }
		public string MachineKey { get; set; }
		public User User { get; set; }
		public int[] Platforms { get; set; }
		public string NotificationServiceUrl { get; set; }
	}
}
