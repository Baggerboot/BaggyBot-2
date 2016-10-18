
using Discord;

namespace BaggyBot.InternalPlugins.Discord
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
