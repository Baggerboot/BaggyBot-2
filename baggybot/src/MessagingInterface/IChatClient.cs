using System;
using System.Collections.Generic;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public interface IChatClient : IDisposable
	{
		IReadOnlyList<ChatChannel> Channels { get; }
		bool Connected { get; }
		MessageSendResult SendMessage(ChatChannel target, string message);
		bool JoinChannel(ChatChannel channel);
		void Part(ChatChannel channel, string reason = null);
		void Quit(string reason);
		StatsDatabaseManager StatsDatabase { get; }
	}
}