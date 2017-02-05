using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using Discord;
using Mono.CSharp;


namespace BaggyBot.InternalPlugins.Discord
{
	public class DiscordPlugin : Plugin
	{
		public override string ServerType => "discord";

		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event NameChangeEvent OnNameChange;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event ConnectionLostEvent OnConnectionLost;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;

		private DiscordClient client;
		private Server server;
		private string token;

		public DiscordPlugin(ServerCfg cfg) : base(cfg)
		{
			token = cfg.Password;
			client = new DiscordClient();
			client.MessageReceived += (s, e) =>
			{
				if (!e.Message.IsAuthor)
				{
					var user = BuildUser(e.User);
					var channel = BuildChannel(e.Channel);
					OnMessageReceived?.Invoke(new ChatMessage(this, user, channel, e.Message.Text));
				}
			};

		}

		private ChatChannel BuildChannel(Channel discordChannel)
		{
			return new ChatChannel(discordChannel.Id.ToString(), discordChannel.Name, discordChannel.IsPrivate);
		}
		private ChatUser BuildUser(User discordUser)
		{
			return new ChatUser(this, discordUser.Name, discordUser.Id.ToString(), name: discordUser.Nickname);
		}

		public override void Disconnect()
		{
			client.Disconnect();
		}

		public override void Dispose()
		{
			client.Dispose();
		}

		public override bool Connect()
		{
			client.Connect(token, TokenType.Bot).Wait();

			while (!client.Servers.Any())
			{
				Thread.Sleep(100);
			}
			server = client.Servers.First();
			return true;
		}

		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected { get; }
		public new StatsDatabaseManager StatsDatabase { get; set; }

		public bool InChannel(ChatChannel channel)
		{
			throw new NotImplementedException();
		}

		public override ChatUser FindUser(string name)
		{
			var matches = server.FindUsers(name).ToArray();
			if (matches.Length == 0) throw new ArgumentException("Invalid username");
			if (matches.Length == 1)return BuildUser(matches[0]);
			throw new ArgumentException("Ambiguous username");
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			var channel = server.GetChannel(ulong.Parse(target.Identifier));
			var result = channel.SendMessage(message).Result;
			return MessageSendResult.Success;
		}

		public override bool JoinChannel(ChatChannel channel)
		{
			throw new NotImplementedException();
		}

		public ChatUser DoWhoisCall(string nick)
		{
			throw new NotImplementedException();
		}

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public override void Part(ChatChannel channel, string reason = null)
		{
			throw new NotImplementedException();
		}

		public override void Quit(string reason)
		{
			throw new NotImplementedException();
		}
	}
}
