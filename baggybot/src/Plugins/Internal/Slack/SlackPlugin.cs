using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace BaggyBot.Plugins.Internal.Slack
{
	[ServerType("slack")]
	public class SlackPlugin : Plugin
	{
#pragma warning disable CS0067
		public override event Action<ChatMessage> OnMessageReceived;
		public override event Action<ChatUser, ChatUser> OnNameChange;
		public override event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public override event Action<ChatChannel, ChatUser, string> OnKicked;
		public override event Action<string, Exception> OnConnectionLost;
		public override event Action<ChatUser, string> OnQuit;
		public override event Action<ChatUser, ChatChannel> OnJoin;
		public override event Action<ChatUser, ChatChannel> OnPart;
#pragma warning restore CS0067

		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected => socketClient.IsConnected;

		private SlackSocketClient socketClient;
		private Timer activityTimer;

		public SlackPlugin(ServerCfg serverCfg) : base(serverCfg)
		{
			Capabilities.AllowsMultilineMessages = true;
			Capabilities.AtMention = true;
			Capabilities.SupportsSpecialCharacters = true;

			MessageFormatters.Add(new SlackMessagePreprocessor());
			MessageFormatters.Add(new SlackMessageFormatter());
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			var ev = new ManualResetEvent(false);
			var success = false;
			socketClient.SendMessage(received =>
			{
				ev.Set();
				success = received.ok;
			}, target.Identifier, message);
			ev.WaitOne(TimeSpan.FromSeconds(10));
			return success ? MessageSendResult.Success : MessageSendResult.Failure;
		}

		public override void Join(ChatChannel channel) { }
		public override void Part(ChatChannel channel, string reason = null) { }
		public override void Quit(string reason) { }

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public override bool Connect()
		{
			var clientReady = new SemaphoreSlim(0);
			var socketReady = new SemaphoreSlim(0);
			socketClient = new SlackSocketClient(Configuration.Password);
			socketClient.Connect(connected =>
			{
				clientReady.Release();
			}, () =>
			{
				socketReady.Release();
			});
			socketClient.OnMessageReceived += MessageReceivedCallback;
			
			if (!Task.WaitAll(new[] { clientReady.WaitAsync(), socketReady.WaitAsync() }, TimeSpan.FromSeconds(10)))
			{
				return false;
			}
			Channels = socketClient.Channels.Select(ToChatChannel)
			                                .Concat(socketClient.Groups.Select(ToChatChannel))
			                                .Concat(socketClient.DirectMessages.Select(ToChatChannel))
			                                .ToList();

			activityTimer = new Timer(state => socketClient.SendPresence(Presence.active), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
			return true;
		}

		private ChatChannel ToChatChannel(Channel ch)
		{
			return new ChatChannel(ch.id, ch.name);
		}

		private ChatChannel ToChatChannel(DirectMessageConversation dm)
		{
			return new ChatChannel(dm.id, socketClient.UserLookup[dm.user].name, true);
		}

		private ChatUser ToChatUser(User user)
		{
			return new ChatUser(user.name, user.id, preferredName: user.profile.first_name);
		}

		private void MessageReceivedCallback(NewMessage message)
		{
			if (message.user == null)
			{
				if (message.subtype != "bot_message")
				{
					Logger.Log(this, "message.user is null, user lookup will not be possible", LogLevel.Warning);
					Logger.Log(this, $"Message details: {message.team}/{message.channel} ({message.subtype}): \"{message.text}\"", LogLevel.Warning);
				}
				return;
			}
			var user = socketClient.UserLookup[message.user];
			var channel = GetChannel(message.channel);
			var chatUser = ToChatUser(user);
			var chatMessage = new ChatMessage(message.ts, chatUser, channel, message.text);
			OnMessageReceived?.Invoke(chatMessage);
		}

		public override void Disconnect()
		{
			socketClient.CloseSocket();
		}
		public override void Dispose()
		{
			activityTimer?.Dispose();
			if (socketClient?.IsConnected ?? false)
			{
				socketClient.CloseSocket();
			}
		}

		public override ChatUser FindUser(string name)
		{
			return ToChatUser(socketClient.Users.FirstOrDefault(u => u.name == name));
		}
	}
}
