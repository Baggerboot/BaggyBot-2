using System;
using System.Collections.Generic;
using System.Net;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using BaggyBot.Plugins.MessageFormatters;
using Curse.NET;
using MessageReceivedEvent = BaggyBot.Plugins.MessageReceivedEvent;
using Curse.NET.Model;
using Curse.NET.SocketModel;
using Group = Curse.NET.Model.Group;

namespace BaggyBot.InternalPlugins.Curse
{
	class CursePlugin : Plugin
	{
		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event NameChangeEvent OnNameChange;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event ConnectionLostEvent OnConnectionLost;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;

		public override string ServerType => "curse";

		public override IReadOnlyList<ChatChannel> Channels { get; }

		public override bool Connected { get; }

		private CurseClient client = new CurseClient();
		private NetworkCredential loginCredentials;

		public CursePlugin(ServerCfg config) : base(config)
		{
			AtMention = true;
			MessageFormatters.Add(new CurseMessageFormatter());
			loginCredentials = new NetworkCredential(config.Identity.Nick, config.Password);
			client.MessageReceived += HandleMessage;
		}

		private void HandleMessage(Group server, Channel channel, MessageResponse message)
		{
			var chatChannel = new ChatChannel(message.ConversationID, channel.GroupTitle);
			var sender = new ChatUser(this, message.SenderName, message.SenderID.ToString());
			var msg = new ChatMessage(this, sender, chatChannel, message.Body);
			OnMessageReceived?.Invoke(msg);
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			foreach (var formatter in MessageFormatters)
			{
				message = formatter.ProcessOutgoingMessage(message);
			}
			client.SendMessage(client.ChannelMap[target.Identifier], message);
			return MessageSendResult.Success;
		}

		public override bool JoinChannel(ChatChannel channel)
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
			client.Connect(loginCredentials.UserName, loginCredentials.Password);
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