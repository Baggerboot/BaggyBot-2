using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.Handlers;
using BaggyBot.Handlers.ChatClientEvents;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public class ChatClient : IDisposable
	{
		public string ServerType => plugin.ServerType;
		public string ServerName => plugin.ServerName;
		public bool Connected => plugin.Connected;
		public IReadOnlyList<Operator> Operators => plugin.Operators;
		public IReadOnlyList<ChatChannel> Channels => plugin.Channels;
		public bool AtMention => plugin.AtMention;
		public bool AllowsMultilineMessages => plugin.AllowsMultilineMessages;
		public bool SupportsUnicode => plugin.SupportsUnicode;

		public bool Connect() => plugin.Connect();
		public void JoinChannel(ChatChannel channel) => plugin.JoinChannel(channel);
		public void Quit(string reason) => plugin.Quit(reason);
		public void Part(ChatChannel channel, string reason = null) => plugin.Part(channel, reason);
		public ChatChannel FindChannel(string name) => plugin.FindChannel(name);
		public ChatChannel GetChannel(string id) => plugin.GetChannel(id);
		public void NotifyOperators(string message) => plugin.NotifyOperators(message);


		private readonly Plugin plugin;
		internal StatsDatabaseManager StatsDatabase { get; }

		internal ChatClient(Plugin plugin, ServerCfg serverConfiguration)
		{
			this.plugin = plugin;
			StatsDatabase = new StatsDatabaseManager(ConnectDatabase(serverConfiguration.Backend));

			var handlers = new List<ChatClientEventHandler>
			{
				new LogHandler {Client = this},
				new StatsHandler {Client = this},
				new AdministrationHandler {Client = this},
				new CommandHandler {Client = this}
			};
			foreach (var handler in handlers)
			{
				handler.Initialise();
			}
			var eventManager = new ChatClientEventManager(handlers);

			AttachEventHandlers(eventManager);
		}

		private void AttachEventHandlers(ChatClientEventManager eventManager)
		{
			plugin.OnMessageReceived += message => eventManager.HandleMessage(new MessageEvent(this, message,
				reply => Reply(message.Channel, message.Sender, reply),
				reply => SendMessage(message.Channel, reply)));
			plugin.OnNameChange += (oldName, newName) => eventManager.HandleNameChange(new NameChangeEvent(this, oldName, newName));
			plugin.OnKick += (kickee, channel, kicker, reason) => eventManager.HandleKick(new KickEvent(this, kickee, channel, kicker, reason));
			plugin.OnKicked += (channel, kicker, reason) => eventManager.HandleKicked(new KickedEvent(this, channel, kicker, reason));
			plugin.OnJoinChannel += (user, channel) => eventManager.HandleJoin(new JoinEvent(this, user, channel));
			plugin.OnPartChannel += (user, channel) => eventManager.HandlePart(new PartEvent(this, user, channel));
			plugin.OnQuit += (user, reason) => eventManager.HandleQuit(new QuitEvent(this, user, reason));
		}

		/// <summary>
		/// Connects the client to the SQL database.
		/// </summary>
		private SqlConnector ConnectDatabase(Backend backend)
		{
			var sqlConnector = new SqlConnector();
			Logger.Log(this, "Connecting to the database", LogLevel.Info);
			try
			{
				if (sqlConnector.OpenConnection(backend.ConnectionString))
					Logger.Log(this, "Database connection established", LogLevel.Info);
				else
					Logger.Log(this, "Database connection not established. Statistics collection will not be possible.", LogLevel.Warning);
			}
			catch (Exception e)
			{
				Logger.LogException(this, e, "trying to connect to the database");
			}
			return sqlConnector;
		}

		public bool Validate(ChatUser user)
		{
			Logger.Log(null, "Validating user");
			return Operators.Any(op => StatsDatabase.Validate(user, op));
		}


		public void Dispose()
		{
			plugin?.Dispose();
			StatsDatabase?.Dispose();
		}

		public MessageSendResult SendMessage(ChatChannel target, string message)
		{
			message = plugin.MessageFormatters.Aggregate(message, (current, formatter) => formatter.ProcessOutgoingMessage(current));
			return plugin.SendMessage(target, message);
		}

		public MessageSendResult Reply(ChatChannel channel, ChatUser user, string message)
		{
			message = plugin.MessageFormatters.Aggregate(message, (current, formatter) => formatter.ProcessOutgoingMessage(current));
			return plugin.Reply(channel, user, message);
		}
	}
}
