using System;
using System.Collections.Generic;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public interface IChatClient : IDisposable
	{
		event DebugLogEvent OnDebugLog; // Can be used to send debug log messages to BaggyBot's log output
		event MessageReceivedEvent OnMessageReceived; // A message is received
		event NameChangeEvent OnNameChange; // Someone changes their name
		event KickEvent OnKick; // Someone is kicked
		event KickedEvent OnKicked; // The client is kicked
		event ConnectionLostEvent OnConnectionLost; // The client disconnects, whether intended or not
		event QuitEvent OnQuit; // Someone quits from IRC
		event JoinChannelEvent OnJoinChannel; // Someone joins a channel
		event PartChannelEvent OnPartChannel; // Someone parts a channel

		string ServerName { get; }
		IReadOnlyList<ChatChannel> Channels { get; }
		bool Connected { get; }
		//ChatChannel GetChannel(string name);
		MessageSendResult SendMessage(ChatChannel target, string message);
		bool JoinChannel(ChatChannel channel);
		void Part(ChatChannel channel, string reason = null);
		void Quit(string reason);
		StatsDatabaseManager StatsDatabase { get; }
	}
}