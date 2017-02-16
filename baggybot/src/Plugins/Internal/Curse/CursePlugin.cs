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
		public override bool Connected => client.Connected;

		private readonly CurseClient client = new CurseClient();
		private readonly NetworkCredential loginCredentials;
		private string serverName;

		public CursePlugin(ServerCfg config) : base(config)
		{
			// Configure the plugin
			Capabilities.AllowsMultilineMessages = true;
			Capabilities.AtMention = true;
			MessageFormatters.Add(new CurseMessageFormatter());
			
			serverName = (string)config.PluginSettings["server-name"];
			
			loginCredentials = new NetworkCredential(config.Username, config.Password);

			// Configure the Curse client
			client.MessageReceived += HandleMessage;
			client.WebsocketReconnected += () => Logger.Log(this, $"Websocket connction lost, but managed to reconnect.", LogLevel.Warning);
			client.ConnectionLost += () => OnConnectionLost?.Invoke("Websocket connection lost", null);
		}

		private void HandleMessage(Group group, Channel channel, MessageResponse message)
		{
			var chatChannel = new ChatChannel(message.ConversationID, channel.GroupTitle);
			var sender = new ChatUser(message.SenderName, message.SenderID.ToString());
			var msg = new ChatMessage(message.Timestamp, sender, chatChannel, message.Body, state: message);
			OnMessageReceived?.Invoke(msg);
		}

		public override ChatUser GetUser(string id)
		{
			throw new NotImplementedException();
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			client.SendMessage(client.ChannelMap[target.Identifier], message);
			return MessageSendResult.Success;
		}

		public override MessageSendResult SendMessage(ChatUser target, string message)
		{
			client.SendMessage(client.GroupMap[]);
		}

		public override void Join(ChatChannel channel)
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

		public override void Delete(ChatMessage message)
		{
			client.DeleteMessage(message.Channel.Identifier, ((MessageResponse)message.State).ServerID, message.SentAt);
		}

		public override void Kick(ChatUser chatUser)
		{
			//client.KickUser();
		}

		public override void Ban(ChatUser chatUser)
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
			client.Dispose();
		}

		public override ChatUser FindUser(string name)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ChatMessage> GetBacklog(ChatChannel channel, DateTime before, DateTime after)
		{
			const int max = 100;
			int returned;
			int iteration = 1;
			var endTimestamp = before;
			do
			{
				var messages = client.GetMessages(channel.Identifier, after, endTimestamp, max);
				returned = messages.Length;
				foreach (var message in messages)
				{
					var sender = new ChatUser(message.SenderName, message.SenderID.ToString());
					yield return new ChatMessage(message.Timestamp, sender, channel, message.Body);
				}
				Logger.Log(this, $"Iteration {iteration++}: {messages.Length} messages. First: {messages[0].Timestamp} Last: {messages[messages.Length-1].Timestamp}");
				endTimestamp = messages[messages.Length - 1].Timestamp;

			} while (returned == max);
		}
	}
}