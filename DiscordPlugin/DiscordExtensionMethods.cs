using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordPlugin
{
	internal static class DiscordExtensionMethods
	{
		public static string GetDisplayName(this User user)
		{
			if (user.Nickname != null) return user.Nickname;
			return user.Name;
		}
	}
}
