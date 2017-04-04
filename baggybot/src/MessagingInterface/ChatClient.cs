using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.MessagingInterface.Handlers;
using BaggyBot.MessagingInterface.Handlers.Administration;
using BaggyBot.Monitoring;
using BaggyBot.Permissions;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	internal class ChatClient : IDisposable
	{
		public string ServerType => plugin.ServerType;
		public string ServerName => plugin.ServerName;
		public bool Connected => plugin.Connected;
		public IReadOnlyList<Operator> Operators => plugin.Operators;
		public IReadOnlyList<ChatChannel> Channels => plugin.Channels;
		public ServerCapabilities Capabilities => plugin.Capabilities;

		public void JoinChannel(ChatChannel channel) => plugin.Join(channel);
		public void Quit(string reason) => plugin.Quit(reason);
		public void Part(ChatChannel channel, string reason = null) => plugin.Part(channel, reason);
		public ChatChannel FindChannel(string name) => plugin.FindChannel(name);
		public ChatChannel GetChannel(string id) => plugin.GetChannel(id);
		public void NotifyOperators(string message) => plugin.NotifyOperators(message);
		public ChatUser Self => plugin.Self;

		public event Action<string, Exception> ConnectionLost;

		private readonly ChatClientEventManager eventManager;
		private readonly Plugin plugin;
		public ServerCfg Configuration;
		public StatsDatabaseManager StatsDatabase { get; }
		public PermissionsManager Permissions { get; }

		internal ChatClient(Plugin plugin, ServerCfg configuration)
		{
			this.plugin = plugin;
			Configuration = configuration;
			StatsDatabase = new StatsDatabaseManager(ConnectDatabase(configuration.Backend));
			Permissions = new PermissionsManager(StatsDatabase, Configuration.Operators);

			var handlers = new List<ChatClientEventHandler>
			{
				// This handler binds ChatUsers to database users and should therefore run first
				new BindHandler(),
				// Logs client events to stdout and optionally the log file
				new LogHandler(),
				// Generates statistics
				new StatsHandler(),
				// Performs channel-administrative actions
				new AdministrationHandler(),
				// Processes commands
				new CommandHandler()
			};

			// Allows sending messages through console input
			if(ConfigManager.Config.Input.Enabled) handlers.Add(new InputHandler());

			foreach (var handler in handlers)
			{
				handler.BindClient(this);
				handler.Initialise();
			}
			eventManager = new ChatClientEventManager(handlers);
			AttachEventHandlers();
		}

		public bool Connect()
		{
			var res = plugin.Connect();
			if (res)
			{
				eventManager.HandleConnectionEstablished();
			}
			return res;
		}

		/// <summary>
		/// Connects the client to the SQL database.
		/// </summary>
		private SqlConnector ConnectDatabase(Backend backend)
		{
			if (backend == null)
			{
				Logger.Log(this, "No database specified. Statistics collection will not be possible.", LogLevel.Warning);
				return null;
			}

			var sqlConnector = new SqlConnector();
			Logger.Log(this, "Connecting to the database...", LogLevel.Info);
			try
			{
				if (sqlConnector.OpenConnection(backend.ConnectionString))
					Logger.Log(this, "Integrity check successful, database ready.", LogLevel.Info);
				else
					Logger.Log(this, "Database connection not established. Statistics collection will not be possible.", LogLevel.Warning);
			}
			catch (Exception e)
			{
				Logger.LogException(this, e, "trying to connect to the database");
			}
			return sqlConnector;
		}

		private void AttachEventHandlers()
		{
			plugin.OnMessageReceived += message =>
			{
				foreach (var formatter in plugin.MessageFormatters)
				{
					message = formatter.ProcessIncomingMessage(message);
				}
				eventManager.HandleMessage(new MessageEvent(message,
				                                            reply => Reply(message.Channel, message.Sender, reply),
				                                            reply => SendMessage(message.Channel, reply)));
			};
			plugin.OnNameChange += (oldName, newName) => eventManager.HandleNameChange(new NameChangeEvent(oldName, newName));
			plugin.OnKick += (kickee, channel, kicker, reason) => eventManager.HandleKick(new KickEvent(kickee, channel, kicker, reason));
			plugin.OnKicked += (channel, kicker, reason) => eventManager.HandleKicked(new KickedEvent(channel, kicker, reason));
			plugin.OnJoin += (user, channel) => eventManager.HandleJoin(new JoinEvent(user, channel));
			plugin.OnPart += (user, channel) => eventManager.HandlePart(new PartEvent(user, channel));
			plugin.OnQuit += (user, reason) => eventManager.HandleQuit(new QuitEvent(user, reason));
			plugin.OnConnectionLost += ConnectionLost;
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

		internal MessageSendResult SendMessage(ChatUser user, string message)
		{
			message = plugin.MessageFormatters.Aggregate(message, (current, formatter) => formatter.ProcessOutgoingMessage(current));
			return plugin.SendMessage(user, message);
		}

		public MessageSendResult Reply(ChatChannel channel, ChatUser user, string message)
		{
			message = plugin.MessageFormatters.Aggregate(message, (current, formatter) => formatter.ProcessOutgoingMessage(current));
			return plugin.Reply(channel, user, message);
		}

		public IEnumerable<ChatMessage> GetBacklog(ChatChannel channel, DateTime before, DateTime after)
		{
			return plugin.GetBacklog(channel, before, after);
		}

		public bool IncludeChannel(ChatChannel channel)
		{
			if (Configuration.ExcludeChannels.Contains(channel.Identifier)) return false;
			return Configuration.IncludeChannels.Length == 0 || Configuration.IncludeChannels.Contains(channel.Identifier);
		}

		public ChatUser GetUser(string id)
		{
			return plugin.GetUser(id);
		}

		public ChatUser FindUser(string nickname)
		{
			return plugin.FindUser(nickname);
		}

		public void Reconnect()
		{
			ConnectionLost?.Invoke("Manual reconnect.", null);
		}

		public void Delete(ChatMessage message)
		{
			plugin.Delete(message);
		}

		public void Kick(ChatUser chatUser, ChatChannel channel)
		{
			plugin.Kick(chatUser, channel);
		}

		public void Ban(ChatUser chatUser, ChatChannel channel)
		{
			plugin.Ban(chatUser, channel);
		}

		public string GetMentionString(ChatUser user)
		{
			return plugin.GetMentionString(user);
		}
	}
}
