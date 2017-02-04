using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.SocketModel
{
	class SocketResponse
	{
		public ResponseType TypeID { get; set; }
		public ResponseBody Body { get; set; }

		public static SocketResponse Deserialise(string message)
		{
			var body = JObject.Parse(message)["Body"].ToString();
			var obj = JsonConvert.DeserializeObject<SocketResponse>(message);
			switch (obj.TypeID)
			{
				case ResponseType.ChatMessage:
					obj.Body = JsonConvert.DeserializeObject<MessageResponse>(body);
					break;
				case ResponseType.Login:
					obj.Body = JsonConvert.DeserializeObject<LoginResponse>(body);
					break;
				case ResponseType.UnknownChange:
					obj.Body = JsonConvert.DeserializeObject<UnknownChangeResponse>(body);
					break;
				case ResponseType.UserActivityChange:
					break;
				case ResponseType.ChannelReference:
					obj.Body = JsonConvert.DeserializeObject<ChannelReferenceResponse>(body);
					break;
				default:
					break;
			}
			return obj;
		}
	}
}
