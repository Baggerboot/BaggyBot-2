using System;
using System.Collections.Generic;
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
		public List<ChatUser> Users { get; protected set; }

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

		public override ChatUser GetUser(string id)
		{
			return Users.First(u => u.UniqueId == id);
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

		public override MessageSendResult SendMessage(ChatUser target, string message)
		{
			socketClient.SendMessage(null, socketClient.DirectMessageLookup[target.UniqueId].id, message);
			//todo: check success
			return  MessageSendResult.Success;
		}

		public override void Join(ChatChannel channel) { }
		public override void Part(ChatChannel channel, string reason = null) { }
		public override void Quit(string reason) { }

		public override void Delete(ChatMessage message)
		{
			socketClient.DeleteMessage(null, message.Channel.Identifier, message.SentAt);
		}

		public override void Kick(ChatUser chatUser)
		{
			throw new NotImplementedException();
		}

		public override void Ban(ChatUser chatUser)
		{
			throw new NotImplementedException();
		}

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
			
			var usersReady = new SemaphoreSlim(0);
			socketClient.GetUserList(response =>
			{
				Users = response.members.Select(ToChatUser).ToList();
				usersReady.Release();
			});
			if (!usersReady.Wait(TimeSpan.FromSeconds(30)))
			{
				return false;
			}

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
			return new ChatUser(user.name, user.id);
		}

		private ChatUser GetChatUser(User user)
		{
			var match = Users.FirstOrDefault(u => u.UniqueId == user.id);
			if (match == null)
			{
				match = ToChatUser(user);
				Users.Add(match);
			}
			return match;
		}

		private ChatMessage ToChatMessage(NewMessage message)
		{
			return new ChatMessage(message.ts, GetChatUser(socketClient.UserLookup[message.user]), GetChannel(message.channel), message.text);
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

			if (socketClient.MyData.id == user.id)
			{
				return;
			}
			
			OnMessageReceived?.Invoke(ToChatMessage(message));
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
			return GetChatUser(socketClient.Users.FirstOrDefault(u => u.name == name));
		}

		private ChatMessage[] GetMessages(ChatChannel channel, DateTime? before, DateTime? after, int count)
		{
			var semaphore = new SemaphoreSlim(0);
			MessageHistory history = null;
			switch (channel.Identifier[0])
			{
				case 'C':
					{
						var ch = socketClient.ChannelLookup[channel.Identifier];
						socketClient.GetChannelHistory((res =>
						{
							history = res;
							semaphore.Release();
						}), ch, before, after, count);
						break;
					}

				case 'G':
					{
						var gr = socketClient.GroupLookup[channel.Identifier];
						socketClient.GetGroupHistory((res =>
						{
							history = res;
							semaphore.Release();
						}), gr, before, after, count);
						break;
					}
				case 'D':
					{
						var dm = socketClient.DirectMessageLookup[channel.Identifier];
						socketClient.GetDirectMessageHistory((res =>
						{
							history = res;
							semaphore.Release();
						}), dm, before, after, count);
						break;
					}
				default:
					throw new ArgumentException("Invalid channel ID");
			}
			if (semaphore.Wait(TimeSpan.FromSeconds(30)))
			{
				return history.messages.Select(m => new ChatMessage(m.ts, ToChatUser(socketClient.UserLookup[m.user]), channel, m.text)).ToArray();
			}
			else
			{
				throw new InvalidOperationException("Network connection lost");
			}
		}

		public override IEnumerable<ChatMessage> GetBacklog(ChatChannel channel, DateTime before, DateTime after)
		{
			const int max = 100;
			int returned;
			int iteration = 1;
			var endTimestamp = before;
			do
			{
				DateTime? beginTimestamp = after;
				if (after == DateTime.MinValue) beginTimestamp = null;

				var messages = GetMessages(channel, endTimestamp, beginTimestamp, max);
				returned = messages.Length;
				foreach (var message in messages)
				{
					yield return message;
				}
				Logger.Log(this, $"Iteration {iteration++}: {messages.Length} messages. First: {messages[0].SentAt} Last: {messages[messages.Length - 1].SentAt}");
				endTimestamp = messages[messages.Length - 1].SentAt;

			} while (returned == max);
		}
	}
}
