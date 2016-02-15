using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Database;
using BaggyBot.Monitoring;
using IRCSharp;
using IRCSharp.IRC;

namespace BaggyBot.MessagingInterface
{
	/// <summary>
	/// Provides a light wrapper over an <see cref="IrcClient"/>, exposing only
	/// the methods required for normal operation.
	/// </summary>
	public class IrcClientWrapper : IDisposable
	{
		private readonly IrcClient client;
		public IReadOnlyList<IrcChannel> Channels => client.Channels;
		public string ServerName { get; }
		public StatsDatabaseManager StatsDatabase { get; }

		internal IrcClientWrapper(IrcClient client, StatsDatabaseManager database, string serverName)
		{
			ServerName = serverName;
			StatsDatabase = database;
			this.client = client;
		}

		public IrcChannel GetChannel(string name)
		{
			return Channels.FirstOrDefault(c => c.Name == name);
		}

		public bool InChannel(string channel)
		{
			return Channels.Any(c => c.Name == channel);
		}

		public MessageSendResult SendMessage(string target, string message)
		{
			return client.SendMessage(target, message);
		}
		public bool JoinChannel(string channel)
		{
			return client.JoinChannel(channel);
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

		internal IrcUser DoWhoisCall(string nick)
		{
			return new IrcUser(this, client.Whois(nick));
		}

		public void Reconnect()
		{
			// TODO: Implement reconnect
			throw new NotImplementedException();
		}

		public void Part(string channel, string reason = null)
		{
			client.Part(channel, reason);
		}

		public void Quit(string reason)
		{
			client.Quit(reason);
		}

		public void Dispose()
		{
			StatsDatabase.Dispose();
		}
	}
}
