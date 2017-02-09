using Discord;

namespace BaggyBot.Plugins.Internal.Discord
{
	internal static class DiscordExtensionMethods
	{
		public static string GetDisplayName(this User user) => user.Nickname ?? user.Name;
	}
}
