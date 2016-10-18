using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;
using IRCSharp.IRC;

namespace BaggyBot.Plugins
{
	public delegate void DebugLogEvent(object sender, string message);
	public delegate void MessageReceivedEvent(ChatMessage message);
	public delegate void NameChangeEvent(ChatUser oldName, ChatUser newName);
	public delegate void KickEvent(ChatUser kickee, ChatChannel channel, string reason, ChatUser kicker);
	public delegate void KickedEvent(ChatChannel channel, string reason, ChatUser kicker);
	public delegate void ConnectionLostEvent(string reason, Exception e);
	public delegate void QuitEvent(ChatUser user, string reason);
	public delegate void JoinChannelEvent(ChatUser user, ChatChannel channel);
	public delegate void PartChannelEvent(ChatUser user, ChatChannel channel);
}
