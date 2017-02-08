using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using IRCSharp;
using IRCSharp.IrcCommandProcessors.Quirks;
using IRCSharp.IRC;

namespace BaggyBot.Plugins.Internal.Irc
{
	/// <summary>
	/// Provides a light wrapper over an IrcClient object, exposing only
	/// the methods required for normal operation.
	/// </summary>
	public class IrcPlugin : Plugin
	{
		public override string ServerType => "irc";

		public override event Action<ChatMessage> OnMessageReceived;
		public override event Action<ChatUser, ChatUser> OnNameChange;
		public override event Action<ChatUser, ChatChannel, ChatUser, string> OnKick;
		public override event Action<ChatChannel, ChatUser, string> OnKicked;
		public override event Action<string, Exception> OnConnectionLost;
		public override event Action<ChatUser, string> OnQuit;
		public override event Action<ChatUser, ChatChannel> OnJoinChannel;
		public override event Action<ChatUser, ChatChannel> OnPartChannel;

		private readonly IrcClient client;
		public override IReadOnlyList<ChatChannel> Channels { get; protected set; }
		public override bool Connected => client.Connected;
		private readonly ServerCfg serverCfg;

		// These IRC commands are not handled in any way, as the information contained in them
		// is not considered useful for the bot.
		private readonly string[] ignoredCommands =
		{
			"004" /*RPL_MYINFO*/,
			"005" /*RPL_ISUPPORT*/,
			"251" /*RPL_LUSERCLIENT*/,
			"254" /*RPL_LUSERCHANNELS*/,
			"252" /*RPL_LUSEROP*/,
			"255" /*RPL_LUSERME*/,
			"265" /*RPL_LOCALUSERS*/,
			"266" /*RPL_GLOBALUSERS*/,
			"250" /*RPL_STATSCONN*/
		};


		internal IrcPlugin(ServerCfg config) : base(config)
		{
			client = new IrcClient();
			client.OnNetLibDebugLog += (sender, message) => Logger.Log(sender, "[NL#]" + message, LogLevel.Info);
			client.OnMessageReceived += MessageReceivedHandler;
			serverCfg = config;
		}

		private void MessageReceivedHandler(IrcMessage message)
		{
			var sender = new ChatUser(message.Sender.Nick, message.Sender.ToString(), false);
			var channel = new ChatChannel(message.Channel);
			OnMessageReceived?.Invoke(new ChatMessage(sender, channel, message.Message, message.Action));
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
		public override void JoinChannel(ChatChannel channel)
		{
			client.JoinChannel(channel.Identifier);
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



		/// <summary>
		/// Custom code for checking whether a user has registered with NickServ. Ugly, but it works.
		/// </summary>
		private void ProcessFormattedLine(IrcLine line)
		{
			//TODO: where's the nickserv code gone?
			// This IRC server does not have a NickServ service.
			/*if (line.Command.Equals("401") && ircInterface.HasNickservCall && line.FinalArgument.ToLower().Contains("nickserv: no such nick"))
			{
				ircInterface.DisableNickservCalls();
				// Proess reply to WHOIS call.
			}*/
			if (!ignoredCommands.Contains(line.Command))
			{
				// TODO: handle formatted lines
				switch (line.Command)
				{
					case "001": // RPL_WELCOME
					case "002": // RPL_YOURHOST
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "003": // RPL_CREATED
						Logger.Log(this, $"{line.Sender}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "332": // RPL_TOPIC
						Logger.Log(this, $"Topic for {line.Arguments[1]}: {line.FinalArgument}", LogLevel.Irc);
						break;
					case "333": // Ignore names list
					case "366":
						break;
					case "253": // RPL_LUSERUNKNOWN
						break;
					case "375": // RPL_MOTDSTART
					case "376": // RPL_ENDOFMOTD
					case "372": // RPL_MOTD
					case "451": // ERR_NOTREGISTERED
						Logger.Log(this, $"{line.FinalArgument}", LogLevel.Irc);
						break;
					case "MODE":
						// TODO: Figure out the difference between these two and document it. Probably channel/user
						if (line.FinalArgument != null)
						{
							Logger.Log(this, $"{line.Sender} sets mode {line.FinalArgument} for {line.Arguments[0]}", LogLevel.Irc);
						}
						else
						{
							Logger.Log(this, $"{line.Sender} sets mode {line.Arguments[1]} for {line.Arguments[0]}");
						}
						break;
					default:
						Debugger.Break();
						Logger.Log(this, line.ToString(), LogLevel.Irc);
						break;
				}
			}
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}
	}
}
