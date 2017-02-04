using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.InternalPlugins.Curse.CurseApi.ExtensionMethods;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	static class ModelExtensions
	{
		internal static CurseApi Api { get; set; }

		public static Message[] GetMessages(this Channel channel, DateTime start, DateTime end, int pageSize)
		{
			var rs = Api.Get<Message[]>($"https://conversations-v1.curseapp.net/conversations/{channel.GroupID}" +
			        $"?startTimestamp={start.ToTimestamp()}" +
			        $"&endTimestamp={end.ToTimestamp()}" +
			        $"&pageSize={pageSize}");

			return rs;
		}
	}
}
