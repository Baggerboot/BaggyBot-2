using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using BaggyBot.Plugins.MessageFormatters;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace BaggyBot.InternalPlugins.Slack
{
	public class SlackPlugin : Plugin
	{
		public override string ServerType => "slack";

		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event ConnectionLostEvent OnConnectionLost;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;
		public override event NameChangeEvent OnNameChange;

		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected => client.IsConnected;
		private SlackSocketClient client;
		private readonly string token;

		public SlackPlugin(ServerCfg serverCfg) : base(serverCfg)
		{
			AllowsMultilineMessages = true;
			MessageFormatters.Add(new SlackMessageFormatter());
			MessageFormatters.Add(new MarkdownMessageFormatter());
			token = serverCfg.Password;
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			foreach (var formatter in MessageFormatters)
			{
				message = formatter.ProcessOutgoingMessage(message);
			}

			var ev = new ManualResetEvent(false);
			var success = false;
			client.SendMessage(received =>
			{
				ev.Set();
				success = received.ok;
			}, target.Identifier, message);
			ev.WaitOne(TimeSpan.FromSeconds(10));
			return success ? MessageSendResult.Success : MessageSendResult.Failure;
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

		public override bool Connect()
		{
			var clientReady = new ManualResetEventSlim(false);
			client = new SlackSocketClient(token);
			client.Connect(connected =>
			{
				clientReady.Set();
			}, () =>
			{
				// called once the RTM client has connected
			});
			client.OnMessageReceived += MessageReceivedCallback;
			if (!clientReady.Wait(TimeSpan.FromSeconds(10)))
			{
				return false;
			}
			Channels = client.Channels.Select(ToChatChannel).ToList();
			Channels = Channels.Concat(client.Groups.Select(ToChatChannel)).ToList();
			Channels = Channels.Concat(client.DirectMessages.Select(ToChatChannel)).ToList();
			return true;
		}

		private ChatChannel ToChatChannel(Channel ch)
		{
			return new ChatChannel(ch.id, ch.name);
		}

		private ChatChannel ToChatChannel(DirectMessageConversation dm)
		{
			return new ChatChannel(dm.id, client.UserLookup[dm.user].name, true);
		}

		private void MessageReceivedCallback(NewMessage message)
		{
			if (message.user == null && message.subtype == "bot_message")
			{
				return;
			}

			var user = client.UserLookup[message.user];

			var channel = GetChannel(message.channel);

			var chatUser = new ChatUser(this, user.name, user.id, name: user.profile.first_name);
			var chatMessage = new ChatMessage(this, chatUser, channel, message.text);
			OnMessageReceived?.Invoke(chatMessage);
		}

		private void ChannelMarkedCallback(MarkResponse markResponse)
		{
			;
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}
		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public override ChatUser FindUser(string name)
		{
			throw new NotImplementedException();
		}
	}
}
