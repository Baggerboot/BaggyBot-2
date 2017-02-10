using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using Curse.NET;
using Curse.NET.Model;
using Curse.NET.SocketModel;

namespace BaggyBot.Plugins.Internal.Curse
{
	[ServerType("curse")]
	internal class CursePlugin : Plugin
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
		public override bool Connected { get; }

		private readonly CurseClient client = new CurseClient();
		private readonly NetworkCredential loginCredentials;

		public CursePlugin(ServerCfg config) : base(config)
		{
			Capabilities.AllowsMultilineMessages = true;
			Capabilities.AtMention = true;


			MessageFormatters.Add(new CurseMessageFormatter());
			loginCredentials = new NetworkCredential(config.Username, config.Password);
			client.MessageReceived += HandleMessage;
			client.WebsocketReconnected += () => Logger.Log(this, $"Websocket connction lost, but managed to reconnect.", LogLevel.Warning);
		}

		private void HandleMessage(Group server, Channel channel, MessageResponse message)
		{
			var chatChannel = new ChatChannel(message.ConversationID, channel.GroupTitle);
			var sender = new ChatUser(message.SenderName, message.SenderID.ToString());
			var msg = new ChatMessage(sender, chatChannel, message.Body);
			OnMessageReceived?.Invoke(msg);
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			client.SendMessage(client.ChannelMap[target.Identifier], message);
			return MessageSendResult.Success;
		}

		public override void JoinChannel(ChatChannel channel)
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
			try
			{
				client.Connect(loginCredentials.UserName, loginCredentials.Password);
			}
			catch (CurseDotNetException e)
			{
				Logger.Log(this, $"Connection failed. An exception occurred ({e.GetType().Name}: {e.Message})");
				return false;
			}
			Channels = client.ChannelMap.Values.Select(ch => new ChatChannel(ch.GroupID, ch.GroupTitle)).ToList();
			return true;
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