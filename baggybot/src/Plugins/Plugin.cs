using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins.MessageFormatters;

namespace BaggyBot.Plugins
{
	public abstract class Plugin : IDisposable
	{
		// events
		public abstract event Action<ChatMessage> OnMessageReceived;
		public abstract event Action<ChatUser, ChatUser> OnNameChange;
		public abstract event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public abstract event Action<ChatChannel, ChatUser, string> OnKicked;
		public abstract event Action<string, Exception> OnConnectionLost;
		public abstract event Action<ChatUser, string> OnQuit;
		public abstract event Action<ChatUser, ChatChannel> OnJoinChannel;
		public abstract event Action<ChatUser, ChatChannel> OnPartChannel;

		public abstract string ServerType { get; }
		public abstract bool Connected { get; }
		public abstract IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public IReadOnlyList<Operator> Operators => ServerConfiguration.Operators;
		public List<IMessageFormatter> MessageFormatters { get; } = new List<IMessageFormatter>();
		protected ServerCfg ServerConfiguration { get; }
		public string ServerName => ServerConfiguration.ServerName;

		// Capabilities
		public bool AtMention { get; protected set; }
		public bool AllowsMultilineMessages { get; protected set; }

		public abstract ChatUser FindUser(string name);
		public abstract MessageSendResult SendMessage(ChatChannel target, string message);
		public abstract void JoinChannel(ChatChannel channel);
		public abstract void Part(ChatChannel channel, string reason = null);
		public abstract void Quit(string reason);
		public abstract bool Connect();
		public abstract void Disconnect();
		public abstract void Dispose();

		protected Plugin(ServerCfg config)
		{
			ServerConfiguration = config;
		}

		/// <summary>
		/// Lookup a channel by its name
		/// </summary>
		public ChatChannel FindChannel(string name)
		{
			var matches = Channels.Where(c => c.Name == name).ToArray();
			if (matches.Length == 0) throw new ArgumentException("Invalid channel name.");
			if (matches.Length == 1) return matches[0];
			throw new ArgumentException("Ambiguous channel name");
		}

		/// <summary>
		/// Lookup a channel by its ID
		/// </summary>
		public ChatChannel GetChannel(string channelId)
		{
			if (channelId == null) throw new ArgumentNullException(nameof(channelId));
			return Channels.First(c => c.Identifier == channelId);
		}

		/// <summary>
		/// Send a message to every operator defined for this client
		/// </summary>
		public void NotifyOperators(string message)
		{
			foreach (var op in Operators)
			{
				SendMessage(FindChannel(op.Nick), message);
			}
		}

		/// <summary>
		/// Replies to a specific user in a channel.
		/// </summary>
		public virtual MessageSendResult Reply(ChatChannel channel, ChatUser user, string message)
		{
			return SendMessage(channel, $"{(AtMention ? "@" : "")}{user.AddressableName}, {message}");
		}
	}
}
