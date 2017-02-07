using System;
using System.Collections.Generic;
using BaggyBot.Configuration;
using BaggyBot.Handlers;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	internal class ChatClientManager : IDisposable
	{
		private readonly Dictionary<string, ChatClient> clients = new Dictionary<string, ChatClient>();
		internal ChatClient this[string identifier] => clients[identifier];

		public ChatClientManager()
		{

		}

		public bool ConnectUsingPlugin(ServerCfg serverConfiguration, Plugin plugin)
		{
			Logger.Log(this, $"Connecting plugin: {plugin.ServerType}:{plugin.ServerName}");


			var client = new ChatClient(plugin, serverConfiguration);
			var result = client.Connect();
			if (!result)
			{
				Logger.Log(this, $"Unable to connect to {plugin.ServerType}:{plugin.ServerName}", LogLevel.Error);
				return false;
			}
			Logger.Log(this, "Plugin connection successful.");
			try
			{
				clients.Add(serverConfiguration.ServerName, client);
			}
			catch (ArgumentException)
			{
				Logger.Log(this, "Failed to add the chat server: a serverConfiguration with the same name already exists.", LogLevel.Error);
				plugin.Disconnect();
				return false;
			}
			return true;
		}

		private void HandleConnectionLoss(ChatClient client, string reason, Exception ex)
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
			client.Dispose();

			throw new NotImplementedException("Reconnect is not implemented yet ");

			Logger.Log(this, $"Successfully reconnected to {client.ServerName}.", LogLevel.Warning);
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
						client.SendMessage(new ChatChannel(op.Nick, true), message);
					}
				}
			}
		}
	}
}
