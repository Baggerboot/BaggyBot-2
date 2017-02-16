using System;
using System.Collections.Generic;
using BaggyBot.Configuration;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	internal class ChatClientManager : IDisposable
	{
		private readonly Dictionary<string, ChatClient> clients = new Dictionary<string, ChatClient>();
		internal ChatClient this[string identifier] => clients[identifier];


		public bool ConnectUsingPlugin(Type pluginType, ServerCfg configuration)
		{
			Plugin plugin;
			try
			{
				plugin = (Plugin)Activator.CreateInstance(pluginType, configuration);
			}
			catch (Exception e)
			{
				Logger.Log(this, $"Unable to connect to {configuration.ServerName}: An exception occured while creating the plugin ({e.GetType().Name}: {e.Message})");
				return false;
			}
			return ConnectUsingPlugin(plugin, configuration);
		}

		private bool ConnectUsingPlugin(Plugin plugin, ServerCfg configuration)
		{
			Logger.Log(this, $"Connecting plugin: {plugin.ServerType}:{plugin.ServerName}");
			var client = new ChatClient(plugin, configuration);

			// If the plugin loses connection, we need to dispose of all the state associated with it.
			client.ConnectionLost += (_, __) =>
			{
				clients.Remove(client.ServerName);
				client.Dispose();
			};
			// When that's done, we can try reconnecting.
			client.ConnectionLost += (message, exception) => HandleConnectionLoss(plugin.GetType(), configuration, message, exception);

			var result = client.Connect();
			if (!result)
			{
				Logger.Log(this, $"Unable to connect to {plugin.ServerType}:{plugin.ServerName}", LogLevel.Error);
				return false;
			}
			Logger.Log(this, "Plugin connection successful.");
			try
			{
				clients.Add(configuration.ServerName, client);
			}
			catch (ArgumentException)
			{
				Logger.Log(this, "Failed to add the chat server: a serverConfiguration with the same name already exists.", LogLevel.Error);
				plugin.Disconnect();
				return false;
			}
			return true;
		}

		private void HandleConnectionLoss(Type plugin, ServerCfg configuration, string reason, Exception ex)
		{
			if (reason == null)
			{
				if (ex == null)
				{
					Logger.Log(this, $"Connection to {configuration.ServerName} lost. Attempting to reconnect...", LogLevel.Error);
				}
				else
				{
					Logger.Log(this, $"Connection to {configuration.ServerName} lost ({ex.GetType()}: {ex.Message}) Attempting to reconnect...", LogLevel.Error);
				}
			}
			else
			{
				Logger.Log(this, $"Connection to {configuration.ServerName} lost ({reason}) Attempting to reconnect...", LogLevel.Warning);
			}

			if (ConnectUsingPlugin(plugin, configuration))
			{
				Logger.Log(this, $"Successfully reconnected to {configuration.ServerName}.", LogLevel.Warning);
			}
			else
			{
				Logger.Log(this, $"Unable to reconnect to {configuration.ServerName}.", LogLevel.Error);
			}
		}

		public void Disconnect(string reason)
		{
			foreach (var client in clients.Values)
			{
				client.Quit(reason);
			}
		}

		public void Dispose()
		{
			foreach (var client in clients.Values)
			{
				client.Dispose();
			}
		}

		public void NotifyOperators(string message)
		{
			foreach (var client in clients.Values)
			{
				if (client.Connected)
				{
					foreach (var op in client.Operators)
					{
						ChatUser opUser;
						if (op.UniqueId != "*")
						{
							opUser = client.GetUser(op.UniqueId);
						}
						else if (op.Uid != "*")
						{
							opUser = client.GetUser(client.StatsDatabase.GetUserById(int.Parse(op.Uid)).UniqueId);
						}
						else
						{
							opUser = client.FindUser(op.Nick);
						}
						client.SendMessage(opUser, message);
					}
				}
			}
		}
	}
}
