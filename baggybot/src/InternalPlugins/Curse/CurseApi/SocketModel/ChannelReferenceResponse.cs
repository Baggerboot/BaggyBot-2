using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.SocketModel
{
	public class ChannelReferenceResponse : ResponseBody
	{
		public string GroupID { get; set; }
		public int FriendID { get; set; }
		public DateTime Timestamp { get; set; }
		public string ConversationID { get; set; }
	}
}
