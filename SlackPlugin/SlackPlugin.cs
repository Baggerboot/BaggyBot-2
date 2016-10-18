using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using IRCSharp;
using IRCSharp.IRC;

namespace SlackPlugin
{
	public class SlackPlugin :IPlugin
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public event DebugLogEvent OnDebugLog;
		public event DebugLogEvent OnNetLibDebugLog;
		public event FormattedLineReceivedEvent OnFormattedLineReceived;
		public event MessageReceivedEvent OnMessageReceived;
		public event NickChangeEvent OnNickChange;
		public event KickEvent OnKick;
		public event KickedEvent OnKicked;
		public event DisconnectedEvent OnDisconnect;
		public event QuitEvent OnQuit;
		public event JoinChannelEvent OnJoinChannel;
		public event PartChannelEvent OnPartChannel;
		public event NoticeReceivedEvent OnNoticeReceived;

		public IReadOnlyList<IrcChannel> Channels { get; }
		public bool Connected { get; }
		public string ServerName { get; }
		public StatsDatabaseManager StatsDatabase { get; set; }
		public IrcChannel GetChannel(string name)
		{
			throw new NotImplementedException();
		}

		public bool InChannel(string channel)
		{
			throw new NotImplementedException();
		}

		public MessageSendResult SendMessage(string target, string message)
		{
			throw new NotImplementedException();
		}

		public bool JoinChannel(string channel)
		{
			throw new NotImplementedException();
		}

		public bool JoinChannels(IEnumerable<string> channels)
		{
			throw new NotImplementedException();
		}

		public NickservInformation NickservLookup(string nick)
		{
			throw new NotImplementedException();
		}

		public ChatUser DoWhoisCall(string nick)
		{
			throw new NotImplementedException();
		}

		public void Reconnect()
		{
			throw new NotImplementedException();
		}

		public void Part(string channel, string reason = null)
		{
			throw new NotImplementedException();
		}

		public void Quit(string reason)
		{
			throw new NotImplementedException();
		}

		public bool Connect()
		{
			throw new NotImplementedException();
		}

		public string ServerType { get; }
		public void Disconnect()
		{
			throw new NotImplementedException();
		}
	}
}
