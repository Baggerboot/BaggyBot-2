using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Plugins
{
	public abstract class Plugin : IChatClient
	{
		// events
		public abstract event DebugLogEvent OnDebugLog;
		public abstract event MessageReceivedEvent OnMessageReceived;
		public abstract event NameChangeEvent OnNameChange;
		public abstract event KickEvent OnKick;
		public abstract event KickedEvent OnKicked;
		public abstract event ConnectionLostEvent OnConnectionLost;
		public abstract event QuitEvent OnQuit;
		public abstract event JoinChannelEvent OnJoinChannel;
		public abstract event PartChannelEvent OnPartChannel;

		// inherited properties
		public abstract string ServerType { get; }
		public abstract IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public abstract bool Connected { get; }

		// inherited methods
		//public abstract ChatChannel GetChannel(string name);
		public abstract MessageSendResult SendMessage(ChatChannel target, string message);
		public abstract bool JoinChannel(ChatChannel channel);
		public abstract void Part(ChatChannel channel, string reason = null);
		public abstract void Quit(string reason);

		// newly exposed abstract methods
		public abstract bool Connect();
		public abstract void Disconnect();
		public abstract void Dispose();

		// newly exposed properties
		public IReadOnlyList<Operator> Operators { get; }
		public List<IMessageFormatter> MessageFormatters { get; } = new List<IMessageFormatter>();
		public string ServerName { get; }
		public StatsDatabaseManager StatsDatabase { get; set; }
		public bool AtMention { get; protected set; }
		public bool AllowsMultilineMessages { get; protected set; }

		public bool InChannel(ChatChannel channel) => Channels.Contains(channel);
		public bool JoinChannel(string name) => JoinChannel(FindChannel(name));
		public void Part(string name, string reason = null) => Part(FindChannel(name), reason);

		public abstract ChatUser FindUser(string name);

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
		public ChatChannel GetChannel(string id)
		{
			return Channels.First(c => c.Identifier == id);
		}

		protected Plugin(ServerCfg config)
		{
			ServerName = config.ServerName;
			Operators = config.Operators;
		}
	}
}
