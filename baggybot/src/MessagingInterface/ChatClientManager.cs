using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.DataProcessors;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	internal class ChatClientManager : IDisposable
	{
		private readonly Dictionary<string, Plugin> clients = new Dictionary<string, Plugin>();
		private readonly ChatClientEventHandlerManager chatClientEventHandler = new ChatClientEventHandlerManager();
		internal IChatClient this[string identifier] => clients[identifier];

		/// <summary>
		/// Connects the bot to the SQL database.
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

		public bool ConnectUsingPlugin(ServerCfg serverConfiguration, Plugin plugin)
		{
			Logger.Log(this, $"Connecting plugin: {plugin.ServerType}:{plugin.ServerName}");

			var statsDatabaseManager = new StatsDatabaseManager(ConnectDatabase(serverConfiguration.Backend), serverConfiguration.UseNickserv);
			// TODO: Associate the StatsDatabase with the plugin in a different manner
			plugin.StatsDatabase = statsDatabaseManager;

			AttachEventHandlers(plugin);

			Logger.Log(this, "Waiting for the plugin to connect to its server..");
			var result = plugin.Connect();
			if (!result)
			{
				Logger.Log(this, $"Unable to connect to {plugin.ServerType}:{plugin.ServerName}", LogLevel.Error);
				return false;
			}
			Logger.Log(this, "Plugin connection successful.");
			try
			{
				clients.Add(serverConfiguration.ServerName, plugin);
			}
			catch (ArgumentException)
			{
				Logger.Log(this, "Failed to add the chat server: a serverConfiguration with the same name already exists.", LogLevel.Error);
				plugin.Disconnect();
				return false;
			}
			return true;
		}

		private void AttachEventHandlers(Plugin plugin)
		{
			plugin.OnNameChange += (sender, newNick) => chatClientEventHandler.HandleNickChange(sender, newNick);
			plugin.OnMessageReceived += message => Task.Run(() =>
			{
				foreach (var formatter in plugin.MessageFormatters)
				{
					formatter.ProcessIncomingMessage(message);
				}
				chatClientEventHandler.HandleMessage(message);
			});
			plugin.OnConnectionLost += (reason, exception) => HandleConnectionLoss(plugin, reason, exception);
			plugin.OnKicked += (channel, reason, kicker) => Logger.Log(this, $"I was kicked from {channel} by {kicker.Nickname} ({reason})", LogLevel.Warning);
			plugin.OnDebugLog += (sender, message) => Logger.Log(sender, "[PLG]" + message, LogLevel.Warning);
			plugin.OnJoinChannel += chatClientEventHandler.HandleJoin;
			plugin.OnPartChannel += chatClientEventHandler.HandlePart;
			plugin.OnKick += chatClientEventHandler.HandleKick;
			plugin.OnQuit += chatClientEventHandler.HandleQuit;
		}

		private void HandleConnectionLoss(Plugin client, string reason, Exception ex)
		{
			if (reason == null)
			{
				if (ex == null)
				{
					Logger.Log(client, $"Connection to {client.ServerName} lost. Attempting to reconnect...", LogLevel.Error);
				}
				else
				{
					Logger.Log(client, $"Connection to {client.ServerName} lost ({ex.GetType()}: {ex.Message}) Attempting to reconnect...", LogLevel.Error);
				}
			}
			else
			{
				Logger.Log(client, $"Connection to {client.ServerName} lost ({reason}) Attempting to reconnect...", LogLevel.Warning);
			}
			bool success;

			do
			{
				success = ConnectUsingPlugin(ConfigManager.Config.Servers.First(server => server.ServerName == client.ServerName), client);
				if (success) continue;
				Logger.Log(this, "Reconnection attempt failed. Retrying in 5 seconds.", LogLevel.Warning);
				Thread.Sleep(5000);
			} while (!success);

			Logger.Log(this, $"Successfully reconnected to {client.ServerName}.", LogLevel.Warning);
		}

		public void Disconnect(string reason)
		{
			foreach (var client in clients)
			{
				client.Value.Quit(reason);
			}
		}

		public void Dispose()
		{
			foreach (var client in clients)
			{
				client.Value.Dispose();
			}
		}

		public void NotifyOperators(string message)
		{
			foreach (var client in clients)
			{
				if (client.Value.Connected)
				{
					foreach (var op in client.Value.Operators)
					{
						client.Value.SendMessage(new ChatChannel(op.Nick, true), message);
					}
				}
			}
		}
	}
}
