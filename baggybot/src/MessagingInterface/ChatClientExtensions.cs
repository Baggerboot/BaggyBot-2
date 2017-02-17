using System.Linq;

namespace BaggyBot.MessagingInterface
{
	static class ChatClientExtensions
	{
		// Helper methods
		public static bool InChannel(this ChatClient client, ChatChannel channel) => client.Channels.Contains(channel);
		public static void JoinChannel(this ChatClient client, string name) => client.JoinChannel(client.FindChannel(name));
		public static void Part(this ChatClient plugin, string name, string reason = null) => plugin.Part(plugin.FindChannel(name), reason);
	}
}
