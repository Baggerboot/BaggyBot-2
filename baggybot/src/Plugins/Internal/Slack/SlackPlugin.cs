using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace BaggyBot.Plugins.Internal.Slack
{
	[ServerType("slack")]
	public class SlackPlugin : Plugin
	{
		public override event Action<ChatMessage> OnMessageReceived;
		public override event Action<ChatUser, ChatUser> OnNameChange;
		public override event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public override event Action<ChatChannel, ChatUser, string> OnKicked;
		public override event Action<string, Exception> OnConnectionLost;
		public override event Action<ChatUser, string> OnQuit;
		public override event Action<ChatUser, ChatChannel> OnJoinChannel;
		public override event Action<ChatUser, ChatChannel> OnPartChannel;

		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected => client.IsConnected;

		private SlackSocketClient client;
		private readonly string token;
		private Timer activityTimer;

		public SlackPlugin(ServerCfg serverCfg) : base(serverCfg)
		{
			Capabilities.AllowsMultilineMessages = true;
			Capabilities.AtMention = true;

			MessageFormatters.Add(new SlackMessagePreprocessor());
			MessageFormatters.Add(new SlackMessageFormatter());
			token = serverCfg.Password;
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
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

		public override void JoinChannel(ChatChannel channel) { }

		public override void Part(ChatChannel channel, string reason = null) { }

		public void Reconnect()
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
			Channels = client.Channels.Select(ToChatChannel)
			                          .Concat(client.Groups.Select(ToChatChannel))
			                          .Concat(client.DirectMessages.Select(ToChatChannel))
			                          .ToList();

			activityTimer = new Timer(state => client.SendPresence(Presence.active), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
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

		private ChatUser ToChatUser(User user)
		{
			return new ChatUser(user.name, user.id, name: user.profile.first_name);
		}

		private void MessageReceivedCallback(NewMessage message)
		{
			if (message.user == null && message.subtype == "bot_message")
			{
				return;
			}
			var user = client.UserLookup[message.user];
			var channel = GetChannel(message.channel);
			var chatUser = ToChatUser(user);
			var chatMessage = new ChatMessage(chatUser, channel, message.text);
			OnMessageReceived?.Invoke(chatMessage);
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}
		public override void Dispose()
		{
			activityTimer?.Dispose();
			if (client?.IsConnected ?? false)
			{
				client.CloseSocket();
			}
		}

		public override ChatUser FindUser(string name)
		{
			return ToChatUser(client.Users.FirstOrDefault(u => u.name == name));
		}
	}
}
