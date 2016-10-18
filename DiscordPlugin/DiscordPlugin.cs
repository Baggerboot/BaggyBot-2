using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using IRCSharp;
using IRCSharp.IRC;
using Discord;
using IrcMessage = IRCSharp.IRC.IrcMessage;


namespace DiscordPlugin
{
	public class PluginInterface : IPlugin
	{
		public string ServerType => "discord";

		public event DebugLogEvent OnDebugLog;
		public event DebugLogEvent OnNetLibDebugLog;
		public event FormattedLineReceivedEvent OnFormattedLineReceived;
		public event MessageReceivedEvent OnMessageReceived;
		public event NickChangeEvent OnNickChange;
		public event KickEvent OnKick;
		public event KickedEvent OnKicked;
		public event DisconnectedEvent OnDisconnect;
		public event QuitEvent OnQuit;
		public event JoinChannelEvent OnJoinChannel;
		public event PartChannelEvent OnPartChannel;
		public event NoticeReceivedEvent OnNoticeReceived;

		private DiscordClient client;
		private Server server;

		public PluginInterface()
		{
			client = new DiscordClient();
			client.MessageReceived += (s, e) =>
			{
				if (!e.Message.IsAuthor)
				{
					OnMessageReceived?.Invoke(new IrcMessage(new IRCSharp.IRC.IrcUser(e.Message.User.GetDisplayName(), e.Message.User.Name, "discord.gg"), e.Channel.Name, e.Message.Text));
				}
			};

		}

		public void Disconnect()
		{
			client.Disconnect();
		}

		public void Dispose()
		{
			client.Dispose();
		}

		public bool Connect()
		{
			client.Connect("MjM4MDQyMzAyNTg3NjAwODk3.CugyIg.Ym0aJJpBMDqjh5SoICfXZoRFM4A", TokenType.Bot).Wait();

			while (!client.Servers.Any())
			{
				Thread.Sleep(100);
			}
			server = client.Servers.First();
			return true;
		}

		public IReadOnlyList<IrcChannel> Channels { get; }
		public bool Connected { get; }
		public string ServerName { get; }
		public StatsDatabaseManager StatsDatabase { get; set; }
		public IrcChannel GetChannel(string name)
		{
			throw new NotImplementedException();
		}

		public bool InChannel(string channel)
		{
			throw new NotImplementedException();
		}

		public MessageSendResult SendMessage(string target, string message)
		{
			var channels = server.FindChannels(target, ChannelType.Text, true)
				.Concat(client.PrivateChannels.Where(c => c.Name == target));
			var result = channels.First().SendMessage(message).Result;
			return MessageSendResult.Success;
		}

		public bool JoinChannel(string channel)
		{
			throw new NotImplementedException();
		}

		public bool JoinChannels(IEnumerable<string> channels)
		{
			// Discord does not require you to join any channels.
			return true;
		}

		public NickservInformation NickservLookup(string nick)
		{
			return null;
			//throw new NotImplementedException();
		}

		public ChatUser DoWhoisCall(string nick)
		{
			throw new NotImplementedException();
		}

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public void Part(string channel, string reason = null)
		{
			throw new NotImplementedException();
		}

		public void Quit(string reason)
		{
			throw new NotImplementedException();
		}
	}
}
