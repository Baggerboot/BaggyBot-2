using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Plugins;
using IRCSharp.IrcCommandProcessors.Quirks;
using IrcClient = IRCSharp.IrcClient;
using NickservInformation = IRCSharp.IRC.NickservInformation;
using ConnectionInfo = IRCSharp.ConnectionInfo;

namespace BaggyBot.InternalPlugins.Irc
{
	/// <summary>
	/// Provides a light wrapper over an IrcClient object, exposing only
	/// the methods required for normal operation.
	/// </summary>
	public class IrcPlugin : Plugin
	{
		public override string ServerType => "irc";

		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event NameChangeEvent OnNameChange;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event ConnectionLostEvent OnConnectionLost;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;

		private readonly IrcClient client;
		public override IReadOnlyList<ChatChannel> Channels { get; }
		public override bool Connected => client.Connected;
		private readonly ServerCfg serverCfg;


		internal IrcPlugin(ServerCfg config) : base(config)
		{
			client = new IrcClient();
			client.OnNetLibDebugLog += (sender, message) => Logger.Log(sender, "[NL#]" + message, LogLevel.Info);
			serverCfg = config;
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			switch (client.SendMessage(target.Identifier, message))
			{
				case IRCSharp.MessageSendResult.Success:
					return MessageSendResult.Success;
				case IRCSharp.MessageSendResult.Failure:
					return MessageSendResult.Success;
				case IRCSharp.MessageSendResult.FloodLimitHit:
					return MessageSendResult.FloodLimitHit;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		public override bool JoinChannel(ChatChannel channel)
		{
			return client.JoinChannel(channel.Identifier);
		}

		public bool JoinChannels(IEnumerable<string> channels)
		{
			return client.JoinChannels(channels);
		}

		public NickservInformation NickservLookup(string nick)
		{
			Logger.Log(this, $"Performing NickServ lookup for {nick}.");
			return client.NickservLookup(nick);
		}

		public ChatUser DoWhoisCall(string nick)
		{
			var ircUser = client.Whois(nick);
			// TODO: create IRCUser subclass of ChatUser
			throw new NotImplementedException();
			//return new ChatUser(this, );
		}

		public void Reconnect()
		{
			// TODO: Implement reconnect
			throw new NotImplementedException();
		}

		public override void Part(ChatChannel channel, string reason = null)
		{
			client.Part(channel.Identifier, reason);
		}

		public override void Quit(string reason)
		{
			client.Quit(reason);
		}

		public override void Dispose()
		{
			StatsDatabase.Dispose();
		}

		public override ChatUser FindUser(string name)
		{
			throw new NotImplementedException();
		}

		public override bool Connect()
		{
			var host = serverCfg.Host;
			var port = serverCfg.Port;
			var nick = serverCfg.Identity.Nick;
			var ident = serverCfg.Identity.Ident;
			var realname = serverCfg.Identity.RealName;
			var password = serverCfg.Password;
			var tls = serverCfg.UseTls;
			var verify = serverCfg.VerifyCertificate;
			var slackCompatMode = serverCfg.CompatModes.Contains("slack");

			// The slack IRC server messes up domain names, because it prepends
			// http://<domain> to everything that looks like it's a domain name
			// or an incorrectly formatted URL, despite the fact that this sort
			// of formatting should really be done by IRC clients, not servers.
			// BaggyBot doesn't like this; it messes up several of his commands
			// as they'll mistakenly consider the URL added by Slack as another
			// argument. The workaround for this is to use a compatibility mode
			// for Slack, by adding a message processor to the IRC client which
			// strips all the formatting added by Slack before it is processed.
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
			return true;
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}
	}
}
