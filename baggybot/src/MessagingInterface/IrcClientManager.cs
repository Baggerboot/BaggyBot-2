using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.DataProcessors;
using BaggyBot.Monitoring;
using IRCSharp;
using IRCSharp.IrcCommandProcessors.Quirks;

namespace BaggyBot.MessagingInterface
{
	class IrcClientManager
	{
		private readonly IrcEventHandler ircEventHandler;
		private readonly Dictionary<string, IrcClientWrapper> clients = new Dictionary<string, IrcClientWrapper>();

		internal IrcClientWrapper this[string identifier] => clients[identifier];

		public IrcClientManager(IrcEventHandler ircEventHandler)
		{
			this.ircEventHandler = ircEventHandler;
		}

		/// <summary>
		/// Connects the bot to the IRC server
		/// </summary>
		public bool ConnectIrc(ServerCfg server)
		{
			var client = new IRCSharp.IrcClient();
			var wrapper = new IrcClientWrapper(client);
			HookupIrcEvents(client, wrapper, server.ServerName);

			var host = server.Host;
			var port = server.Port;
			var nick = server.Identity.Nick;
			var ident = server.Identity.Ident;
			var realname = server.Identity.RealName;
			var password = server.Password;
			var tls = server.UseTls;
			var verify = server.VerifyCertificate;
			var slackCompatMode = server.CompatModes.Contains("slack");

			// Slack IRC messes up domain names by prepending http://<domain> to everything that looks like a domain name or incorrectly formatted URL,
			// despite the fact that such formatting should really be done by IRC clients, not servers.
			// BaggyBot doesn't like this; it messes up several of his commands, such as -resolve and -ping.
			// Therefore, when Slack Compatibility Mode is enabled, we add a SlackIrcProcessor which strips these misformatted domain names from the message.
			if (slackCompatMode)
			{
				client.DataProcessors.Add(new SlackIrcProcessor());
			}

			Logger.Log(this, "Connecting to the IRC server..");
			Logger.Log(this, "\tNick\t" + nick);
			Logger.Log(this, "\tIdent\t" + ident);
			Logger.Log(this, "\tName\t" + realname);
			Logger.Log(this, "\tHost\t" + host);
			Logger.Log(this, "\tPort\t" + port);
			try
			{
				client.Connect(new ConnectionInfo
				{
					Host = host,
					Port = port,
					Nick = nick,
					Ident = ident,
					Password = password,
					RealName = realname,
					useTLS = tls,
					verifyServerCertificate = verify
				});

				clients.Add(server.ServerName, wrapper);
				return true;
			}
			catch (SocketException e)
			{
				Logger.Log(this, "Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			}
			catch (ArgumentException e)
			{
				Logger.Log(this, $"Failed to connect to the IRC server: The settings file does not contain a value for \"{e.ParamName}\"", LogLevel.Error);
				return false;
			}
			catch (Exception e)
			{
				Logger.Log(this, "Failed to connect to the IRC server: " + e.Message, LogLevel.Error);
				return false;
			}
		}

		private void HookupIrcEvents(IrcClient client, IrcClientWrapper wrapper, string serverName)
		{
			client.OnNickChange += (sender, newNick) => ircEventHandler.HandleNickChange(new IrcUser(wrapper, sender), newNick);
			client.OnMessageReceived += message => Task.Run(() =>
			{
				try
				{
					ircEventHandler.ProcessMessage(new IrcMessage(wrapper, message));
				}
				catch (Exception e)
				{
					Logger.LogException(this, e, "processing a message");
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}
				}
			});
			client.OnFormattedLineReceived += ircEventHandler.ProcessFormattedLine;
			client.OnNoticeReceived += (sender, notice) => ircEventHandler.ProcessNotice(new IrcUser(wrapper, sender), notice);
			client.OnDisconnect += (reason, exception) => HandleDisconnect(client, serverName, reason, exception);
			client.OnKicked += (channel, reason, kicker) => Logger.Log(this, $"I was kicked from {channel} by {kicker.Nick} ({reason})", LogLevel.Warning);
			client.OnDebugLog += (sender, message) => Logger.Log(sender, "[IC#]" + message, LogLevel.Info);
			client.OnNetLibDebugLog += (sender, message) => Logger.Log(sender, "[NL#]" + message, LogLevel.Info);
			client.OnJoinChannel += (user, channel) => ircEventHandler.HandleJoin(new IrcUser(wrapper, user), channel);
			client.OnPartChannel += (user, channel) => ircEventHandler.HandlePart(new IrcUser(wrapper, user), channel);
			client.OnKick += (kickee, channel, reason, kicker) => ircEventHandler.HandleKick(kickee, channel, reason, new IrcUser(wrapper, kicker));
			client.OnQuit += (user, channel) => ircEventHandler.HandleQuit(new IrcUser(wrapper, user), channel);
		}

		private void HandleDisconnect(IrcClient client, string serverName, DisconnectReason reason, Exception ex)
		{
			if (reason == DisconnectReason.DisconnectOnRequest)
			{
				Logger.Log(this, $"Disconnected from an IRC server ({serverName}).", LogLevel.Info);
			}
			else
			{
				var lostChannels = client.Channels;

				if (reason == DisconnectReason.Other)
				{
					Logger.Log(client, $"Connection to {serverName} lost ({ex.GetType()}: {ex.Message}) Attempting to reconnect...", LogLevel.Error);
				}
				else
				{
					Logger.Log(client, $"Connection to {serverName} lost ({reason}) Attempting to reconnect...", LogLevel.Warning);
				}
				bool success;

				do
				{
					success = ConnectIrc(ConfigManager.Config.Servers.First(server => server.ServerName == serverName));
					if (success) continue;
					Logger.Log(this, "Reconnection attempt failed. Retrying in 5 seconds.", LogLevel.Warning);
					Thread.Sleep(5000);
				} while (!success);

				Logger.Log(this, $"Successfully reconnected to {serverName}!", LogLevel.Warning);

				foreach (var channel in lostChannels)
				{
					if (!client.JoinChannel(channel.Name, true))
					{
						Logger.Log(this, $"Failed to rejoin {channel} on {serverName}! Skipping this channel.", LogLevel.Error);
					}
				}
			}
		}

		public void Disconnect(string reason)
		{
			foreach (var client in clients)
			{
				client.Value.Quit(reason);
			}
		}
	}
}
