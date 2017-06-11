using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using Discord;
using Discord.WebSocket;

namespace BaggyBot.Plugins.Internal.Discord
{
	[ServerType("discord")]
	public class DiscordPlugin : Plugin
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

		public override bool Connected => client.ConnectionState == ConnectionState.Connected;
		public override ChatUser Self => new ChatUser(client.CurrentUser.Username, client.CurrentUser.Id.ToString());
		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }

		private readonly DiscordSocketClient client;
		//private Server server;

		public DiscordPlugin(ServerCfg cfg) : base(cfg)
		{
			client = new DiscordSocketClient();

			client.MessageReceived += HandleMessageReceived;
		}

		private async Task HandleMessageReceived(SocketMessage message)
		{
			if (message.Author != client.CurrentUser)
			{
				var user = ToChatUser(message.Author);
				var channel = ToChatChannel(message.Channel);
				OnMessageReceived?.Invoke(new ChatMessage(message.Timestamp.LocalDateTime, user, channel, message.Content, state: message));
			}
		}

		private ChatChannel ToChatChannel(IMessageChannel discordChannel)
		{
			return new ChatChannel(discordChannel.Id.ToString(), discordChannel.Name);
		}
		private ChatUser ToChatUser(IUser discordUser)
		{
			return new ChatUser(discordUser.Username, discordUser.Id.ToString(), preferredName: discordUser.Username);
		}

		public override void Disconnect()
		{
			client.Dispose();
		}

		public override void Dispose()
		{
			client.Dispose();
		}

		public override void Ban(ChatUser chatUser, ChatChannel channel = null)
		{
			if(channel != null) throw new NotSupportedException("The Discord plugin does not support banning users from a channel.");
			throw new NotImplementedException();
		}

		public override void Kick(ChatUser chatUser, ChatChannel channel = null)
		{
			if(channel != null) throw new NotSupportedException("The Discord plugin does not support kicking users from a channel.");
			server.GetUser(ulong.Parse(chatUser.UniqueId)).Kick();
		}

		public override bool Connect()
		{
			client.Connect(Configuration.Password, TokenType.Bot).Wait();

			while (!client.Servers.Any())
			{
				Thread.Sleep(100);
			}
			server = client.Servers.First();
			return true;
		}

		public override ChatUser FindUser(string name)
		{
			var matches = server.FindUsers(name).ToArray();
			if (matches.Length == 0) throw new ArgumentException("Invalid username");
			if (matches.Length == 1)return ToChatUser(matches[0]);
			throw new ArgumentException("Ambiguous username");
		}

		public override ChatUser GetUser(string id)
		{
			return ToChatUser(server.GetUser(ulong.Parse(id)));
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message, params Attachment[] attachments)
		{
			message = CreatePlaintextAttachments(message, attachments);
			var channel = server.GetChannel(ulong.Parse(target.Identifier));
			channel.SendMessage(message).Wait();
			return MessageSendResult.Success;
		}

		public override MessageSendResult SendMessage(ChatUser target, string message, params Attachment[] attachments)
		{
			message = CreatePlaintextAttachments(message, attachments);
			throw new NotImplementedException();
		}

		public override void Join(ChatChannel channel) { }

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public override void Part(ChatChannel channel, string reason = null) { }

		public override void Quit(string reason)
		{
			throw new NotImplementedException();
		}

		public override void Delete(ChatMessage message)
		{
			var m = (Message)message.State;
			m.Delete();
		}
	}
}
