using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	public class SessionRequest : RequestObject
	{
		public string MachineKey { get; private set; }
		public int Platform { get; private set; }
		public string DeviceID { get; private set; }
		public string PushKitToken { get; private set; }

		public static SessionRequest Create()
		{
			return new SessionRequest
			{
				MachineKey = Guid.NewGuid().ToString(),
				Platform = 6,
			};
		}
	}
}
