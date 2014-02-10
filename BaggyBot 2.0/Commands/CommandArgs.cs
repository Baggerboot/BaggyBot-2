using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot.Commands
{
	public struct CommandArgs
	{
		public string Command;
		public string[] Args;
		public IrcUser Sender;
		public string Channel;
		public string FullArgument;
		public Action<string, string> ReplyCallback;

		public CommandArgs(string command, string[] args, IrcUser sender, string channel, string fullArgument, Action<string, string> replyCallback)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
			ReplyCallback = replyCallback;
		}
		public void Reply(string format, params object[] args)
		{
			ReplyCallback(Channel, Sender.Nick + ", " + string.Format(format, args));
		}
		public void ReturnMessage(string format, params object[] args)
		{
			ReplyCallback(Channel, string.Format(format, args));
		}
	}
}
