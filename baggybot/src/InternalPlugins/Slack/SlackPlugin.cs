using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;
using BaggyBot.Plugins.MessageFormatters;
using IRCSharp;
using IRCSharp.IRC;
using SlackAPI;
using SlackAPI.WebSocketMessages;
using DebugLogEvent = BaggyBot.Plugins.DebugLogEvent;
using JoinChannelEvent = BaggyBot.Plugins.JoinChannelEvent;
using KickedEvent = BaggyBot.Plugins.KickedEvent;
using KickEvent = BaggyBot.Plugins.KickEvent;
using MessageReceivedEvent = BaggyBot.Plugins.MessageReceivedEvent;
using MessageSendResult = BaggyBot.Plugins.MessageSendResult;
using PartChannelEvent = BaggyBot.Plugins.PartChannelEvent;
using QuitEvent = BaggyBot.Plugins.QuitEvent;

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

		public override IReadOnlyList<ChatChannel> Channels { get; }
		public override bool Connected => client.IsConnected;
		private SlackSocketClient client;
		private readonly string token;

		public SlackPlugin(ServerCfg serverCfg) : base(serverCfg)
		{
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

		public NickservInformation NickservLookup(string nick)
		{
			return null;
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
			return clientReady.Wait(TimeSpan.FromSeconds(10));
		}

		private void MessageReceivedCallback(NewMessage message)
		{
			if (message.user == null && message.subtype == "bot_message")
			{
				return;
			}

			var user = client.UserLookup[message.user];

			ChatChannel chatChannel;
			switch (message.channel[0])
			{
				case 'D':
					var dm = client.DirectMessageLookup[message.channel];
					chatChannel = new ChatChannel(dm.id, dm.user, true);
					break;
				case 'C':
					var ch = client.ChannelLookup[message.channel];
					chatChannel = new ChatChannel(ch.id, ch.IsPrivateGroup);
					break;
				default:
					throw new InvalidOperationException("Unrecognised channel");
			}

			var chatUser = new ChatUser(this, user.name, user.id, name: user.profile.first_name);
			var chatMessage = new ChatMessage(this, chatUser, chatChannel, message.text);
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
